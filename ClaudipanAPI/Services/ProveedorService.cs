using AutoMapper;
using ClaudipanAPI.Data;
using ClaudipanAPI.Interfaces;
using ClaudipanAPI.Models.DTOs;
using ClaudipanAPI.Models.Entities;
using ClaudipanAPI.Models.Responses;
using Microsoft.EntityFrameworkCore;

namespace ClaudipanAPI.Services;

public class ProveedorService : IProveedorService
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public ProveedorService(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<ApiResponse<List<ProveedorDto>>> GetAllAsync(bool? soloActivos = true)
    {
        var query = _context.Proveedores.AsQueryable();
        if (soloActivos.HasValue && soloActivos.Value)
            query = query.Where(p => p.Activo);

        var list = await query.OrderBy(p => p.Nombre).ToListAsync();
        return ApiResponse<List<ProveedorDto>>.Ok(_mapper.Map<List<ProveedorDto>>(list));
    }

    public async Task<ApiResponse<ProveedorDto>> GetByIdAsync(int id)
    {
        var prov = await _context.Proveedores.FindAsync(id);
        if (prov == null) return ApiResponse<ProveedorDto>.Fail("Proveedor no encontrado");
        return ApiResponse<ProveedorDto>.Ok(_mapper.Map<ProveedorDto>(prov));
    }

    public async Task<ApiResponse<ProveedorDto>> CreateAsync(ProveedorCreateDto dto)
    {
        if (await _context.Proveedores.AnyAsync(p => p.Nit == dto.Nit))
            return ApiResponse<ProveedorDto>.Fail("Ya existe un proveedor con ese NIT");

        var prov = _mapper.Map<Proveedor>(dto);
        prov.FechaRegistro = DateTime.UtcNow;
        prov.Activo = true;

        _context.Proveedores.Add(prov);
        await _context.SaveChangesAsync();

        return ApiResponse<ProveedorDto>.Ok(_mapper.Map<ProveedorDto>(prov), "Proveedor registrado con éxito");
    }

    public async Task<ApiResponse<ProveedorDto>> UpdateAsync(int id, ProveedorUpdateDto dto)
    {
        var prov = await _context.Proveedores.FindAsync(id);
        if (prov == null) return ApiResponse<ProveedorDto>.Fail("Proveedor no encontrado");

        if (dto.Nombre != null) prov.Nombre = dto.Nombre;
        if (dto.Nit != null) prov.Nit = dto.Nit;
        if (dto.Contacto != null) prov.Contacto = dto.Contacto;
        if (dto.Telefono != null) prov.Telefono = dto.Telefono;
        if (dto.Email != null) prov.Email = dto.Email;
        if (dto.Direccion != null) prov.Direccion = dto.Direccion;
        if (dto.Ciudad != null) prov.Ciudad = dto.Ciudad;
        if (dto.TipoInsumos != null) prov.TipoInsumos = dto.TipoInsumos;
        if (dto.Activo.HasValue) prov.Activo = dto.Activo.Value;

        await _context.SaveChangesAsync();
        return ApiResponse<ProveedorDto>.Ok(_mapper.Map<ProveedorDto>(prov), "Proveedor actualizado con éxito");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int id)
    {
        var prov = await _context.Proveedores.FindAsync(id);
        if (prov == null) return ApiResponse<bool>.Fail("Proveedor no encontrado");

        prov.Activo = false;
        await _context.SaveChangesAsync();
        return ApiResponse<bool>.Ok(true, "Proveedor desactivado correctamente");
    }
}
