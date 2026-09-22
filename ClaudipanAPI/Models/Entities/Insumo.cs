namespace ClaudipanAPI.Models.Entities;

public class Insumo
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string UnidadMedida { get; set; } = "Kg"; // Kg, Gr, Litro, Mililitro, Unidad, Bulto
    public decimal StockActual { get; set; } = 0m;
    public decimal CostoUnitario { get; set; } = 0m;
    public decimal StockMinimo { get; set; } = 5m;
    public string? ProveedorPrincipal { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime FechaActualizacion { get; set; } = DateTime.UtcNow;

    public ICollection<DetalleCompra> DetallesCompra { get; set; } = new List<DetalleCompra>();
    public ICollection<DetalleReceta> DetallesReceta { get; set; } = new List<DetalleReceta>();
}
