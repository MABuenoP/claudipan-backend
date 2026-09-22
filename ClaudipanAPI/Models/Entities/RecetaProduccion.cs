namespace ClaudipanAPI.Models.Entities;

public class RecetaProduccion
{
    public int Id { get; set; }
    public int ProductoId { get; set; }
    public Producto? Producto { get; set; }

    public string NombreReceta { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public int RendimientoUnidades { get; set; } = 50; // Ej: Rinde 50 o 100 panes
    public decimal CostoTotalInsumos { get; set; } = 0m;
    public decimal CostoUnitarioEstimado { get; set; } = 0m;
    public bool Activo { get; set; } = true;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public ICollection<DetalleReceta> Detalles { get; set; } = new List<DetalleReceta>();
    public ICollection<OrdenProduccion> OrdenesProduccion { get; set; } = new List<OrdenProduccion>();
}
