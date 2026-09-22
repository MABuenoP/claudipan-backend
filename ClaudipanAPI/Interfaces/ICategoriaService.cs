using ClaudipanAPI.Models.DTOs;
using ClaudipanAPI.Models.Responses;

namespace ClaudipanAPI.Interfaces;

public interface ICategoriaService
{
    Task<ApiResponse<List<CategoriaDto>>> GetAllAsync();
    Task<ApiResponse<CategoriaDto>> GetByIdAsync(int id);
    Task<ApiResponse<CategoriaDto>> CreateAsync(CategoriaCreateDto dto);
    Task<ApiResponse<CategoriaDto>> UpdateAsync(int id, CategoriaUpdateDto dto);
    Task<ApiResponse<bool>> DeleteAsync(int id);
}
