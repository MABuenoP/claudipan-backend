namespace ClaudipanAPI.Models.Entities;

public class Proveedor
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Nit { get; set; } = string.Empty;
    public string? Contacto { get; set; }
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public string? Direccion { get; set; }
    public string? Ciudad { get; set; }
    public string? TipoInsumos { get; set; } // Gaseosas, Lácteos, Harinas e Insumos, Empaques, etc.
    public bool Activo { get; set; } = true;
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

    public ICollection<Compra> Compras { get; set; } = new List<Compra>();
}
