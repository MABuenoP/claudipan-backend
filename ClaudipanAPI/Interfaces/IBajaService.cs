using ClaudipanAPI.Models.DTOs;
using ClaudipanAPI.Models.Responses;

namespace ClaudipanAPI.Interfaces;

public interface IBajaService
{
    Task<ApiResponse<List<BajaProductoDto>>> GetAllAsync(int? productoId = null, string? motivo = null);
    Task<ApiResponse<BajaProductoDto>> GetByIdAsync(int id);
    Task<ApiResponse<BajaProductoDto>> CreateAsync(int? usuarioId, BajaProductoCreateDto dto);
    Task<ApiResponse<bool>> DeleteAsync(int id);
}
