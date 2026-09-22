namespace ClaudipanAPI.Models.DTOs;

public class RecetaProduccionDto
{
    public int Id { get; set; }
    public int ProductoId { get; set; }
    public string ProductoNombre { get; set; } = string.Empty;
    public string NombreReceta { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public int RendimientoUnidades { get; set; }
    public decimal CostoTotalInsumos { get; set; }
    public decimal CostoUnitarioEstimado { get; set; }
    public bool Activo { get; set; }
    public List<DetalleRecetaDto> Detalles { get; set; } = new();
}

public class DetalleRecetaDto
{
    public int Id { get; set; }
    public int InsumoId { get; set; }
    public string InsumoNombre { get; set; } = string.Empty;
    public decimal CantidadNecesaria { get; set; }
    public string UnidadMedida { get; set; } = "Kg";
    public decimal CostoUnitarioInsumo { get; set; }
    public decimal CostoSubtotal => CantidadNecesaria * CostoUnitarioInsumo;
}

public class RecetaCreateDto
{
    public int ProductoId { get; set; }
    public string NombreReceta { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public int RendimientoUnidades { get; set; } = 50;
    public List<DetalleRecetaCreateDto> Detalles { get; set; } = new();
}

public class DetalleRecetaCreateDto
{
    public int InsumoId { get; set; }
    public decimal CantidadNecesaria { get; set; }
    public string UnidadMedida { get; set; } = "Kg";
}

public class OrdenProduccionDto
{
    public int Id { get; set; }
    public string CodigoOrden { get; set; } = string.Empty;
    public int PanaderoUsuarioId { get; set; }
    public string PanaderoNombre { get; set; } = string.Empty;
    public int ProductoId { get; set; }
    public string ProductoNombre { get; set; } = string.Empty;
    public int? RecetaId { get; set; }
    public int CantidadProgramada { get; set; }
    public int CantidadProducida { get; set; }
    public decimal CostoInsumos { get; set; }
    public string Estado { get; set; } = string.Empty;
    public DateTime FechaOrden { get; set; }
    public DateTime? FechaEntrega { get; set; }
    public string? Observaciones { get; set; }
}

public class OrdenProduccionCreateDto
{
    public int ProductoId { get; set; }
    public int? RecetaId { get; set; }
    public int CantidadProgramada { get; set; }
    public string? Observaciones { get; set; }
}

public class EntregarProduccionDto
{
    public int CantidadProducida { get; set; }
    public string? Observaciones { get; set; }
}
