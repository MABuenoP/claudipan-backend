using ClaudipanAPI.Data;
using ClaudipanAPI.Interfaces;
using ClaudipanAPI.Models.DTOs;
using ClaudipanAPI.Models.Entities;
using ClaudipanAPI.Models.Responses;
using Microsoft.EntityFrameworkCore;

namespace ClaudipanAPI.Services;

public class InventarioService : IInventarioService
{
    private readonly AppDbContext _context;

    public InventarioService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<List<MaterialInventarioDto>>> GetAllAsync()
    {
        var materiales = await _context.MaterialesInventario
            .Select(m => new MaterialInventarioDto
            {
                Id = m.Id,
                Nombre = m.Nombre,
                Cantidad = m.Cantidad,
                Unidad = m.Unidad,
                Proveedor = m.Proveedor,
                StockMinimo = m.StockMinimo,
                FechaVencimiento = m.FechaVencimiento
            })
            .ToListAsync();

        return ApiResponse<List<MaterialInventarioDto>>.Ok(materiales);
    }

    public async Task<ApiResponse<MaterialInventarioDto>> GetByIdAsync(int id)
    {
        var material = await _context.MaterialesInventario.FindAsync(id);
        if (material == null)
            return ApiResponse<MaterialInventarioDto>.Fail("Material no encontrado");

        return ApiResponse<MaterialInventarioDto>.Ok(new MaterialInventarioDto
        {
            Id = material.Id,
            Nombre = material.Nombre,
            Cantidad = material.Cantidad,
            Unidad = material.Unidad,
            Proveedor = material.Proveedor,
            StockMinimo = material.StockMinimo,
            FechaVencimiento = material.FechaVencimiento
        });
    }

    public async Task<ApiResponse<MaterialInventarioDto>> CreateAsync(MaterialInventarioCreateDto dto)
    {
        var material = new MaterialInventario
        {
            Nombre = dto.Nombre,
            Cantidad = dto.Cantidad,
            Unidad = dto.Unidad,
            Proveedor = dto.Proveedor,
            StockMinimo = dto.StockMinimo,
            FechaVencimiento = dto.FechaVencimiento,
            FechaActualizacion = DateTime.UtcNow
        };

        _context.MaterialesInventario.Add(material);
        await _context.SaveChangesAsync();

        return ApiResponse<MaterialInventarioDto>.Ok(new MaterialInventarioDto
        {
            Id = material.Id,
            Nombre = material.Nombre,
            Cantidad = material.Cantidad,
            Unidad = material.Unidad,
            Proveedor = material.Proveedor,
            StockMinimo = material.StockMinimo,
            FechaVencimiento = material.FechaVencimiento
        }, "Material creado exitosamente");
    }

    public async Task<ApiResponse<MaterialInventarioDto>> UpdateAsync(int id, MaterialInventarioUpdateDto dto)
    {
        var material = await _context.MaterialesInventario.FindAsync(id);
        if (material == null)
            return ApiResponse<MaterialInventarioDto>.Fail("Material no encontrado");

        if (!string.IsNullOrWhiteSpace(dto.Nombre)) material.Nombre = dto.Nombre;
        if (dto.Cantidad.HasValue) material.Cantidad = dto.Cantidad.Value;
        if (!string.IsNullOrWhiteSpace(dto.Unidad)) material.Unidad = dto.Unidad;
        if (dto.Proveedor != null) material.Proveedor = dto.Proveedor;
        if (dto.StockMinimo.HasValue) material.StockMinimo = dto.StockMinimo;
        if (dto.FechaVencimiento.HasValue) material.FechaVencimiento = dto.FechaVencimiento;

        material.FechaActualizacion = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return ApiResponse<MaterialInventarioDto>.Ok(new MaterialInventarioDto
        {
            Id = material.Id,
            Nombre = material.Nombre,
            Cantidad = material.Cantidad,
            Unidad = material.Unidad,
            Proveedor = material.Proveedor,
            StockMinimo = material.StockMinimo,
            FechaVencimiento = material.FechaVencimiento
        }, "Material actualizado exitosamente");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int id)
    {
        var material = await _context.MaterialesInventario.FindAsync(id);
        if (material == null)
            return ApiResponse<bool>.Fail("Material no encontrado");

        _context.MaterialesInventario.Remove(material);
        await _context.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, "Material eliminado exitosamente");
    }
}
