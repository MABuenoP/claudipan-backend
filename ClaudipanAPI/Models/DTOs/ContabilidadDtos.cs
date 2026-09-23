namespace ClaudipanAPI.Models.DTOs;

public class ResumenContableDto
{
    public decimal TotalVentas { get; set; }
    public decimal TotalCobrado { get; set; }
    public decimal CarteraPorCobrar { get; set; }
    public decimal TotalComprasProveedores { get; set; }
    public decimal TotalGastosServicios { get; set; }
    public decimal TotalGastosNomina { get; set; }
    public decimal TotalOtrosGastos { get; set; }
    public decimal TotalPerdidasBajas { get; set; }
    public decimal UtilidadBruta { get; set; }
    public decimal UtilidadNeta { get; set; }
    public int TotalPedidos { get; set; }
    public int TotalClientesConDeuda { get; set; }
}

public class EstadoResultadosDto
{
    public decimal IngresosVentas { get; set; }
    public decimal CostoVentas { get; set; }
    public decimal GananciaBruta => IngresosVentas - CostoVentas;

    public decimal GastosServiciosPublicos { get; set; }
    public decimal GastosNomina { get; set; }
    public decimal OtrosGastosOperativos { get; set; }
    public decimal TotalGastosOperativos => GastosServiciosPublicos + GastosNomina + OtrosGastosOperativos;

    public decimal PerdidasBajasMermas { get; set; }

    public decimal GananciaNeta => GananciaBruta - TotalGastosOperativos - PerdidasBajasMermas;
    public decimal MargenNetoPorcentaje => IngresosVentas > 0 ? (GananciaNeta / IngresosVentas) * 100m : 0m;
}

public class ReporteProductoRotacionDto
{
    public int ProductoId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string CategoriaNombre { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public int CantidadVendida { get; set; }
    public decimal TotalVentasGeneradas { get; set; }
    public int CantidadDadaDeBaja { get; set; }
    public decimal PerdidasGeneradas { get; set; }
    public int StockActual { get; set; }
    public string EstadoRotacion { get; set; } = "Normal"; // Alta_Rotacion, Baja_Rotacion, Rezago, Con_Perdidas
}

public class AuditoriaDto
{
    public int Id { get; set; }
    public int? UsuarioId { get; set; }
    public string? UsuarioNombre { get; set; }
    public string UsuarioEmail { get; set; } = string.Empty;
    public string? UsuarioRol { get; set; }
    public string Accion { get; set; } = string.Empty;
    public string TablaAfectada { get; set; } = string.Empty;
    public string? RegistroId { get; set; }
    public string? Formulario { get; set; }
    public string? ValoresAnteriores { get; set; }
    public string? ValoresNuevos { get; set; }
    public DateTime Fecha { get; set; }
    public string? DireccionIp { get; set; }
}

public class MisDeudasResumenDto
{
    public int UsuarioId { get; set; }
    public string ClienteNombre { get; set; } = string.Empty;
    public decimal LimiteCredito { get; set; }
    public decimal DeudaActual { get; set; }
    public decimal CupoDisponible { get; set; }
    public decimal TotalCompras { get; set; }
    public decimal TotalComprasFiadas { get; set; }
    public decimal TotalComprasContado { get; set; }
    public decimal TotalAbonos { get; set; }
    public int CantidadPedidos { get; set; }
    public List<PedidoDto> Pedidos { get; set; } = new();
    public List<TransaccionDeudaDto> Transacciones { get; set; } = new();
}
