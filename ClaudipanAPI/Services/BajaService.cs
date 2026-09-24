using AutoMapper;
using ClaudipanAPI.Data;
using ClaudipanAPI.Interfaces;
using ClaudipanAPI.Models.DTOs;
using ClaudipanAPI.Models.Entities;
using ClaudipanAPI.Models.Responses;
using Microsoft.EntityFrameworkCore;

namespace ClaudipanAPI.Services;

public class BajaService : IBajaService
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public BajaService(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<ApiResponse<List<BajaProductoDto>>> GetAllAsync(int? productoId = null, string? motivo = null)
    {
        var query = _context.BajasProductos
            .Include(b => b.Producto)
            .Include(b => b.Usuario)
            .AsQueryable();

        if (productoId.HasValue && productoId.Value > 0)
            query = query.Where(b => b.ProductoId == productoId.Value);

        if (!string.IsNullOrWhiteSpace(motivo))
            query = query.Where(b => b.Motivo == motivo);

        var list = await query.OrderByDescending(b => b.FechaBaja).ToListAsync();
        return ApiResponse<List<BajaProductoDto>>.Ok(_mapper.Map<List<BajaProductoDto>>(list));
    }

    public async Task<ApiResponse<BajaProductoDto>> GetByIdAsync(int id)
    {
        var baja = await _context.BajasProductos
            .Include(b => b.Producto)
            .Include(b => b.Usuario)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (baja == null) return ApiResponse<BajaProductoDto>.Fail("Registro de baja no encontrado");
        return ApiResponse<BajaProductoDto>.Ok(_mapper.Map<BajaProductoDto>(baja));
    }

    public async Task<ApiResponse<BajaProductoDto>> CreateAsync(int? usuarioId, BajaProductoCreateDto dto)
    {
        var producto = await _context.Productos
            .Include(p => p.Categoria)
            .FirstOrDefaultAsync(p => p.Id == dto.ProductoId);

        if (producto == null) return ApiResponse<BajaProductoDto>.Fail("Producto no encontrado");

        if (dto.Cantidad <= 0) return ApiResponse<BajaProductoDto>.Fail("La cantidad debe ser mayor a 0");

        var esPan = (producto.Categoria?.Nombre?.ToLower().Contains("pan") ?? false) 
                    || producto.Nombre.ToLower().Contains("pan");

        var transformar = dto.EsParaTransformar || dto.Motivo.Equals("Transformacion", StringComparison.OrdinalIgnoreCase);

        // Validación: si se solicita transformar pero el producto no es pan, rechazar
        if (transformar && !esPan)
        {
            return ApiResponse<BajaProductoDto>.Fail("La transformación como materia prima solo es permitida para productos de panadería (panes).");
        }

        var costoUnit = producto.CostoBaseProduccion > 0 ? producto.CostoBaseProduccion : (producto.Precio * 0.6m); // Si no tiene costo base, 60% del precio
        var perdidaTotal = dto.Cantidad * costoUnit;

        var baja = new BajaProducto
        {
            ProductoId = dto.ProductoId,
            Cantidad = dto.Cantidad,
            Motivo = transformar ? "Transformacion" : dto.Motivo,
            CostoUnitario = costoUnit,
            CostoPerdidaTotal = perdidaTotal,
            FechaBaja = DateTime.UtcNow,
            UsuarioId = usuarioId,
            Observaciones = dto.Observaciones
        };

        // 1. Descontar del inventario de panes o productos que salieron
        producto.Stock -= dto.Cantidad;
        if (producto.Stock < 0) producto.Stock = 0;

        string mensaje = $"Baja registrada: {dto.Cantidad} unidades descontadas del inventario de '{producto.Nombre}'.";

        // 2. Si se trata de panes y es para transformar, sumar al inventario de insumos Transformación
        if (transformar && esPan)
        {
            var insumo = await _context.Insumos.FirstOrDefaultAsync(i => 
                i.Nombre.ToLower().Contains("transformación") || 
                i.Nombre.ToLower().Contains("transformacion") || 
                i.Nombre.ToLower().Contains("miga de pan"));

            if (insumo == null)
            {
                insumo = new Insumo
                {
                    Nombre = "Pan de Transformación (Harina de Pan / Pastas Negras)",
                    Descripcion = "Miga y piezas de pan recuperadas de horneado o mostrador para reciclaje y transformación gastronómica.",
                    UnidadMedida = "Kg",
                    StockActual = 0m,
                    StockMinimo = 5m,
                    CostoUnitario = 1500m,
                    ProveedorPrincipal = "Producción Interna Claudipan",
                    Activo = true,
                    FechaActualizacion = DateTime.UtcNow
                };
                _context.Insumos.Add(insumo);
                await _context.SaveChangesAsync();
            }

            decimal cantidadASumar;
            if (dto.KilosTransformacion.HasValue && dto.KilosTransformacion.Value > 0)
            {
                cantidadASumar = dto.KilosTransformacion.Value;
            }
            else
            {
                // Estimación estándar: 1 pan = 0.08 Kg (80 gramos) para insumo medido en Kg
                cantidadASumar = insumo.UnidadMedida.Equals("Kg", StringComparison.OrdinalIgnoreCase) 
                    ? Math.Round(dto.Cantidad * 0.08m, 2) 
                    : dto.Cantidad;
            }

            insumo.StockActual += cantidadASumar;
            insumo.FechaActualizacion = DateTime.UtcNow;

            var notaTransf = $"[TRANSFORMACIÓN A INSUMO] {dto.Cantidad} unidades de pan ({cantidadASumar:F2} {insumo.UnidadMedida}) sumadas al inventario de '{insumo.Nombre}'.";
            baja.Observaciones = string.IsNullOrWhiteSpace(baja.Observaciones) 
                ? notaTransf 
                : $"{baja.Observaciones} | {notaTransf}";

            mensaje = $"¡Baja y Transformación exitosa! Se descontaron {dto.Cantidad} unidades de '{producto.Nombre}' de inventario y se sumaron {cantidadASumar:F2} {insumo.UnidadMedida} al inventario del insumo '{insumo.Nombre}'.";
        }

        _context.BajasProductos.Add(baja);
        await _context.SaveChangesAsync();

        await _context.Entry(baja).Reference(b => b.Producto).LoadAsync();
        if (usuarioId.HasValue)
            await _context.Entry(baja).Reference(b => b.Usuario).LoadAsync();

        var resultDto = _mapper.Map<BajaProductoDto>(baja);
        resultDto.EsParaTransformar = transformar;

        return ApiResponse<BajaProductoDto>.Ok(resultDto, mensaje);
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int id)
    {
        var baja = await _context.BajasProductos.FindAsync(id);
        if (baja == null) return ApiResponse<bool>.Fail("Registro de baja no encontrado");

        _context.BajasProductos.Remove(baja);
        await _context.SaveChangesAsync();
        return ApiResponse<bool>.Ok(true, "Registro de baja eliminado");
    }
}
