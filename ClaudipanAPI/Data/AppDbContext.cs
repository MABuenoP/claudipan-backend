using ClaudipanAPI.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClaudipanAPI.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<Proveedor> Proveedores => Set<Proveedor>();
    public DbSet<Compra> Compras => Set<Compra>();
    public DbSet<DetalleCompra> DetallesCompra => Set<DetalleCompra>();
    public DbSet<Insumo> Insumos => Set<Insumo>();
    public DbSet<RecetaProduccion> RecetasProduccion => Set<RecetaProduccion>();
    public DbSet<DetalleReceta> DetallesReceta => Set<DetalleReceta>();
    public DbSet<OrdenProduccion> OrdenesProduccion => Set<OrdenProduccion>();
    public DbSet<Gasto> Gastos => Set<Gasto>();
    public DbSet<BajaProducto> BajasProductos => Set<BajaProducto>();
    public DbSet<Pedido> Pedidos => Set<Pedido>();
    public DbSet<DetallePedido> DetallesPedido => Set<DetallePedido>();
    public DbSet<MaterialInventario> MaterialesInventario => Set<MaterialInventario>();
    public DbSet<TransaccionDeuda> TransaccionesDeuda => Set<TransaccionDeuda>();
    public DbSet<Auditoria> Auditorias => Set<Auditoria>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Precisión de campos decimales
        modelBuilder.Entity<Usuario>().Property(u => u.LimiteCredito).HasPrecision(18, 2);
        modelBuilder.Entity<Usuario>().Property(u => u.DeudaActual).HasPrecision(18, 2);

        modelBuilder.Entity<Producto>().Property(p => p.Precio).HasPrecision(18, 2);
        modelBuilder.Entity<Producto>().Property(p => p.PrecioOferta).HasPrecision(18, 2);
        modelBuilder.Entity<Producto>().Property(p => p.CostoBaseProduccion).HasPrecision(18, 2);

        modelBuilder.Entity<Pedido>().Property(p => p.Total).HasPrecision(18, 2);
        modelBuilder.Entity<Pedido>().Property(p => p.MontoFiado).HasPrecision(18, 2);
        modelBuilder.Entity<DetallePedido>().Property(d => d.PrecioUnitario).HasPrecision(18, 2);

        modelBuilder.Entity<Insumo>().Property(i => i.StockActual).HasPrecision(18, 2);
        modelBuilder.Entity<Insumo>().Property(i => i.CostoUnitario).HasPrecision(18, 2);
        modelBuilder.Entity<Insumo>().Property(i => i.StockMinimo).HasPrecision(18, 2);

        modelBuilder.Entity<Compra>().Property(c => c.Total).HasPrecision(18, 2);
        modelBuilder.Entity<DetalleCompra>().Property(d => d.Cantidad).HasPrecision(18, 2);
        modelBuilder.Entity<DetalleCompra>().Property(d => d.PrecioUnitario).HasPrecision(18, 2);
        modelBuilder.Entity<DetalleCompra>().Property(d => d.Subtotal).HasPrecision(18, 2);

        modelBuilder.Entity<RecetaProduccion>().Property(r => r.CostoTotalInsumos).HasPrecision(18, 2);
        modelBuilder.Entity<RecetaProduccion>().Property(r => r.CostoUnitarioEstimado).HasPrecision(18, 2);
        modelBuilder.Entity<DetalleReceta>().Property(d => d.CantidadNecesaria).HasPrecision(18, 2);

        modelBuilder.Entity<OrdenProduccion>().Property(o => o.CostoInsumos).HasPrecision(18, 2);

        modelBuilder.Entity<Gasto>().Property(g => g.Monto).HasPrecision(18, 2);

        modelBuilder.Entity<BajaProducto>().Property(b => b.CostoUnitario).HasPrecision(18, 2);
        modelBuilder.Entity<BajaProducto>().Property(b => b.CostoPerdidaTotal).HasPrecision(18, 2);

        modelBuilder.Entity<TransaccionDeuda>().Property(t => t.Monto).HasPrecision(18, 2);
        modelBuilder.Entity<TransaccionDeuda>().Property(t => t.SaldoAnterior).HasPrecision(18, 2);
        modelBuilder.Entity<TransaccionDeuda>().Property(t => t.SaldoNuevo).HasPrecision(18, 2);

        modelBuilder.Entity<MaterialInventario>().Property(m => m.Cantidad).HasPrecision(18, 2);
        modelBuilder.Entity<MaterialInventario>().Property(m => m.StockMinimo).HasPrecision(18, 2);

        // Relaciones Pedido
        modelBuilder.Entity<Pedido>()
            .HasOne(p => p.Usuario)
            .WithMany(u => u.Pedidos)
            .HasForeignKey(p => p.UsuarioId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Pedido>()
            .HasOne(p => p.Cliente)
            .WithMany(c => c.Pedidos)
            .HasForeignKey(p => p.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<DetallePedido>()
            .HasOne(d => d.Pedido)
            .WithMany(p => p.Detalles)
            .HasForeignKey(d => d.PedidoId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<DetallePedido>()
            .HasOne(d => d.Producto)
            .WithMany(p => p.DetallesPedido)
            .HasForeignKey(d => d.ProductoId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relaciones Deudas / Créditos
        modelBuilder.Entity<TransaccionDeuda>()
            .HasOne(t => t.Usuario)
            .WithMany(u => u.TransaccionesDeuda)
            .HasForeignKey(t => t.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TransaccionDeuda>()
            .HasOne(t => t.Pedido)
            .WithMany()
            .HasForeignKey(t => t.PedidoId)
            .OnDelete(DeleteBehavior.SetNull);

        // Relaciones Compras y Proveedores
        modelBuilder.Entity<Compra>()
            .HasOne(c => c.Proveedor)
            .WithMany(p => p.Compras)
            .HasForeignKey(c => c.ProveedorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<DetalleCompra>()
            .HasOne(d => d.Compra)
            .WithMany(c => c.Detalles)
            .HasForeignKey(d => d.CompraId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relaciones Recetas y Producción
        modelBuilder.Entity<RecetaProduccion>()
            .HasOne(r => r.Producto)
            .WithMany()
            .HasForeignKey(r => r.ProductoId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<DetalleReceta>()
            .HasOne(d => d.RecetaProduccion)
            .WithMany(r => r.Detalles)
            .HasForeignKey(d => d.RecetaProduccionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<DetalleReceta>()
            .HasOne(d => d.Insumo)
            .WithMany(i => i.DetallesReceta)
            .HasForeignKey(d => d.InsumoId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<OrdenProduccion>()
            .HasOne(o => o.PanaderoUsuario)
            .WithMany(u => u.OrdenesProduccion)
            .HasForeignKey(o => o.PanaderoUsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<OrdenProduccion>()
            .HasOne(o => o.Producto)
            .WithMany()
            .HasForeignKey(o => o.ProductoId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relaciones Bajas
        modelBuilder.Entity<BajaProducto>()
            .HasOne(b => b.Producto)
            .WithMany(p => p.Bajas)
            .HasForeignKey(b => b.ProductoId)
            .OnDelete(DeleteBehavior.Restrict);

        // Índices
        modelBuilder.Entity<Producto>().HasIndex(p => p.Nombre);
        modelBuilder.Entity<Usuario>().HasIndex(u => u.Email).IsUnique();
        modelBuilder.Entity<Proveedor>().HasIndex(p => p.Nit);
    }
}