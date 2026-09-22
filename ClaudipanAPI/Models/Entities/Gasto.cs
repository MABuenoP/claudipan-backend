namespace ClaudipanAPI.Models.Entities;

public class Gasto
{
    public int Id { get; set; }
    public string TipoGasto { get; set; } = "ServicioPublico"; // ServicioPublico, Nomina, Insumo, Mantenimiento, Varios
    public string CategoriaGasto { get; set; } = "Luz"; // Luz, Agua, Gas, Internet, Salario Panadero, Salario Vendedora, Harina Extra, etc.
    public string Descripcion { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public DateTime FechaGasto { get; set; } = DateTime.UtcNow;
    public string? Beneficiario { get; set; } // Enel, Acueducto, Vanti, Empleado, etc.
    public string MetodoPago { get; set; } = "Efectivo"; // Efectivo, Transferencia, Tarjeta
    public string? NumeroComprobante { get; set; }

    public int? ResponsableUsuarioId { get; set; }
    public Usuario? ResponsableUsuario { get; set; }
}
