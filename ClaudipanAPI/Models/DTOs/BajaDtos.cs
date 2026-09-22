namespace ClaudipanAPI.Models.DTOs;

public class BajaProductoDto
{
    public int Id { get; set; }
    public int ProductoId { get; set; }
    public string ProductoNombre { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public string Motivo { get; set; } = string.Empty;
    public decimal CostoUnitario { get; set; }
    public decimal CostoPerdidaTotal { get; set; }
    public DateTime FechaBaja { get; set; }
    public int? UsuarioId { get; set; }
    public string? UsuarioNombre { get; set; }
    public string? Observaciones { get; set; }
}

public class BajaProductoCreateDto
{
    public int ProductoId { get; set; }
    public int Cantidad { get; set; }
    public string Motivo { get; set; } = "Vencimiento"; // Vencimiento, Danado, Averia, ProduccionDefectuosa
    public string? Observaciones { get; set; }
}
