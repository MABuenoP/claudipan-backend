namespace ClaudipanAPI.Models.DTOs;

public class InsumoDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string UnidadMedida { get; set; } = "Kg";
    public decimal StockActual { get; set; }
    public decimal CostoUnitario { get; set; }
    public decimal StockMinimo { get; set; }
    public string? ProveedorPrincipal { get; set; }
    public bool Activo { get; set; }
    public DateTime FechaActualizacion { get; set; }
}

public class InsumoCreateDto
{
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string UnidadMedida { get; set; } = "Kg";
    public decimal StockActual { get; set; }
    public decimal CostoUnitario { get; set; }
    public decimal StockMinimo { get; set; } = 5m;
    public string? ProveedorPrincipal { get; set; }
}

public class InsumoUpdateDto
{
    public string? Nombre { get; set; }
    public string? Descripcion { get; set; }
    public string? UnidadMedida { get; set; }
    public decimal? StockActual { get; set; }
    public decimal? CostoUnitario { get; set; }
    public decimal? StockMinimo { get; set; }
    public string? ProveedorPrincipal { get; set; }
    public bool? Activo { get; set; }
}
