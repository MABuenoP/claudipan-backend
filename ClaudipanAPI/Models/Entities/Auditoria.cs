namespace ClaudipanAPI.Models.Entities;

public class Auditoria
{
    public int Id { get; set; }
    public int? UsuarioId { get; set; }
    public string UsuarioEmail { get; set; } = "Sistema";
    public string Accion { get; set; } = string.Empty; // Crear, Modificar, Eliminar, Login, Produccion, Ajuste
    public string TablaAfectada { get; set; } = string.Empty;
    public string? RegistroId { get; set; }
    public string? ValoresAnteriores { get; set; }
    public string? ValoresNuevos { get; set; }
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
    public string? DireccionIp { get; set; }
}
