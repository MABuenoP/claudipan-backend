using ClaudipanAPI.Models.DTOs;
using ClaudipanAPI.Models.Responses;

namespace ClaudipanAPI.Interfaces;

public interface IPedidoService
{
    Task<ApiResponse<List<PedidoDto>>> GetAllAsync(int? usuarioId = null, string? estado = null, string? tipoPago = null);
    Task<ApiResponse<PedidoDto>> GetByIdAsync(int id);
    Task<ApiResponse<PedidoDto>> CreateAsync(PedidoCreateDto dto);
    Task<ApiResponse<PedidoDto>> UpdateEstadoAsync(int id, string nuevoEstado);
    Task<ApiResponse<PedidoDto>> EntregarAsync(int id, EntregarPedidoDto dto);
    Task<ApiResponse<PedidoDto>> UpdatePedidoAsync(int id, PedidoCreateDto dto);
    Task<ApiResponse<bool>> CancelarPedidoAsync(int id);
    Task<ApiResponse<List<TransaccionDeudaDto>>> GetTransaccionesDeudaAsync(int? usuarioId = null);
    Task<ApiResponse<TransaccionDeudaDto>> RegistrarAbonoClienteAsync(RegistrarAbonoDto dto);
}