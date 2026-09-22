namespace ClaudipanAPI.Models.DTOs;

public class GastoDto
{
    public int Id { get; set; }
    public string TipoGasto { get; set; } = string.Empty;
    public string CategoriaGasto { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public DateTime FechaGasto { get; set; }
    public string? Beneficiario { get; set; }
    public string MetodoPago { get; set; } = string.Empty;
    public string? NumeroComprobante { get; set; }
    public int? ResponsableUsuarioId { get; set; }
    public string? ResponsableUsuarioNombre { get; set; }
}

public class GastoCreateDto
{
    public string TipoGasto { get; set; } = "ServicioPublico"; // ServicioPublico, Nomina, Insumo, Mantenimiento, Varios
    public string CategoriaGasto { get; set; } = "Luz"; // Luz, Agua, Gas, Internet, Salario Panadero, Salario Vendedor, Insumos Menores, etc.
    public string Descripcion { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public string? Beneficiario { get; set; }
    public string MetodoPago { get; set; } = "Efectivo";
    public string? NumeroComprobante { get; set; }
}
