using AutoMapper;
using ClaudipanAPI.Data;
using ClaudipanAPI.Interfaces;
using ClaudipanAPI.Models.DTOs;
using ClaudipanAPI.Models.Entities;
using ClaudipanAPI.Models.Responses;
using Microsoft.EntityFrameworkCore;

namespace ClaudipanAPI.Services;

public class ProductoService : IProductoService
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public ProductoService(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<ApiResponse<List<ProductoDto>>> GetAllAsync(
        int? categoriaId = null, 
        string? search = null, 
        decimal? minPrecio = null, 
        decimal? maxPrecio = null, 
        bool? soloOfertas = null, 
        string? marca = null, 
        string? sabor = null, 
        string? tamano = null,
        string? presentacion = null)
    {
        var query = _context.Productos
            .Include(p => p.Categoria)
            .AsQueryable();

        if (categoriaId.HasValue && categoriaId.Value > 0)
            query = query.Where(p => p.CategoriaId == categoriaId.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(p => p.Nombre.ToLower().Contains(s) || 
                                     p.Descripcion.ToLower().Contains(s) ||
                                     (p.Marca != null && p.Marca.ToLower().Contains(s)) ||
                                     (p.Sabor != null && p.Sabor.ToLower().Contains(s)));
        }

        if (minPrecio.HasValue)
            query = query.Where(p => p.Precio >= minPrecio.Value);

        if (maxPrecio.HasValue)
            query = query.Where(p => p.Precio <= maxPrecio.Value);

        if (soloOfertas.HasValue && soloOfertas.Value)
            query = query.Where(p => p.EnOferta);

        if (!string.IsNullOrWhiteSpace(marca))
            query = query.Where(p => p.Marca != null && p.Marca.ToLower() == marca.Trim().ToLower());

        if (!string.IsNullOrWhiteSpace(sabor))
            query = query.Where(p => p.Sabor != null && p.Sabor.ToLower().Contains(sabor.Trim().ToLower()));

        if (!string.IsNullOrWhiteSpace(tamano))
            query = query.Where(p => p.Tamano != null && p.Tamano.ToLower().Contains(tamano.Trim().ToLower()));

        if (!string.IsNullOrWhiteSpace(presentacion))
            query = query.Where(p => p.Presentacion != null && p.Presentacion.ToLower().Contains(presentacion.Trim().ToLower()));

        var list = await query.OrderBy(p => p.CategoriaId).ThenBy(p => p.Precio).ToListAsync();
        var dtos = _mapper.Map<List<ProductoDto>>(list);
        return ApiResponse<List<ProductoDto>>.Ok(dtos);
    }

    public async Task<ApiResponse<ProductoDto>> GetByIdAsync(int id)
    {
        var producto = await _context.Productos
            .Include(p => p.Categoria)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (producto == null)
            return ApiResponse<ProductoDto>.Fail("Producto no encontrado");

        var dto = _mapper.Map<ProductoDto>(producto);
        return ApiResponse<ProductoDto>.Ok(dto);
    }

    public async Task<ApiResponse<ProductoDto>> CreateAsync(ProductoCreateDto dto)
    {
        var producto = _mapper.Map<Producto>(dto);
        producto.FechaCreacion = DateTime.UtcNow;

        _context.Productos.Add(producto);
        await _context.SaveChangesAsync();

        // Recargar con Categoria para nombre
        await _context.Entry(producto).Reference(p => p.Categoria).LoadAsync();

        var resultDto = _mapper.Map<ProductoDto>(producto);
        return ApiResponse<ProductoDto>.Ok(resultDto, "Producto creado exitosamente");
    }

    public async Task<ApiResponse<ProductoDto>> UpdateAsync(int id, ProductoUpdateDto dto)
    {
        var producto = await _context.Productos
            .Include(p => p.Categoria)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (producto == null)
            return ApiResponse<ProductoDto>.Fail("Producto no encontrado");

        if (dto.Nombre != null) producto.Nombre = dto.Nombre;
        if (dto.Descripcion != null) producto.Descripcion = dto.Descripcion;
        if (dto.Precio.HasValue) producto.Precio = dto.Precio.Value;
        if (dto.PrecioOferta.HasValue) producto.PrecioOferta = dto.PrecioOferta.Value;
        if (dto.EnOferta.HasValue) producto.EnOferta = dto.EnOferta.Value;
        if (dto.CostoBaseProduccion.HasValue) producto.CostoBaseProduccion = dto.CostoBaseProduccion.Value;
        if (dto.Stock.HasValue) producto.Stock = dto.Stock.Value;
        if (dto.ImagenUrl != null) producto.ImagenUrl = dto.ImagenUrl;
        if (dto.Disponible.HasValue) producto.Disponible = dto.Disponible.Value;
        if (dto.Presentacion != null) producto.Presentacion = dto.Presentacion;
        if (dto.Marca != null) producto.Marca = dto.Marca;
        if (dto.Sabor != null) producto.Sabor = dto.Sabor;
        if (dto.Tamano != null) producto.Tamano = dto.Tamano;
        if (dto.CategoriaId.HasValue) producto.CategoriaId = dto.CategoriaId.Value;

        await _context.SaveChangesAsync();

        var resultDto = _mapper.Map<ProductoDto>(producto);
        return ApiResponse<ProductoDto>.Ok(resultDto, "Producto actualizado exitosamente");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int id)
    {
        var producto = await _context.Productos.FindAsync(id);
        if (producto == null)
            return ApiResponse<bool>.Fail("Producto no encontrado");

        _context.Productos.Remove(producto);
        await _context.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, "Producto eliminado exitosamente");
    }

    public async Task<ApiResponse<bool>> UpdateStockAsync(int id, int cantidad)
    {
        var producto = await _context.Productos.FindAsync(id);
        if (producto == null)
            return ApiResponse<bool>.Fail("Producto no encontrado");

        producto.Stock += cantidad;
        if (producto.Stock < 0) producto.Stock = 0;

        await _context.SaveChangesAsync();
        return ApiResponse<bool>.Ok(true, "Stock actualizado exitosamente");
    }
}