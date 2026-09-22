namespace ClaudipanAPI.Models.DTOs;

public class MaterialInventarioDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public decimal Cantidad { get; set; }
    public string Unidad { get; set; } = string.Empty;
    public string? Proveedor { get; set; }
    public decimal? StockMinimo { get; set; }
    public DateTime? FechaVencimiento { get; set; }
    public bool StockBajo => StockMinimo.HasValue && Cantidad <= StockMinimo.Value;
}

public class MaterialInventarioCreateDto
{
    public string Nombre { get; set; } = string.Empty;
    public decimal Cantidad { get; set; }
    public string Unidad { get; set; } = string.Empty;
    public string? Proveedor { get; set; }
    public decimal? StockMinimo { get; set; }
    public DateTime? FechaVencimiento { get; set; }
}

public class MaterialInventarioUpdateDto
{
    public string? Nombre { get; set; }
    public decimal? Cantidad { get; set; }
    public string? Unidad { get; set; }
    public string? Proveedor { get; set; }
    public decimal? StockMinimo { get; set; }
    public DateTime? FechaVencimiento { get; set; }
}