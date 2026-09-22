namespace ClaudipanAPI.Models.Entities;

public class Usuario
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Cedula { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Rol { get; set; } = "Cliente"; // Administrador, Gerente, Contable, Panadero, Vendedor, Cliente
    public string? Telefono { get; set; }
    public string? Direccion { get; set; }
    public string? RedesSociales { get; set; } // Facebook, Instagram, WhatsApp
    public decimal LimiteCredito { get; set; } = 0m; // Tope para fiar
    public decimal DeudaActual { get; set; } = 0m; // Monto actualmente adeudado
    public bool Activo { get; set; } = true;
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiry { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
    public ICollection<TransaccionDeuda> TransaccionesDeuda { get; set; } = new List<TransaccionDeuda>();
    public ICollection<OrdenProduccion> OrdenesProduccion { get; set; } = new List<OrdenProduccion>();
}