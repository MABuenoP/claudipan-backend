using AutoMapper;
using ClaudipanAPI.Data;
using ClaudipanAPI.Interfaces;
using ClaudipanAPI.Models.DTOs;
using ClaudipanAPI.Models.Entities;
using ClaudipanAPI.Models.Responses;
using Microsoft.EntityFrameworkCore;

namespace ClaudipanAPI.Services;

public class PedidoService : IPedidoService
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public PedidoService(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<ApiResponse<List<PedidoDto>>> GetAllAsync(int? usuarioId = null, string? estado = null, string? tipoPago = null)
    {
        var query = _context.Pedidos
            .Include(p => p.Usuario)
            .Include(p => p.Detalles)
                .ThenInclude(d => d.Producto)
            .AsQueryable();

        if (usuarioId.HasValue && usuarioId.Value > 0)
            query = query.Where(p => p.UsuarioId == usuarioId.Value);

        if (!string.IsNullOrWhiteSpace(estado))
            query = query.Where(p => p.Estado == estado);

        if (!string.IsNullOrWhiteSpace(tipoPago))
            query = query.Where(p => p.TipoPago == tipoPago);

        var list = await query.OrderByDescending(p => p.FechaPedido).ToListAsync();
        return ApiResponse<List<PedidoDto>>.Ok(_mapper.Map<List<PedidoDto>>(list));
    }

    public async Task<ApiResponse<PedidoDto>> GetByIdAsync(int id)
    {
        var pedido = await _context.Pedidos
            .Include(p => p.Usuario)
            .Include(p => p.Detalles)
                .ThenInclude(d => d.Producto)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (pedido == null) return ApiResponse<PedidoDto>.Fail("Pedido no encontrado");
        return ApiResponse<PedidoDto>.Ok(_mapper.Map<PedidoDto>(pedido));
    }

    public async Task<ApiResponse<PedidoDto>> CreateAsync(PedidoCreateDto dto)
    {
        if (dto.Detalles == null || !dto.Detalles.Any())
            return ApiResponse<PedidoDto>.Fail("El pedido debe contener al menos un producto");

        Usuario? usuario = null;
        if (dto.UsuarioId.HasValue && dto.UsuarioId.Value > 0)
        {
            usuario = await _context.Usuarios.FindAsync(dto.UsuarioId.Value);
        }

        // Si intenta fiar (compra a crédito), validar que sea cliente y tenga cupo
        if (dto.TipoPago == "Credito_Fiado")
        {
            if (usuario == null)
            {
                return ApiResponse<PedidoDto>.Fail("Solo los clientes registrados pueden comprar a crédito (fiar)");
            }
        }

        var pedido = new Pedido
        {
            UsuarioId = dto.UsuarioId,
            EsInvitado = dto.EsInvitado || usuario == null,
            InvitadoNombre = dto.InvitadoNombre ?? (usuario == null ? "Comprador General" : null),
            InvitadoEmail = dto.InvitadoEmail,
            InvitadoTelefono = dto.InvitadoTelefono,
            InvitadoCedula = dto.InvitadoCedula,
            FechaPedido = DateTime.UtcNow,
            FechaEntrega = dto.FechaEntrega,
            DireccionEntrega = dto.DireccionEntrega ?? usuario?.Direccion,
            Observaciones = dto.Observaciones,
            TipoPago = dto.TipoPago,
            ComprobanteBase64 = dto.ComprobanteBase64,
            ReferenciaPago = dto.ReferenciaPago,
            Estado = "Pendiente",
            Total = 0m
        };

        foreach (var item in dto.Detalles)
        {
            var producto = await _context.Productos.FindAsync(item.ProductoId);
            if (producto == null)
                return ApiResponse<PedidoDto>.Fail($"Producto con ID {item.ProductoId} no encontrado");

            if (producto.Stock < item.Cantidad)
                return ApiResponse<PedidoDto>.Fail($"Stock insuficiente para {producto.Nombre}. Disponible: {producto.Stock}");

            var precioEfectivo = (producto.EnOferta && producto.PrecioOferta.HasValue && producto.PrecioOferta.Value > 0)
                ? producto.PrecioOferta.Value
                : producto.Precio;

            var subtotal = item.Cantidad * precioEfectivo;
            pedido.Total += subtotal;

            pedido.Detalles.Add(new DetallePedido
            {
                ProductoId = item.ProductoId,
                Cantidad = item.Cantidad,
                PrecioUnitario = precioEfectivo
            });

            // Descontar del stock
            producto.Stock -= item.Cantidad;
        }

        decimal saldoAnterior = 0m;
        // Validar cupo de crédito para fiar
        if (dto.TipoPago == "Credito_Fiado" && usuario != null)
        {
            var cupoDisponible = usuario.LimiteCredito - usuario.DeudaActual;
            if (pedido.Total > cupoDisponible)
            {
                return ApiResponse<PedidoDto>.Fail(
                    $"El valor del pedido (${pedido.Total:N0}) supera su cupo disponible para fiar (${cupoDisponible:N0}). Cupo total: ${usuario.LimiteCredito:N0}.");
            }

            saldoAnterior = usuario.DeudaActual;
            usuario.DeudaActual += pedido.Total;
            pedido.EstadoPago = "Pendiente_Credito";
            pedido.MontoFiado = pedido.Total;
        }
        else
        {
            pedido.EstadoPago = "Pagado";
            pedido.MontoFiado = 0m;
        }

        _context.Pedidos.Add(pedido);
        await _context.SaveChangesAsync();

        // Registrar transacción de deuda si fue fiado
        if (dto.TipoPago == "Credito_Fiado" && usuario != null)
        {
            var transaccion = new TransaccionDeuda
            {
                UsuarioId = usuario.Id,
                PedidoId = pedido.Id,
                Monto = pedido.Total,
                SaldoAnterior = saldoAnterior,
                SaldoNuevo = usuario.DeudaActual,
                Tipo = "Cargo_Credito",
                Concepto = $"Compra a crédito (fiado) - Pedido #{pedido.Id}",
                Fecha = DateTime.UtcNow
            };

            _context.TransaccionesDeuda.Add(transaccion);
            await _context.SaveChangesAsync();
        }

        // Recargar con todas las relaciones para respuesta completa
        var finalPedido = await _context.Pedidos
            .Include(p => p.Usuario)
            .Include(p => p.Detalles)
                .ThenInclude(d => d.Producto)
            .FirstOrDefaultAsync(p => p.Id == pedido.Id);

        var resultDto = _mapper.Map<PedidoDto>(finalPedido ?? pedido);
        return ApiResponse<PedidoDto>.Ok(resultDto, "Pedido registrado exitosamente");
    }

    public async Task<ApiResponse<PedidoDto>> UpdateEstadoAsync(int id, string nuevoEstado)
    {
        var pedido = await _context.Pedidos
            .Include(p => p.Usuario)
            .Include(p => p.Detalles)
                .ThenInclude(d => d.Producto)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (pedido == null) return ApiResponse<PedidoDto>.Fail("Pedido no encontrado");

        pedido.Estado = nuevoEstado;
        await _context.SaveChangesAsync();

        return ApiResponse<PedidoDto>.Ok(_mapper.Map<PedidoDto>(pedido), $"Estado del pedido actualizado a '{nuevoEstado}'");
    }

    public async Task<ApiResponse<bool>> CancelarPedidoAsync(int id)
    {
        var pedido = await _context.Pedidos
            .Include(p => p.Usuario)
            .Include(p => p.Detalles)
                .ThenInclude(d => d.Producto)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (pedido == null) return ApiResponse<bool>.Fail("Pedido no encontrado");
        if (pedido.Estado == "Cancelado") return ApiResponse<bool>.Fail("El pedido ya está cancelado");

        // Revertir stock
        foreach (var det in pedido.Detalles)
        {
            if (det.Producto != null)
                det.Producto.Stock += det.Cantidad;
        }

        // Revertir deuda si fue fiado y no se ha cancelado
        if (pedido.TipoPago == "Credito_Fiado" && pedido.Usuario != null && pedido.EstadoPago == "Pendiente_Credito")
        {
            var saldoAnterior = pedido.Usuario.DeudaActual;
            pedido.Usuario.DeudaActual -= pedido.Total;
            if (pedido.Usuario.DeudaActual < 0) pedido.Usuario.DeudaActual = 0;

            _context.TransaccionesDeuda.Add(new TransaccionDeuda
            {
                UsuarioId = pedido.Usuario.Id,
                PedidoId = pedido.Id,
                Monto = pedido.Total,
                SaldoAnterior = saldoAnterior,
                SaldoNuevo = pedido.Usuario.DeudaActual,
                Tipo = "Abono_Pago",
                Concepto = $"Reversión por cancelación de pedido #{pedido.Id}",
                Fecha = DateTime.UtcNow
            });
        }

        pedido.Estado = "Cancelado";
        await _context.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, "Pedido cancelado y stock reincorporado");
    }

    public async Task<ApiResponse<List<TransaccionDeudaDto>>> GetTransaccionesDeudaAsync(int? usuarioId = null)
    {
        var query = _context.TransaccionesDeuda
            .Include(t => t.Usuario)
            .AsQueryable();

        if (usuarioId.HasValue && usuarioId.Value > 0)
            query = query.Where(t => t.UsuarioId == usuarioId.Value);

        var list = await query.OrderByDescending(t => t.Fecha).ToListAsync();
        return ApiResponse<List<TransaccionDeudaDto>>.Ok(_mapper.Map<List<TransaccionDeudaDto>>(list));
    }

    public async Task<ApiResponse<TransaccionDeudaDto>> RegistrarAbonoClienteAsync(RegistrarAbonoDto dto)
    {
        var usuario = await _context.Usuarios.FindAsync(dto.UsuarioId);
        if (usuario == null) return ApiResponse<TransaccionDeudaDto>.Fail("Cliente no encontrado");

        if (dto.Monto <= 0) return ApiResponse<TransaccionDeudaDto>.Fail("El monto del abono debe ser mayor a cero");

        var saldoAnterior = usuario.DeudaActual;
        usuario.DeudaActual -= dto.Monto;
        if (usuario.DeudaActual < 0) usuario.DeudaActual = 0; // Si pagó de más queda en 0

        var transaccion = new TransaccionDeuda
        {
            UsuarioId = usuario.Id,
            Monto = dto.Monto,
            SaldoAnterior = saldoAnterior,
            SaldoNuevo = usuario.DeudaActual,
            Tipo = "Abono_Pago",
            Concepto = string.IsNullOrWhiteSpace(dto.Concepto) ? "Abono a deuda de fiado" : dto.Concepto,
            MetodoPagoAbono = dto.MetodoPago ?? "Efectivo",
            ComprobanteBase64 = dto.ComprobanteBase64,
            ReferenciaPago = dto.ReferenciaPago,
            Fecha = DateTime.UtcNow
        };

        _context.TransaccionesDeuda.Add(transaccion);
        await _context.SaveChangesAsync();

        await _context.Entry(transaccion).Reference(t => t.Usuario).LoadAsync();

        return ApiResponse<TransaccionDeudaDto>.Ok(_mapper.Map<TransaccionDeudaDto>(transaccion), 
            $"Abono de ${dto.Monto:N0} registrado. Nuevo saldo pendiente: ${usuario.DeudaActual:N0}. Cupo disponible restablecido: ${usuario.LimiteCredito - usuario.DeudaActual:N0}");
    }
}