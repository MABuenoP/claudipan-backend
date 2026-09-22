namespace ClaudipanAPI.Models.Requests;

public class CreateProductoRequest
{
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public int Stock { get; set; }
    public string? ImagenUrl { get; set; }
    public int CategoriaId { get; set; }
}

public class UpdateProductoRequest
{
    public string? Nombre { get; set; }
    public string? Descripcion { get; set; }
    public decimal? Precio { get; set; }
    public int? Stock { get; set; }
    public string? ImagenUrl { get; set; }
    public bool? Disponible { get; set; }
    public int? CategoriaId { get; set; }
}
