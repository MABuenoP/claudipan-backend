using AutoMapper;
using ClaudipanAPI.Data;
using ClaudipanAPI.Interfaces;
using ClaudipanAPI.Models.DTOs;
using ClaudipanAPI.Models.Entities;
using ClaudipanAPI.Models.Responses;
using Microsoft.EntityFrameworkCore;

namespace ClaudipanAPI.Services;

public class CompraService : ICompraService
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public CompraService(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<ApiResponse<List<CompraDto>>> GetAllAsync(int? proveedorId = null)
    {
        var query = _context.Compras
            .Include(c => c.Proveedor)
            .Include(c => c.Detalles)
                .ThenInclude(d => d.Insumo)
            .Include(c => c.Detalles)
                .ThenInclude(d => d.Producto)
            .AsQueryable();

        if (proveedorId.HasValue && proveedorId.Value > 0)
            query = query.Where(c => c.ProveedorId == proveedorId.Value);

        var list = await query.OrderByDescending(c => c.FechaCompra).ToListAsync();
        return ApiResponse<List<CompraDto>>.Ok(_mapper.Map<List<CompraDto>>(list));
    }

    public async Task<ApiResponse<CompraDto>> GetByIdAsync(int id)
    {
        var compra = await _context.Compras
            .Include(c => c.Proveedor)
            .Include(c => c.Detalles)
                .ThenInclude(d => d.Insumo)
            .Include(c => c.Detalles)
                .ThenInclude(d => d.Producto)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (compra == null) return ApiResponse<CompraDto>.Fail("Compra no encontrada");
        return ApiResponse<CompraDto>.Ok(_mapper.Map<CompraDto>(compra));
    }

    public async Task<ApiResponse<CompraDto>> CreateAsync(CompraCreateDto dto)
    {
        if (dto.Detalles == null || !dto.Detalles.Any())
            return ApiResponse<CompraDto>.Fail("La compra debe tener al menos un detalle");

        var proveedor = await _context.Proveedores.FindAsync(dto.ProveedorId);
        if (proveedor == null) return ApiResponse<CompraDto>.Fail("Proveedor no encontrado");

        var compra = new Compra
        {
            ProveedorId = dto.ProveedorId,
            NumeroFactura = dto.NumeroFactura,
            FechaCompra = DateTime.UtcNow,
            MetodoPago = dto.MetodoPago,
            EstadoPago = dto.EstadoPago,
            Observaciones = dto.Observaciones,
            Total = 0m
        };

        foreach (var d in dto.Detalles)
        {
            var subtotal = d.Cantidad * d.PrecioUnitario;
            compra.Total += subtotal;

            var detalle = new DetalleCompra
            {
                InsumoId = d.InsumoId,
                ProductoId = d.ProductoId,
                DescripcionItem = d.DescripcionItem,
                Cantidad = d.Cantidad,
                PrecioUnitario = d.PrecioUnitario,
                Subtotal = subtotal
            };

            compra.Detalles.Add(detalle);

            // Actualizar stock de Insumo si aplica
            if (d.InsumoId.HasValue && d.InsumoId.Value > 0)
            {
                var insumo = await _context.Insumos.FindAsync(d.InsumoId.Value);
                if (insumo != null)
                {
                    insumo.StockActual += d.Cantidad;
                    insumo.CostoUnitario = d.PrecioUnitario; // Costo promedio o última compra
                    insumo.FechaActualizacion = DateTime.UtcNow;
                }
            }

            // Actualizar stock de Producto si aplica (ej. Gaseosas, Lácteos terminados)
            if (d.ProductoId.HasValue && d.ProductoId.Value > 0)
            {
                var producto = await _context.Productos.FindAsync(d.ProductoId.Value);
                if (producto != null)
                {
                    producto.Stock += (int)d.Cantidad;
                    producto.CostoBaseProduccion = d.PrecioUnitario; // Costo de compra para margen
                }
            }
        }

        _context.Compras.Add(compra);
        await _context.SaveChangesAsync();

        // Recargar relaciones
        await _context.Entry(compra).Reference(c => c.Proveedor).LoadAsync();

        return ApiResponse<CompraDto>.Ok(_mapper.Map<CompraDto>(compra), "Compra registrada y stock actualizado con éxito");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int id)
    {
        var compra = await _context.Compras.Include(c => c.Detalles).FirstOrDefaultAsync(c => c.Id == id);
        if (compra == null) return ApiResponse<bool>.Fail("Compra no encontrada");

        _context.Compras.Remove(compra);
        await _context.SaveChangesAsync();
        return ApiResponse<bool>.Ok(true, "Compra eliminada exitosamente");
    }
}
