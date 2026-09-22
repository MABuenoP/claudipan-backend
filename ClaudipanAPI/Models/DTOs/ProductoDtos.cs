namespace ClaudipanAPI.Models.DTOs;

public class ProductoDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public decimal? PrecioOferta { get; set; }
    public bool EnOferta { get; set; }
    public decimal CostoBaseProduccion { get; set; }
    public int Stock { get; set; }
    public string? ImagenUrl { get; set; }
    public bool Disponible { get; set; }
    public string? Presentacion { get; set; }
    public string? Marca { get; set; }
    public string? Sabor { get; set; }
    public string? Tamano { get; set; }
    public int CategoriaId { get; set; }
    public string CategoriaNombre { get; set; } = string.Empty;
}

public class ProductoCreateDto
{
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public decimal? PrecioOferta { get; set; }
    public bool EnOferta { get; set; } = false;
    public decimal CostoBaseProduccion { get; set; } = 0m;
    public int Stock { get; set; }
    public string? ImagenUrl { get; set; }
    public bool Disponible { get; set; } = true;
    public string? Presentacion { get; set; }
    public string? Marca { get; set; }
    public string? Sabor { get; set; }
    public string? Tamano { get; set; }
    public int CategoriaId { get; set; }
}

public class ProductoUpdateDto
{
    public string? Nombre { get; set; }
    public string? Descripcion { get; set; }
    public decimal? Precio { get; set; }
    public decimal? PrecioOferta { get; set; }
    public bool? EnOferta { get; set; }
    public decimal? CostoBaseProduccion { get; set; }
    public int? Stock { get; set; }
    public string? ImagenUrl { get; set; }
    public bool? Disponible { get; set; }
    public string? Presentacion { get; set; }
    public string? Marca { get; set; }
    public string? Sabor { get; set; }
    public string? Tamano { get; set; }
    public int? CategoriaId { get; set; }
}