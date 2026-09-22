using AutoMapper;
using ClaudipanAPI.Data;
using ClaudipanAPI.Interfaces;
using ClaudipanAPI.Models.DTOs;
using ClaudipanAPI.Models.Entities;
using ClaudipanAPI.Models.Responses;
using Microsoft.EntityFrameworkCore;

namespace ClaudipanAPI.Services;

public class GastoService : IGastoService
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public GastoService(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<ApiResponse<List<GastoDto>>> GetAllAsync(string? tipoGasto = null, string? categoriaGasto = null, DateTime? fechaInicio = null, DateTime? fechaFin = null)
    {
        var query = _context.Gastos
            .Include(g => g.ResponsableUsuario)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(tipoGasto))
            query = query.Where(g => g.TipoGasto == tipoGasto);

        if (!string.IsNullOrWhiteSpace(categoriaGasto))
            query = query.Where(g => g.CategoriaGasto.Contains(categoriaGasto));

        if (fechaInicio.HasValue)
            query = query.Where(g => g.FechaGasto >= fechaInicio.Value);

        if (fechaFin.HasValue)
            query = query.Where(g => g.FechaGasto <= fechaFin.Value);

        var list = await query.OrderByDescending(g => g.FechaGasto).ToListAsync();
        return ApiResponse<List<GastoDto>>.Ok(_mapper.Map<List<GastoDto>>(list));
    }

    public async Task<ApiResponse<GastoDto>> GetByIdAsync(int id)
    {
        var gasto = await _context.Gastos
            .Include(g => g.ResponsableUsuario)
            .FirstOrDefaultAsync(g => g.Id == id);

        if (gasto == null) return ApiResponse<GastoDto>.Fail("Gasto no encontrado");
        return ApiResponse<GastoDto>.Ok(_mapper.Map<GastoDto>(gasto));
    }

    public async Task<ApiResponse<GastoDto>> CreateAsync(int? usuarioId, GastoCreateDto dto)
    {
        var gasto = _mapper.Map<Gasto>(dto);
        gasto.FechaGasto = DateTime.UtcNow;
        gasto.ResponsableUsuarioId = usuarioId;

        _context.Gastos.Add(gasto);
        await _context.SaveChangesAsync();

        if (usuarioId.HasValue)
        {
            await _context.Entry(gasto).Reference(g => g.ResponsableUsuario).LoadAsync();
        }

        return ApiResponse<GastoDto>.Ok(_mapper.Map<GastoDto>(gasto), "Gasto registrado exitosamente");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int id)
    {
        var gasto = await _context.Gastos.FindAsync(id);
        if (gasto == null) return ApiResponse<bool>.Fail("Gasto no encontrado");

        _context.Gastos.Remove(gasto);
        await _context.SaveChangesAsync();
        return ApiResponse<bool>.Ok(true, "Gasto eliminado exitosamente");
    }
}
