namespace ClaudipanAPI.Models.Entities;

public class DetalleCompra
{
    public int Id { get; set; }
    public int CompraId { get; set; }
    public Compra? Compra { get; set; }

    public int? InsumoId { get; set; }
    public Insumo? Insumo { get; set; }

    public int? ProductoId { get; set; }
    public Producto? Producto { get; set; }

    public string DescripcionItem { get; set; } = string.Empty;
    public decimal Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal { get; set; }
}
