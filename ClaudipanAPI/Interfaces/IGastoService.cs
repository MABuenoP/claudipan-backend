using ClaudipanAPI.Models.DTOs;
using ClaudipanAPI.Models.Responses;

namespace ClaudipanAPI.Interfaces;

public interface IGastoService
{
    Task<ApiResponse<List<GastoDto>>> GetAllAsync(string? tipoGasto = null, string? categoriaGasto = null, DateTime? fechaInicio = null, DateTime? fechaFin = null);
    Task<ApiResponse<GastoDto>> GetByIdAsync(int id);
    Task<ApiResponse<GastoDto>> CreateAsync(int? usuarioId, GastoCreateDto dto);
    Task<ApiResponse<bool>> DeleteAsync(int id);
}
