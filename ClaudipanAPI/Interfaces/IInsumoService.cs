using ClaudipanAPI.Models.DTOs;
using ClaudipanAPI.Models.Responses;

namespace ClaudipanAPI.Interfaces;

public interface IInsumoService
{
    Task<ApiResponse<List<InsumoDto>>> GetAllAsync(bool? soloActivos = true);
    Task<ApiResponse<InsumoDto>> GetByIdAsync(int id);
    Task<ApiResponse<InsumoDto>> CreateAsync(InsumoCreateDto dto);
    Task<ApiResponse<InsumoDto>> UpdateAsync(int id, InsumoUpdateDto dto);
    Task<ApiResponse<bool>> DeleteAsync(int id);
    Task<ApiResponse<bool>> AjustarStockAsync(int id, decimal cantidadDelta, decimal? nuevoCosto = null);
}
