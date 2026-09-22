namespace ClaudipanAPI.Models.Entities;

public class OrdenProduccion
{
    public int Id { get; set; }
    public string CodigoOrden { get; set; } = string.Empty; // Ej: ORD-2026-001
    public int PanaderoUsuarioId { get; set; }
    public Usuario? PanaderoUsuario { get; set; }

    public int ProductoId { get; set; }
    public Producto? Producto { get; set; }

    public int? RecetaId { get; set; }
    public RecetaProduccion? Receta { get; set; }

    public int CantidadProgramada { get; set; }
    public int CantidadProducida { get; set; } = 0;
    public decimal CostoInsumos { get; set; } = 0m;

    public string Estado { get; set; } = "Pendiente"; // Pendiente, En_Proceso, Terminada, Entregada, Cancelada
    public DateTime FechaOrden { get; set; } = DateTime.UtcNow;
    public DateTime? FechaEntrega { get; set; }
    public string? Observaciones { get; set; }
}
