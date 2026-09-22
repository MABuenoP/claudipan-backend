using AutoMapper;
using ClaudipanAPI.Data;
using ClaudipanAPI.Interfaces;
using ClaudipanAPI.Models.DTOs;
using ClaudipanAPI.Models.Entities;
using ClaudipanAPI.Models.Responses;
using Microsoft.EntityFrameworkCore;

namespace ClaudipanAPI.Services;

public class InsumoService : IInsumoService
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public InsumoService(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<ApiResponse<List<InsumoDto>>> GetAllAsync(bool? soloActivos = true)
    {
        var query = _context.Insumos.AsQueryable();
        if (soloActivos.HasValue && soloActivos.Value)
            query = query.Where(i => i.Activo);

        var list = await query.OrderBy(i => i.Nombre).ToListAsync();
        return ApiResponse<List<InsumoDto>>.Ok(_mapper.Map<List<InsumoDto>>(list));
    }

    public async Task<ApiResponse<InsumoDto>> GetByIdAsync(int id)
    {
        var insumo = await _context.Insumos.FindAsync(id);
        if (insumo == null) return ApiResponse<InsumoDto>.Fail("Insumo no encontrado");
        return ApiResponse<InsumoDto>.Ok(_mapper.Map<InsumoDto>(insumo));
    }

    public async Task<ApiResponse<InsumoDto>> CreateAsync(InsumoCreateDto dto)
    {
        var insumo = _mapper.Map<Insumo>(dto);
        insumo.FechaActualizacion = DateTime.UtcNow;
        insumo.Activo = true;

        _context.Insumos.Add(insumo);
        await _context.SaveChangesAsync();

        return ApiResponse<InsumoDto>.Ok(_mapper.Map<InsumoDto>(insumo), "Insumo creado exitosamente");
    }

    public async Task<ApiResponse<InsumoDto>> UpdateAsync(int id, InsumoUpdateDto dto)
    {
        var insumo = await _context.Insumos.FindAsync(id);
        if (insumo == null) return ApiResponse<InsumoDto>.Fail("Insumo no encontrado");

        if (dto.Nombre != null) insumo.Nombre = dto.Nombre;
        if (dto.Descripcion != null) insumo.Descripcion = dto.Descripcion;
        if (dto.UnidadMedida != null) insumo.UnidadMedida = dto.UnidadMedida;
        if (dto.StockActual.HasValue) insumo.StockActual = dto.StockActual.Value;
        if (dto.CostoUnitario.HasValue) insumo.CostoUnitario = dto.CostoUnitario.Value;
        if (dto.StockMinimo.HasValue) insumo.StockMinimo = dto.StockMinimo.Value;
        if (dto.ProveedorPrincipal != null) insumo.ProveedorPrincipal = dto.ProveedorPrincipal;
        if (dto.Activo.HasValue) insumo.Activo = dto.Activo.Value;

        insumo.FechaActualizacion = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return ApiResponse<InsumoDto>.Ok(_mapper.Map<InsumoDto>(insumo), "Insumo actualizado exitosamente");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int id)
    {
        var insumo = await _context.Insumos.FindAsync(id);
        if (insumo == null) return ApiResponse<bool>.Fail("Insumo no encontrado");

        insumo.Activo = false;
        await _context.SaveChangesAsync();
        return ApiResponse<bool>.Ok(true, "Insumo desactivado exitosamente");
    }

    public async Task<ApiResponse<bool>> AjustarStockAsync(int id, decimal cantidadDelta, decimal? nuevoCosto = null)
    {
        var insumo = await _context.Insumos.FindAsync(id);
        if (insumo == null) return ApiResponse<bool>.Fail("Insumo no encontrado");

        insumo.StockActual += cantidadDelta;
        if (insumo.StockActual < 0) insumo.StockActual = 0;

        if (nuevoCosto.HasValue && nuevoCosto.Value > 0)
            insumo.CostoUnitario = nuevoCosto.Value;

        insumo.FechaActualizacion = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, "Stock de insumo ajustado exitosamente");
    }
}
