using ClaudipanAPI.Models.DTOs;
using ClaudipanAPI.Models.Responses;

namespace ClaudipanAPI.Interfaces;

public interface ICompraService
{
    Task<ApiResponse<List<CompraDto>>> GetAllAsync(int? proveedorId = null);
    Task<ApiResponse<CompraDto>> GetByIdAsync(int id);
    Task<ApiResponse<CompraDto>> CreateAsync(CompraCreateDto dto);
    Task<ApiResponse<bool>> DeleteAsync(int id);
}
