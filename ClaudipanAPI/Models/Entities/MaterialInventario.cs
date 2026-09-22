namespace ClaudipanAPI.Models.Entities;

public class MaterialInventario
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public decimal Cantidad { get; set; }
    public string Unidad { get; set; } = string.Empty; // kg, lt, unidad
    public string? Proveedor { get; set; }
    public decimal? StockMinimo { get; set; }
    public DateTime? FechaVencimiento { get; set; }
    public DateTime FechaActualizacion { get; set; } = DateTime.UtcNow;
}