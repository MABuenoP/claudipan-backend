namespace ClaudipanAPI.Models.DTOs;

public class CompraDto
{
    public int Id { get; set; }
    public int ProveedorId { get; set; }
    public string ProveedorNombre { get; set; } = string.Empty;
    public string NumeroFactura { get; set; } = string.Empty;
    public DateTime FechaCompra { get; set; }
    public decimal Total { get; set; }
    public string MetodoPago { get; set; } = string.Empty;
    public string EstadoPago { get; set; } = string.Empty;
    public string? Observaciones { get; set; }
    public List<DetalleCompraDto> Detalles { get; set; } = new();
}

public class DetalleCompraDto
{
    public int Id { get; set; }
    public int? InsumoId { get; set; }
    public string? InsumoNombre { get; set; }
    public int? ProductoId { get; set; }
    public string? ProductoNombre { get; set; }
    public string DescripcionItem { get; set; } = string.Empty;
    public decimal Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal { get; set; }
}

public class CompraCreateDto
{
    public int ProveedorId { get; set; }
    public string NumeroFactura { get; set; } = string.Empty;
    public string MetodoPago { get; set; } = "Contado";
    public string EstadoPago { get; set; } = "Pagado";
    public string? Observaciones { get; set; }
    public List<DetalleCompraCreateDto> Detalles { get; set; } = new();
}

public class DetalleCompraCreateDto
{
    public int? InsumoId { get; set; }
    public int? ProductoId { get; set; }
    public string DescripcionItem { get; set; } = string.Empty;
    public decimal Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
}
