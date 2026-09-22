namespace ClaudipanAPI.Models.Entities;

public class DetalleReceta
{
    public int Id { get; set; }
    public int RecetaProduccionId { get; set; }
    public RecetaProduccion? RecetaProduccion { get; set; }

    public int InsumoId { get; set; }
    public Insumo? Insumo { get; set; }

    public decimal CantidadNecesaria { get; set; }
    public string UnidadMedida { get; set; } = "Kg";
}
