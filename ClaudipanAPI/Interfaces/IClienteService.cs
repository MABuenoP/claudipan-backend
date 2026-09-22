using ClaudipanAPI.Models.DTOs;
using ClaudipanAPI.Models.Responses;

namespace ClaudipanAPI.Interfaces;

public interface IClienteService
{
    Task<ApiResponse<List<ClienteDto>>> GetAllAsync();
    Task<ApiResponse<ClienteDto>> GetByIdAsync(int id);
    Task<ApiResponse<ClienteDto>> CreateAsync(ClienteCreateDto dto);
    Task<ApiResponse<ClienteDto>> UpdateAsync(int id, ClienteUpdateDto dto);
    Task<ApiResponse<bool>> DeleteAsync(int id);
}
