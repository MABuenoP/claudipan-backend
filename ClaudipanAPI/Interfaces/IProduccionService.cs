using ClaudipanAPI.Models.DTOs;
using ClaudipanAPI.Models.Responses;

namespace ClaudipanAPI.Interfaces;

public interface IProduccionService
{
    // Recetas
    Task<ApiResponse<List<RecetaProduccionDto>>> GetAllRecetasAsync();
    Task<ApiResponse<RecetaProduccionDto>> GetRecetaByIdAsync(int id);
    Task<ApiResponse<RecetaProduccionDto>> CreateRecetaAsync(RecetaCreateDto dto);
    Task<ApiResponse<RecetaProduccionDto>> UpdateRecetaAsync(int id, RecetaUpdateDto dto);
    Task<ApiResponse<bool>> DeleteRecetaAsync(int id);

    // Pre-chequeo de insumos para Gerente / Administrador
    Task<ApiResponse<PreChequeoInsumosResponseDto>> PreChequeoInsumosAsync(PreChequeoInsumosRequestDto dto);

    // Órdenes de Producción
    Task<ApiResponse<List<OrdenProduccionDto>>> GetAllOrdenesAsync(int? panaderoId = null, string? estado = null);
    Task<ApiResponse<OrdenProduccionDto>> GetOrdenByIdAsync(int id);
    Task<ApiResponse<OrdenProduccionDto>> CreateOrdenAsync(int creadorUsuarioId, OrdenProduccionCreateDto dto);
    
    // Flujo operativo de 3 pasos (Panadero)
    Task<ApiResponse<OrdenProduccionDto>> CargarInsumosAsync(int id, int panaderoId, CargarInsumosDto? dto = null);
    Task<ApiResponse<OrdenProduccionDto>> PasarHorneandoAsync(int id, int panaderoId, PasarHorneandoDto? dto = null);
    Task<ApiResponse<OrdenProduccionDto>> FinalizarYCuantificarAsync(int id, int panaderoId, CuantificarProduccionDto dto);
    
    // Compatibilidad y administración
    Task<ApiResponse<OrdenProduccionDto>> IniciarOrdenAsync(int id);
    Task<ApiResponse<OrdenProduccionDto>> EntregarProduccionAsync(int id, EntregarProduccionDto dto);
    Task<ApiResponse<bool>> CancelarOrdenAsync(int id);
}

