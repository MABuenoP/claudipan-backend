using ClaudipanAPI.Models.DTOs;

namespace ClaudipanAPI.Interfaces;

public interface IAuditoriaService
{
    Task RegistrarAccionAsync(
        int? usuarioId, 
        string usuarioEmail, 
        string accion, 
        string tabla, 
        string? registroId, 
        string? anterior = null, 
        string? nuevo = null, 
        string? ip = null,
        string? formulario = null,
        string? usuarioNombre = null,
        string? usuarioRol = null);

    Task<IEnumerable<AuditoriaDto>> GetAllAsync(
        string? tabla = null, 
        string? accion = null,
        string? formulario = null,
        string? busqueda = null);
}
