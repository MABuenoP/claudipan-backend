namespace ClaudipanAPI.Models.Entities;

public class TransaccionDeuda
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }

    public int? PedidoId { get; set; }
    public Pedido? Pedido { get; set; }

    public decimal Monto { get; set; }
    public decimal SaldoAnterior { get; set; } = 0m;
    public decimal SaldoNuevo { get; set; } = 0m;
    public string Tipo { get; set; } = "Cargo_Credito"; // Cargo_Credito (fiar compra), Abono_Pago (pago de deuda)
    public string Concepto { get; set; } = string.Empty;
    public string? MetodoPagoAbono { get; set; } // Efectivo, Transferencia, Nequi, Daviplata
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
}
