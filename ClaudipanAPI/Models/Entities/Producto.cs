namespace ClaudipanAPI.Models.Entities;

public class Producto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public decimal? PrecioOferta { get; set; }
    public bool EnOferta { get; set; } = false;
    public decimal CostoBaseProduccion { get; set; } = 0m;
    public int Stock { get; set; }
    public string? ImagenUrl { get; set; }
    public bool Disponible { get; set; } = true;

    // Atributos de Variantes y Clasificación
    public string? Presentacion { get; set; } // Bolitas, Cemas, Alargados, Cascaritas, Redondos, Campesino, Integral, Tajado, Entero, Baguette, Paquete
    public string? Marca { get; set; }        // Claudipan, Coca-Cola, Postobón, BigCola, Alpina, NorLeche, LecheSan, Colanta
    public string? Sabor { get; set; }        // Dulce, Sal, Leche, Queso, Manzana, Piña, Roja, Uva, Colombiana, Cola, Capuchino, Vainilla, Fresa, etc.
    public string? Tamano { get; set; }       // Mini (250ml), Personal (350ml), 8oz, 12oz, Litro (1L), 1.5L, 2L, 2.5L, 3L, Familiar, $500, $1000, $2000, $5000

    public int CategoriaId { get; set; }
    public Categoria? Categoria { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public ICollection<DetallePedido> DetallesPedido { get; set; } = new List<DetallePedido>();
    public ICollection<BajaProducto> Bajas { get; set; } = new List<BajaProducto>();
}