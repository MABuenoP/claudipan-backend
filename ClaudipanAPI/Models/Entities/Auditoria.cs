namespace ClaudipanAPI.Models.Entities;

public class Auditoria
{
    public int Id { get; set; }
    public int? UsuarioId { get; set; }
    public string? UsuarioNombre { get; set; }
    public string UsuarioEmail { get; set; } = "Sistema";
    public string? UsuarioRol { get; set; }
    public string Accion { get; set; } = string.Empty; // Inicio de Sesión, Registro, Crear, Modificar, Eliminar, Entregar, etc.
    public string TablaAfectada { get; set; } = string.Empty;
    public string? RegistroId { get; set; }
    public string? Formulario { get; set; } // Nombre del formulario o módulo desde donde se realizó
    public string? ValoresAnteriores { get; set; }
    public string? ValoresNuevos { get; set; }
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
    public string? DireccionIp { get; set; }
}
