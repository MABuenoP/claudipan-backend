namespace ClaudipanAPI.Models.DTOs;

public class CategoriaDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
}

public class CategoriaCreateDto
{
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
}

public class CategoriaUpdateDto
{
    public string? Nombre { get; set; }
    public string? Descripcion { get; set; }
}
