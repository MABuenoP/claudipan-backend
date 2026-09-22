namespace ClaudipanAPI.Models.DTOs;

public class PedidoDto
{
    public int Id { get; set; }
    public int? UsuarioId { get; set; }
    public string ClienteNombre { get; set; } = string.Empty;
    public string ClienteEmail { get; set; } = string.Empty;
    public string? ClienteCedula { get; set; }
    public bool EsInvitado { get; set; }
    public string? InvitadoNombre { get; set; }
    public string? InvitadoEmail { get; set; }
    public string? InvitadoTelefono { get; set; }
    public string? InvitadoCedula { get; set; }
    public DateTime FechaPedido { get; set; }
    public DateTime? FechaEntrega { get; set; }
    public decimal Total { get; set; }
    public decimal MontoFiado { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string TipoPago { get; set; } = string.Empty; // Efectivo, Nequi, Transferencia, Tarjeta, Credito_Fiado
    public string EstadoPago { get; set; } = string.Empty;
    public string? ComprobanteBase64 { get; set; }
    public string? ReferenciaPago { get; set; }
    public string? DireccionEntrega { get; set; }
    public string? Observaciones { get; set; }
    public List<DetallePedidoDto> Detalles { get; set; } = new();
}

public class PedidoCreateDto
{
    public int? UsuarioId { get; set; }
    public bool EsInvitado { get; set; } = false;
    public string? InvitadoNombre { get; set; }
    public string? InvitadoEmail { get; set; }
    public string? InvitadoTelefono { get; set; }
    public string? InvitadoCedula { get; set; }

    public DateTime? FechaEntrega { get; set; }
    public string? DireccionEntrega { get; set; }
    public string? Observaciones { get; set; }

    public string TipoPago { get; set; } = "Efectivo"; // Efectivo, Nequi, Transferencia, Tarjeta, Credito_Fiado
    public string? ComprobanteBase64 { get; set; }
    public string? ReferenciaPago { get; set; }
    public List<DetallePedidoCreateDto> Detalles { get; set; } = new();
}

public class DetallePedidoCreateDto
{
    public int ProductoId { get; set; }
    public int Cantidad { get; set; }
}

public class DetallePedidoDto
{
    public int ProductoId { get; set; }
    public string ProductoNombre { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal { get; set; }
}

public class TransaccionDeudaDto
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public string UsuarioNombre { get; set; } = string.Empty;
    public int? PedidoId { get; set; }
    public decimal Monto { get; set; }
    public decimal SaldoAnterior { get; set; }
    public decimal SaldoNuevo { get; set; }
    public string Tipo { get; set; } = string.Empty; // Cargo_Credito, Abono_Pago
    public string Concepto { get; set; } = string.Empty;
    public string? MetodoPagoAbono { get; set; }
    public string? ComprobanteBase64 { get; set; }
    public string? ReferenciaPago { get; set; }
    public DateTime Fecha { get; set; }
}

public class RegistrarAbonoDto
{
    public int UsuarioId { get; set; }
    public decimal Monto { get; set; }
    public string? MetodoPago { get; set; } = "Efectivo"; // Efectivo, Nequi, Transferencia
    public string? Concepto { get; set; }
    public string? ComprobanteBase64 { get; set; }
    public string? ReferenciaPago { get; set; }
}