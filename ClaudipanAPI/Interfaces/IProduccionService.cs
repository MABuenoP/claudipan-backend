using ClaudipanAPI.Models.DTOs;
using ClaudipanAPI.Models.Responses;

namespace ClaudipanAPI.Interfaces;

public interface IProduccionService
{
    // Recetas
    Task<ApiResponse<List<RecetaProduccionDto>>> GetAllRecetasAsync();
    Task<ApiResponse<RecetaProduccionDto>> GetRecetaByIdAsync(int id);
    Task<ApiResponse<RecetaProduccionDto>> CreateRecetaAsync(RecetaCreateDto dto);
    Task<ApiResponse<bool>> DeleteRecetaAsync(int id);

    // Órdenes de Producción
    Task<ApiResponse<List<OrdenProduccionDto>>> GetAllOrdenesAsync(int? panaderoId = null, string? estado = null);
    Task<ApiResponse<OrdenProduccionDto>> GetOrdenByIdAsync(int id);
    Task<ApiResponse<OrdenProduccionDto>> CreateOrdenAsync(int panaderoId, OrdenProduccionCreateDto dto);
    Task<ApiResponse<OrdenProduccionDto>> IniciarOrdenAsync(int id);
    Task<ApiResponse<OrdenProduccionDto>> EntregarProduccionAsync(int id, EntregarProduccionDto dto);
    Task<ApiResponse<bool>> CancelarOrdenAsync(int id);
}
