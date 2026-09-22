namespace ClaudipanAPI.Models.Entities;

public class Compra
{
    public int Id { get; set; }
    public int ProveedorId { get; set; }
    public Proveedor? Proveedor { get; set; }
    public string NumeroFactura { get; set; } = string.Empty;
    public DateTime FechaCompra { get; set; } = DateTime.UtcNow;
    public decimal Total { get; set; }
    public string MetodoPago { get; set; } = "Contado"; // Contado, Transferencia, Credito_Proveedor
    public string EstadoPago { get; set; } = "Pagado"; // Pagado, Pendiente
    public string? Observaciones { get; set; }

    public ICollection<DetalleCompra> Detalles { get; set; } = new List<DetalleCompra>();
}
