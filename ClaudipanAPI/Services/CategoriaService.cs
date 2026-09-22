using ClaudipanAPI.Data;
using ClaudipanAPI.Interfaces;
using ClaudipanAPI.Models.DTOs;
using ClaudipanAPI.Models.Entities;
using ClaudipanAPI.Models.Responses;
using Microsoft.EntityFrameworkCore;

namespace ClaudipanAPI.Services;

public class CategoriaService : ICategoriaService
{
    private readonly AppDbContext _context;

    public CategoriaService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<List<CategoriaDto>>> GetAllAsync()
    {
        var categorias = await _context.Categorias
            .Select(c => new CategoriaDto
            {
                Id = c.Id,
                Nombre = c.Nombre,
                Descripcion = c.Descripcion ?? string.Empty
            })
            .ToListAsync();

        return ApiResponse<List<CategoriaDto>>.Ok(categorias);
    }

    public async Task<ApiResponse<CategoriaDto>> GetByIdAsync(int id)
    {
        var categoria = await _context.Categorias.FindAsync(id);
        if (categoria == null)
            return ApiResponse<CategoriaDto>.Fail("Categoría no encontrada");

        return ApiResponse<CategoriaDto>.Ok(new CategoriaDto
        {
            Id = categoria.Id,
            Nombre = categoria.Nombre,
            Descripcion = categoria.Descripcion ?? string.Empty
        });
    }

    public async Task<ApiResponse<CategoriaDto>> CreateAsync(CategoriaCreateDto dto)
    {
        var categoria = new Categoria
        {
            Nombre = dto.Nombre,
            Descripcion = dto.Descripcion
        };

        _context.Categorias.Add(categoria);
        await _context.SaveChangesAsync();

        return ApiResponse<CategoriaDto>.Ok(new CategoriaDto
        {
            Id = categoria.Id,
            Nombre = categoria.Nombre,
            Descripcion = categoria.Descripcion ?? string.Empty
        }, "Categoría creada exitosamente");
    }

    public async Task<ApiResponse<CategoriaDto>> UpdateAsync(int id, CategoriaUpdateDto dto)
    {
        var categoria = await _context.Categorias.FindAsync(id);
        if (categoria == null)
            return ApiResponse<CategoriaDto>.Fail("Categoría no encontrada");

        if (!string.IsNullOrWhiteSpace(dto.Nombre))
            categoria.Nombre = dto.Nombre;

        if (dto.Descripcion != null)
            categoria.Descripcion = dto.Descripcion;

        await _context.SaveChangesAsync();

        return ApiResponse<CategoriaDto>.Ok(new CategoriaDto
        {
            Id = categoria.Id,
            Nombre = categoria.Nombre,
            Descripcion = categoria.Descripcion ?? string.Empty
        }, "Categoría actualizada exitosamente");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int id)
    {
        var categoria = await _context.Categorias.FindAsync(id);
        if (categoria == null)
            return ApiResponse<bool>.Fail("Categoría no encontrada");

        _context.Categorias.Remove(categoria);
        await _context.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, "Categoría eliminada exitosamente");
    }
}
