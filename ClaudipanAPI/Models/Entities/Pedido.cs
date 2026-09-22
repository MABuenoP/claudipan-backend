namespace ClaudipanAPI.Models.Entities;

public class Pedido
{
    public int Id { get; set; }
    
    // Si es un cliente registrado
    public int? UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }

    // Compatibilidad con Cliente legacy
    public int? ClienteId { get; set; }
    public Cliente? Cliente { get; set; }

    // Datos de Comprador Genérico / Invitado
    public bool EsInvitado { get; set; } = false;
    public string? InvitadoNombre { get; set; }
    public string? InvitadoEmail { get; set; }
    public string? InvitadoTelefono { get; set; }
    public string? InvitadoCedula { get; set; }

    public DateTime FechaPedido { get; set; } = DateTime.UtcNow;
    public DateTime? FechaEntrega { get; set; }
    public decimal Total { get; set; }
    public string Estado { get; set; } = "Pendiente"; // Pendiente, En preparación, Listo, Entregado, Cancelado

    // Tipo de pago y validación
    public string TipoPago { get; set; } = "Efectivo"; // Efectivo, Transferencia, Tarjeta, Credito_Fiado
    public string EstadoPago { get; set; } = "Pagado"; // Pagado, Pendiente_Credito
    public decimal MontoFiado { get; set; } = 0m;

    public string? DireccionEntrega { get; set; }
    public string? Observaciones { get; set; }

    public ICollection<DetallePedido> Detalles { get; set; } = new List<DetallePedido>();
}