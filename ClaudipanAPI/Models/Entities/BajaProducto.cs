namespace ClaudipanAPI.Models.Entities;

public class BajaProducto
{
    public int Id { get; set; }
    public int ProductoId { get; set; }
    public Producto? Producto { get; set; }

    public int Cantidad { get; set; }
    public string Motivo { get; set; } = "Vencimiento"; // Vencimiento, Danado, Averia, ProduccionDefectuosa, Devolucion
    public decimal CostoUnitario { get; set; } = 0m;
    public decimal CostoPerdidaTotal { get; set; } = 0m;
    public DateTime FechaBaja { get; set; } = DateTime.UtcNow;

    public int? UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }

    public string? Observaciones { get; set; }
}
