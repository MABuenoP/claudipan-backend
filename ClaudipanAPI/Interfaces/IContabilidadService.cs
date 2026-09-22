using ClaudipanAPI.Models.DTOs;
using ClaudipanAPI.Models.Responses;

namespace ClaudipanAPI.Interfaces;

public interface IContabilidadService
{
    Task<ApiResponse<ResumenContableDto>> GetResumenContableAsync();
    Task<ApiResponse<EstadoResultadosDto>> GetEstadoResultadosAsync(DateTime? fechaInicio = null, DateTime? fechaFin = null);
    Task<ApiResponse<List<ReporteProductoRotacionDto>>> GetProductosMasVendidosAsync(int top = 10);
    Task<ApiResponse<List<ReporteProductoRotacionDto>>> GetProductosConRezagoOPerdidasAsync();
    Task<ApiResponse<List<TransaccionDeudaDto>>> GetHistorialCreditosAsync(int? usuarioId = null);
    Task<ApiResponse<MisDeudasResumenDto>> GetMisDeudasResumenAsync(int usuarioId);
}
