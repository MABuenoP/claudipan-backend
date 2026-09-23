namespace ClaudipanAPI.Models.Entities;

public class PreRegistro
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? PrimerNombre { get; set; }
    public string? SegundoNombre { get; set; }
    public string? PrimerApellido { get; set; }
    public string? SegundoApellido { get; set; }
    public string? Cedula { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordPlana { get; set; } = string.Empty;
    public string Rol { get; set; } = "Cliente";
    public string? Telefono { get; set; }
    public string? Direccion { get; set; }
    public string? RedesSociales { get; set; }
    public decimal LimiteCredito { get; set; } = 50000m;
    public string? FotoBase64 { get; set; }

    public string TokenValidacion { get; set; } = string.Empty;
    public string TokenCancelacion { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime FechaExpiracion { get; set; } = DateTime.UtcNow.AddHours(48);

    // Estado: "Pendiente", "Validado", "Cancelado", "Expirado"
    public string Estado { get; set; } = "Pendiente";
}
