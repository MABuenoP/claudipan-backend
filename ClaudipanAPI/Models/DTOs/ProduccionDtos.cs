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
    public string? RecetaNombre { get; set; }
    public int CantidadProgramada { get; set; }
    public int CantidadProducida { get; set; }
    public int CantOptima { get; set; }
    public int CantBuenasCondiciones { get; set; }
    public int CantMalasCondiciones { get; set; }
    public string? DestinoMalasCondiciones { get; set; } // "Transformacion", "Desecho", "Ninguno"
    public decimal CostoInsumos { get; set; }
    public string Estado { get; set; } = string.Empty; // "Pendiente", "Preparando", "Horneando", "Entregada", "Cancelada"
    public bool TieneInsumosFaltantes { get; set; }
    public string? InsumosFaltantesDetalle { get; set; }
    public DateTime FechaOrden { get; set; }
    public DateTime? FechaEntrega { get; set; }
    public string? Observaciones { get; set; }
}

public class OrdenProduccionCreateDto
{
    public int ProductoId { get; set; }
    public int? RecetaId { get; set; }
    public int CantidadProgramada { get; set; }
    public int? PanaderoAsignadoId { get; set; }
    public string? Observaciones { get; set; }
}

public class CargarInsumosDto
{
    public string? Observaciones { get; set; }
}

public class PasarHorneandoDto
{
    public string? Observaciones { get; set; }
}

public class CuantificarProduccionDto
{
    public int CantOptima { get; set; }
    public int CantBuenasCondiciones { get; set; }
    public int CantMalasCondiciones { get; set; }
    public string DestinoMalasCondiciones { get; set; } = "Transformacion"; // "Transformacion" o "Desecho"
    public string? Observaciones { get; set; }
}

public class EntregarProduccionDto
{
    public int CantidadProducida { get; set; }
    public string? Observaciones { get; set; }
}

public class PreChequeoInsumosRequestDto
{
    public int ProductoId { get; set; }
    public int? RecetaId { get; set; }
    public int CantidadProgramada { get; set; }
}

public class ItemPreChequeoInsumoDto
{
    public int InsumoId { get; set; }
    public string InsumoNombre { get; set; } = string.Empty;
    public string UnidadMedida { get; set; } = "Kg";
    public decimal CantidadRequerida { get; set; }
    public decimal StockActualBodega { get; set; }
    public decimal StockResultante { get; set; }
    public bool EsDeficit { get; set; }
    public decimal CostoUnitario { get; set; }
    public decimal CostoSubtotal => CantidadRequerida * CostoUnitario;
}

public class PreChequeoInsumosResponseDto
{
    public int ProductoId { get; set; }
    public string ProductoNombre { get; set; } = string.Empty;
    public int? RecetaId { get; set; }
    public string RecetaNombre { get; set; } = string.Empty;
    public int CantidadProgramada { get; set; }
    public int RendimientoBaseReceta { get; set; }
    public decimal FactorLote { get; set; }
    public decimal CostoEstimadoTotal { get; set; }
    public bool TieneDeficit { get; set; }
    public string? InsumosFaltantesResumen { get; set; }
    public List<ItemPreChequeoInsumoDto> Insumos { get; set; } = new();
}
