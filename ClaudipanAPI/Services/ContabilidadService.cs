using AutoMapper;
using ClaudipanAPI.Data;
using ClaudipanAPI.Interfaces;
using ClaudipanAPI.Models.DTOs;
using ClaudipanAPI.Models.Responses;
using Microsoft.EntityFrameworkCore;

namespace ClaudipanAPI.Services;

public class ContabilidadService : IContabilidadService
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public ContabilidadService(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<ApiResponse<ResumenContableDto>> GetResumenContableAsync()
    {
        var pedidos = await _context.Pedidos
            .Where(p => p.Estado == "Entregado")
            .ToListAsync();

        var totalVentas = pedidos.Sum(p => p.Total);
        var totalCobrado = pedidos.Where(p => p.EstadoPago == "Pagado").Sum(p => p.Total);

        var carteraPorCobrar = await _context.Usuarios
            .Where(u => u.Activo)
            .SumAsync(u => u.DeudaActual);

        var totalCompras = await _context.Compras.SumAsync(c => c.Total);

        var gastos = await _context.Gastos.ToListAsync();
        var gastosServicios = gastos.Where(g => g.TipoGasto == "ServicioPublico").Sum(g => g.Monto);
        var gastosNomina = gastos.Where(g => g.TipoGasto == "Nomina").Sum(g => g.Monto);
        var otrosGastos = gastos.Where(g => g.TipoGasto != "ServicioPublico" && g.TipoGasto != "Nomina").Sum(g => g.Monto);

        var totalPerdidasBajas = await _context.BajasProductos.SumAsync(b => b.CostoPerdidaTotal);

        var utilidadBruta = totalVentas - totalCompras;
        var utilidadNeta = utilidadBruta - (gastosServicios + gastosNomina + otrosGastos) - totalPerdidasBajas;

        var clientesConDeuda = await _context.Usuarios
            .CountAsync(u => u.Activo && u.DeudaActual > 0);

        var dto = new ResumenContableDto
        {
            TotalVentas = totalVentas,
            TotalCobrado = totalCobrado,
            CarteraPorCobrar = carteraPorCobrar,
            TotalComprasProveedores = totalCompras,
            TotalGastosServicios = gastosServicios,
            TotalGastosNomina = gastosNomina,
            TotalOtrosGastos = otrosGastos,
            TotalPerdidasBajas = totalPerdidasBajas,
            UtilidadBruta = utilidadBruta,
            UtilidadNeta = utilidadNeta,
            TotalPedidos = pedidos.Count,
            TotalClientesConDeuda = clientesConDeuda
        };

        return ApiResponse<ResumenContableDto>.Ok(dto);
    }

    public async Task<ApiResponse<EstadoResultadosDto>> GetEstadoResultadosAsync(DateTime? fechaInicio = null, DateTime? fechaFin = null)
    {
        var pedidosQuery = _context.Pedidos
            .Include(p => p.Detalles)
                .ThenInclude(d => d.Producto)
            .Where(p => p.Estado == "Entregado");

        var gastosQuery = _context.Gastos.AsQueryable();
        var bajasQuery = _context.BajasProductos.AsQueryable();

        if (fechaInicio.HasValue)
        {
            pedidosQuery = pedidosQuery.Where(p => p.FechaPedido >= fechaInicio.Value);
            gastosQuery = gastosQuery.Where(g => g.FechaGasto >= fechaInicio.Value);
            bajasQuery = bajasQuery.Where(b => b.FechaBaja >= fechaInicio.Value);
        }

        if (fechaFin.HasValue)
        {
            pedidosQuery = pedidosQuery.Where(p => p.FechaPedido <= fechaFin.Value);
            gastosQuery = gastosQuery.Where(g => g.FechaGasto <= fechaFin.Value);
            bajasQuery = bajasQuery.Where(b => b.FechaBaja <= fechaFin.Value);
        }

        var pedidos = await pedidosQuery.ToListAsync();
        var gastos = await gastosQuery.ToListAsync();
        var bajas = await bajasQuery.ToListAsync();

        decimal ingresosVentas = pedidos.Sum(p => p.Total);
        decimal costoVentas = 0m;

        foreach (var p in pedidos)
        {
            foreach (var det in p.Detalles)
            {
                var costoUnit = det.Producto?.CostoBaseProduccion ?? 0m;
                if (costoUnit <= 0 && det.PrecioUnitario > 0)
                    costoUnit = det.PrecioUnitario * 0.55m; // Estimación 55% costo
                costoVentas += det.Cantidad * costoUnit;
            }
        }

        var gastosServicios = gastos.Where(g => g.TipoGasto == "ServicioPublico").Sum(g => g.Monto);
        var gastosNomina = gastos.Where(g => g.TipoGasto == "Nomina").Sum(g => g.Monto);
        var otrosGastos = gastos.Where(g => g.TipoGasto != "ServicioPublico" && g.TipoGasto != "Nomina").Sum(g => g.Monto);
        var perdidasBajas = bajas.Sum(b => b.CostoPerdidaTotal);

        var dto = new EstadoResultadosDto
        {
            IngresosVentas = ingresosVentas,
            CostoVentas = costoVentas,
            GastosServiciosPublicos = gastosServicios,
            GastosNomina = gastosNomina,
            OtrosGastosOperativos = otrosGastos,
            PerdidasBajasMermas = perdidasBajas
        };

        return ApiResponse<EstadoResultadosDto>.Ok(dto);
    }

    public async Task<ApiResponse<List<ReporteProductoRotacionDto>>> GetProductosMasVendidosAsync(int top = 10)
    {
        var productos = await _context.Productos
            .Include(p => p.Categoria)
            .Include(p => p.DetallesPedido)
                .ThenInclude(d => d.Pedido)
            .Include(p => p.Bajas)
            .ToListAsync();

        var ranking = productos
            .Select(p =>
            {
                var ventasValidas = p.DetallesPedido.Where(d => d.Pedido != null && d.Pedido.Estado != "Cancelado");
                var totalCantidad = ventasValidas.Sum(d => d.Cantidad);
                var totalDinero = ventasValidas.Sum(d => d.Subtotal);
                var totalBajas = p.Bajas.Sum(b => b.Cantidad);
                var perdidasBajas = p.Bajas.Sum(b => b.CostoPerdidaTotal);

                return new ReporteProductoRotacionDto
                {
                    ProductoId = p.Id,
                    Nombre = p.Nombre,
                    CategoriaNombre = p.Categoria?.Nombre ?? "General",
                    Precio = p.Precio,
                    CantidadVendida = totalCantidad,
                    TotalVentasGeneradas = totalDinero,
                    CantidadDadaDeBaja = totalBajas,
                    PerdidasGeneradas = perdidasBajas,
                    StockActual = p.Stock,
                    EstadoRotacion = totalCantidad > 20 ? "Alta_Rotacion" : "Normal"
                };
            })
            .OrderByDescending(r => r.CantidadVendida)
            .ThenByDescending(r => r.TotalVentasGeneradas)
            .Take(top)
            .ToList();

        return ApiResponse<List<ReporteProductoRotacionDto>>.Ok(ranking);
    }

    public async Task<ApiResponse<List<ReporteProductoRotacionDto>>> GetProductosConRezagoOPerdidasAsync()
    {
        var productos = await _context.Productos
            .Include(p => p.Categoria)
            .Include(p => p.DetallesPedido)
                .ThenInclude(d => d.Pedido)
            .Include(p => p.Bajas)
            .ToListAsync();

        var rezagos = productos
            .Select(p =>
            {
                var ventasValidas = p.DetallesPedido.Where(d => d.Pedido != null && d.Pedido.Estado != "Cancelado");
                var totalCantidad = ventasValidas.Sum(d => d.Cantidad);
                var totalDinero = ventasValidas.Sum(d => d.Subtotal);
                var totalBajas = p.Bajas.Sum(b => b.Cantidad);
                var perdidasBajas = p.Bajas.Sum(b => b.CostoPerdidaTotal);

                string estado = "Normal";
                if (totalBajas > 0) estado = "Con_Perdidas";
                else if (totalCantidad <= 2 && p.Stock > 10) estado = "Rezago";

                return new ReporteProductoRotacionDto
                {
                    ProductoId = p.Id,
                    Nombre = p.Nombre,
                    CategoriaNombre = p.Categoria?.Nombre ?? "General",
                    Precio = p.Precio,
                    CantidadVendida = totalCantidad,
                    TotalVentasGeneradas = totalDinero,
                    CantidadDadaDeBaja = totalBajas,
                    PerdidasGeneradas = perdidasBajas,
                    StockActual = p.Stock,
                    EstadoRotacion = estado
                };
            })
            .Where(r => r.CantidadDadaDeBaja > 0 || (r.CantidadVendida <= 3 && r.StockActual > 5))
            .OrderByDescending(r => r.PerdidasGeneradas)
            .ThenBy(r => r.CantidadVendida)
            .ToList();

        return ApiResponse<List<ReporteProductoRotacionDto>>.Ok(rezagos);
    }

    public async Task<ApiResponse<List<TransaccionDeudaDto>>> GetHistorialCreditosAsync(int? usuarioId = null)
    {
        var query = _context.TransaccionesDeuda
            .Include(t => t.Usuario)
            .AsQueryable();

        if (usuarioId.HasValue && usuarioId.Value > 0)
            query = query.Where(t => t.UsuarioId == usuarioId.Value);

        var list = await query.OrderByDescending(t => t.Fecha).ToListAsync();
        return ApiResponse<List<TransaccionDeudaDto>>.Ok(_mapper.Map<List<TransaccionDeudaDto>>(list));
    }

    public async Task<ApiResponse<MisDeudasResumenDto>> GetMisDeudasResumenAsync(int usuarioId)
    {
        var usuario = await _context.Usuarios.FindAsync(usuarioId);
        if (usuario == null)
            return ApiResponse<MisDeudasResumenDto>.Fail("Usuario no encontrado");

        var pedidos = await _context.Pedidos
            .Include(p => p.Usuario)
            .Include(p => p.Detalles)
                .ThenInclude(d => d.Producto)
            .Where(p => p.UsuarioId == usuarioId)
            .OrderByDescending(p => p.FechaPedido)
            .ToListAsync();

        var transacciones = await _context.TransaccionesDeuda
            .Include(t => t.Usuario)
            .Where(t => t.UsuarioId == usuarioId)
            .OrderByDescending(t => t.Fecha)
            .ToListAsync();

        var totalCompras = pedidos.Sum(p => p.Total);
        var totalFiado = pedidos
            .Where(p => p.TipoPago == "Credito_Fiado" || (p.TipoPago != null && p.TipoPago.ToLower().Contains("fiado")))
            .Sum(p => p.Total);
        var totalContado = totalCompras - totalFiado;

        var totalAbonos = transacciones
            .Where(t => t.Tipo == "Abono_Pago")
            .Sum(t => t.Monto);

        var cupoDisponible = Math.Max(0m, usuario.LimiteCredito - usuario.DeudaActual);

        var dto = new MisDeudasResumenDto
        {
            UsuarioId = usuario.Id,
            ClienteNombre = usuario.Nombre,
            LimiteCredito = usuario.LimiteCredito,
            DeudaActual = usuario.DeudaActual,
            CupoDisponible = cupoDisponible,
            TotalCompras = totalCompras,
            TotalComprasFiadas = totalFiado,
            TotalComprasContado = totalContado,
            TotalAbonos = totalAbonos,
            CantidadPedidos = pedidos.Count,
            Pedidos = _mapper.Map<List<PedidoDto>>(pedidos),
            Transacciones = _mapper.Map<List<TransaccionDeudaDto>>(transacciones)
        };

        return ApiResponse<MisDeudasResumenDto>.Ok(dto);
    }
}
