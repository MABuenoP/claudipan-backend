using ClaudipanAPI.Models.DTOs;
using ClaudipanAPI.Models.Responses;

namespace ClaudipanAPI.Interfaces;

public interface IInventarioService
{
    Task<ApiResponse<List<MaterialInventarioDto>>> GetAllAsync();
    Task<ApiResponse<MaterialInventarioDto>> GetByIdAsync(int id);
    Task<ApiResponse<MaterialInventarioDto>> CreateAsync(MaterialInventarioCreateDto dto);
    Task<ApiResponse<MaterialInventarioDto>> UpdateAsync(int id, MaterialInventarioUpdateDto dto);
    Task<ApiResponse<bool>> DeleteAsync(int id);
}
