namespace ClaudipanAPI.Models.DTOs;

public class ProveedorDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Nit { get; set; } = string.Empty;
    public string? Contacto { get; set; }
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public string? Direccion { get; set; }
    public string? Ciudad { get; set; }
    public string? TipoInsumos { get; set; }
    public bool Activo { get; set; }
    public DateTime FechaRegistro { get; set; }
}

public class ProveedorCreateDto
{
    public string Nombre { get; set; } = string.Empty;
    public string Nit { get; set; } = string.Empty;
    public string? Contacto { get; set; }
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public string? Direccion { get; set; }
    public string? Ciudad { get; set; }
    public string? TipoInsumos { get; set; }
}

public class ProveedorUpdateDto
{
    public string? Nombre { get; set; }
    public string? Nit { get; set; }
    public string? Contacto { get; set; }
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public string? Direccion { get; set; }
    public string? Ciudad { get; set; }
    public string? TipoInsumos { get; set; }
    public bool? Activo { get; set; }
}
