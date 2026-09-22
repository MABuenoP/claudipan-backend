namespace ClaudipanAPI.Models.Entities;

public class UsuarioFoto
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public string FotoBase64 { get; set; } = string.Empty;
    public DateTime FechaActualizacion { get; set; } = DateTime.UtcNow;

    public Usuario? Usuario { get; set; }
}
