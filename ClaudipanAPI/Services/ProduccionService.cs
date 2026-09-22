using AutoMapper;
using ClaudipanAPI.Data;
using ClaudipanAPI.Interfaces;
using ClaudipanAPI.Models.DTOs;
using ClaudipanAPI.Models.Entities;
using ClaudipanAPI.Models.Responses;
using Microsoft.EntityFrameworkCore;

namespace ClaudipanAPI.Services;

public class ProduccionService : IProduccionService
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public ProduccionService(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<ApiResponse<List<RecetaProduccionDto>>> GetAllRecetasAsync()
    {
        var recetas = await _context.RecetasProduccion
            .Include(r => r.Producto)
            .Include(r => r.Detalles)
                .ThenInclude(d => d.Insumo)
            .Where(r => r.Activo)
            .OrderBy(r => r.NombreReceta)
            .ToListAsync();

        return ApiResponse<List<RecetaProduccionDto>>.Ok(_mapper.Map<List<RecetaProduccionDto>>(recetas));
    }

    public async Task<ApiResponse<RecetaProduccionDto>> GetRecetaByIdAsync(int id)
    {
        var receta = await _context.RecetasProduccion
            .Include(r => r.Producto)
            .Include(r => r.Detalles)
                .ThenInclude(d => d.Insumo)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (receta == null) return ApiResponse<RecetaProduccionDto>.Fail("Receta no encontrada");
        return ApiResponse<RecetaProduccionDto>.Ok(_mapper.Map<RecetaProduccionDto>(receta));
    }

    public async Task<ApiResponse<RecetaProduccionDto>> CreateRecetaAsync(RecetaCreateDto dto)
    {
        var producto = await _context.Productos.FindAsync(dto.ProductoId);
        if (producto == null) return ApiResponse<RecetaProduccionDto>.Fail("Producto no encontrado");

        var receta = new RecetaProduccion
        {
            ProductoId = dto.ProductoId,
            NombreReceta = dto.NombreReceta,
            Descripcion = dto.Descripcion,
            RendimientoUnidades = dto.RendimientoUnidades > 0 ? dto.RendimientoUnidades : 50,
            FechaCreacion = DateTime.UtcNow,
            Activo = true
        };

        decimal costoTotal = 0m;

        foreach (var d in dto.Detalles)
        {
            var insumo = await _context.Insumos.FindAsync(d.InsumoId);
            if (insumo != null)
            {
                costoTotal += d.CantidadNecesaria * insumo.CostoUnitario;
            }

            receta.Detalles.Add(new DetalleReceta
            {
                InsumoId = d.InsumoId,
                CantidadNecesaria = d.CantidadNecesaria,
                UnidadMedida = d.UnidadMedida
            });
        }

        receta.CostoTotalInsumos = costoTotal;
        receta.CostoUnitarioEstimado = receta.RendimientoUnidades > 0 ? (costoTotal / receta.RendimientoUnidades) : 0m;

        // Actualizar el costo base del producto
        producto.CostoBaseProduccion = receta.CostoUnitarioEstimado;

        _context.RecetasProduccion.Add(receta);
        await _context.SaveChangesAsync();

        await _context.Entry(receta).Reference(r => r.Producto).LoadAsync();
        foreach (var det in receta.Detalles)
        {
            await _context.Entry(det).Reference(d => d.Insumo).LoadAsync();
        }

        return ApiResponse<RecetaProduccionDto>.Ok(_mapper.Map<RecetaProduccionDto>(receta), "Receta creada y costo base de producto actualizado");
    }

    public async Task<ApiResponse<bool>> DeleteRecetaAsync(int id)
    {
        var receta = await _context.RecetasProduccion.FindAsync(id);
        if (receta == null) return ApiResponse<bool>.Fail("Receta no encontrada");

        receta.Activo = false;
        await _context.SaveChangesAsync();
        return ApiResponse<bool>.Ok(true, "Receta desactivada");
    }

    public async Task<ApiResponse<List<OrdenProduccionDto>>> GetAllOrdenesAsync(int? panaderoId = null, string? estado = null)
    {
        var query = _context.OrdenesProduccion
            .Include(o => o.PanaderoUsuario)
            .Include(o => o.Producto)
            .Include(o => o.Receta)
            .AsQueryable();

        if (panaderoId.HasValue && panaderoId.Value > 0)
            query = query.Where(o => o.PanaderoUsuarioId == panaderoId.Value);

        if (!string.IsNullOrWhiteSpace(estado))
            query = query.Where(o => o.Estado == estado);

        var list = await query.OrderByDescending(o => o.FechaOrden).ToListAsync();
        return ApiResponse<List<OrdenProduccionDto>>.Ok(_mapper.Map<List<OrdenProduccionDto>>(list));
    }

    public async Task<ApiResponse<OrdenProduccionDto>> GetOrdenByIdAsync(int id)
    {
        var orden = await _context.OrdenesProduccion
            .Include(o => o.PanaderoUsuario)
            .Include(o => o.Producto)
            .Include(o => o.Receta)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (orden == null) return ApiResponse<OrdenProduccionDto>.Fail("Orden de producción no encontrada");
        return ApiResponse<OrdenProduccionDto>.Ok(_mapper.Map<OrdenProduccionDto>(orden));
    }

    public async Task<ApiResponse<OrdenProduccionDto>> CreateOrdenAsync(int panaderoId, OrdenProduccionCreateDto dto)
    {
        var producto = await _context.Productos.FindAsync(dto.ProductoId);
        if (producto == null) return ApiResponse<OrdenProduccionDto>.Fail("Producto no encontrado");

        // Buscar receta asociada si no se especificó
        var recetaId = dto.RecetaId;
        if (!recetaId.HasValue)
        {
            var receta = await _context.RecetasProduccion.FirstOrDefaultAsync(r => r.ProductoId == dto.ProductoId && r.Activo);
            if (receta != null) recetaId = receta.Id;
        }

        var count = await _context.OrdenesProduccion.CountAsync() + 1;
        var orden = new OrdenProduccion
        {
            CodigoOrden = $"ORD-{DateTime.UtcNow:yyyyMM}-{count:D4}",
            PanaderoUsuarioId = panaderoId,
            ProductoId = dto.ProductoId,
            RecetaId = recetaId,
            CantidadProgramada = dto.CantidadProgramada,
            CantidadProducida = 0,
            CostoInsumos = 0m,
            Estado = "Pendiente",
            FechaOrden = DateTime.UtcNow,
            Observaciones = dto.Observaciones
        };

        _context.OrdenesProduccion.Add(orden);
        await _context.SaveChangesAsync();

        await _context.Entry(orden).Reference(o => o.PanaderoUsuario).LoadAsync();
        await _context.Entry(orden).Reference(o => o.Producto).LoadAsync();

        return ApiResponse<OrdenProduccionDto>.Ok(_mapper.Map<OrdenProduccionDto>(orden), "Orden de producción creada");
    }

    public async Task<ApiResponse<OrdenProduccionDto>> IniciarOrdenAsync(int id)
    {
        var orden = await _context.OrdenesProduccion
            .Include(o => o.PanaderoUsuario)
            .Include(o => o.Producto)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (orden == null) return ApiResponse<OrdenProduccionDto>.Fail("Orden no encontrada");

        orden.Estado = "En_Proceso";
        await _context.SaveChangesAsync();

        return ApiResponse<OrdenProduccionDto>.Ok(_mapper.Map<OrdenProduccionDto>(orden), "Orden iniciada en producción");
    }

    public async Task<ApiResponse<OrdenProduccionDto>> EntregarProduccionAsync(int id, EntregarProduccionDto dto)
    {
        var orden = await _context.OrdenesProduccion
            .Include(o => o.PanaderoUsuario)
            .Include(o => o.Producto)
            .Include(o => o.Receta)
                .ThenInclude(r => r!.Detalles)
                    .ThenInclude(d => d.Insumo)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (orden == null) return ApiResponse<OrdenProduccionDto>.Fail("Orden no encontrada");
        if (orden.Estado == "Entregada") return ApiResponse<OrdenProduccionDto>.Fail("Esta orden ya fue entregada anteriormente");

        var cantidadReal = dto.CantidadProducida > 0 ? dto.CantidadProducida : orden.CantidadProgramada;
        orden.CantidadProducida = cantidadReal;
        orden.FechaEntrega = DateTime.UtcNow;
        orden.Estado = "Entregada";
        if (!string.IsNullOrWhiteSpace(dto.Observaciones))
            orden.Observaciones = dto.Observaciones;

        decimal costoInsumosTotal = 0m;

        // Descontar insumos si existe receta vinculada
        if (orden.Receta != null && orden.Receta.Detalles.Any())
        {
            var factorLote = (decimal)cantidadReal / (orden.Receta.RendimientoUnidades > 0 ? orden.Receta.RendimientoUnidades : 50m);

            foreach (var det in orden.Receta.Detalles)
            {
                var insumoRequerido = det.CantidadNecesaria * factorLote;
                var insumoDb = await _context.Insumos.FindAsync(det.InsumoId);
                if (insumoDb != null)
                {
                    insumoDb.StockActual -= insumoRequerido;
                    if (insumoDb.StockActual < 0) insumoDb.StockActual = 0; // Evitar stock negativo absurdo
                    insumoDb.FechaActualizacion = DateTime.UtcNow;

                    costoInsumosTotal += insumoRequerido * insumoDb.CostoUnitario;
                }
            }
        }
        else
        {
            // Estimación directa con costo base del producto
            costoInsumosTotal = cantidadReal * (orden.Producto?.CostoBaseProduccion ?? 0m);
        }

        orden.CostoInsumos = costoInsumosTotal;

        // Sumar al stock de productos listos para la venta
        if (orden.Producto != null)
        {
            orden.Producto.Stock += cantidadReal;
            orden.Producto.Disponible = true;
        }

        await _context.SaveChangesAsync();

        return ApiResponse<OrdenProduccionDto>.Ok(_mapper.Map<OrdenProduccionDto>(orden), 
            $"¡Producción de {cantidadReal} unidades entregada exitosamente al inventario!");
    }

    public async Task<ApiResponse<bool>> CancelarOrdenAsync(int id)
    {
        var orden = await _context.OrdenesProduccion.FindAsync(id);
        if (orden == null) return ApiResponse<bool>.Fail("Orden no encontrada");

        orden.Estado = "Cancelada";
        await _context.SaveChangesAsync();
        return ApiResponse<bool>.Ok(true, "Orden cancelada");
    }
}
