using ClaudipanAPI.Models.DTOs;
using ClaudipanAPI.Models.Responses;

namespace ClaudipanAPI.Interfaces;

public interface IProveedorService
{
    Task<ApiResponse<List<ProveedorDto>>> GetAllAsync(bool? soloActivos = true);
    Task<ApiResponse<ProveedorDto>> GetByIdAsync(int id);
    Task<ApiResponse<ProveedorDto>> CreateAsync(ProveedorCreateDto dto);
    Task<ApiResponse<ProveedorDto>> UpdateAsync(int id, ProveedorUpdateDto dto);
    Task<ApiResponse<bool>> DeleteAsync(int id);
}
