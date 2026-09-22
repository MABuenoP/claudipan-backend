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
        var producto = await _context.Productos.FindAsync(dto.ProductoId);
        if (producto == null) return ApiResponse<BajaProductoDto>.Fail("Producto no encontrado");

        if (dto.Cantidad <= 0) return ApiResponse<BajaProductoDto>.Fail("La cantidad debe ser mayor a 0");

        var costoUnit = producto.CostoBaseProduccion > 0 ? producto.CostoBaseProduccion : (producto.Precio * 0.6m); // Si no tiene costo base, 60% del precio
        var perdidaTotal = dto.Cantidad * costoUnit;

        var baja = new BajaProducto
        {
            ProductoId = dto.ProductoId,
            Cantidad = dto.Cantidad,
            Motivo = dto.Motivo,
            CostoUnitario = costoUnit,
            CostoPerdidaTotal = perdidaTotal,
            FechaBaja = DateTime.UtcNow,
            UsuarioId = usuarioId,
            Observaciones = dto.Observaciones
        };

        // Descontar del inventario de productos
        producto.Stock -= dto.Cantidad;
        if (producto.Stock < 0) producto.Stock = 0;

        _context.BajasProductos.Add(baja);
        await _context.SaveChangesAsync();

        await _context.Entry(baja).Reference(b => b.Producto).LoadAsync();
        if (usuarioId.HasValue)
            await _context.Entry(baja).Reference(b => b.Usuario).LoadAsync();

        return ApiResponse<BajaProductoDto>.Ok(_mapper.Map<BajaProductoDto>(baja), "Baja registrada y descontada de inventario");
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
