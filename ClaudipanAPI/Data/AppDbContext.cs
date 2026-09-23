using System.Security.Claims;
using System.Text.Json;
using ClaudipanAPI.Models.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace ClaudipanAPI.Data;

public class AppDbContext : DbContext
{
    private readonly IHttpContextAccessor? _httpContextAccessor;

    public AppDbContext(DbContextOptions<AppDbContext> options, IHttpContextAccessor? httpContextAccessor = null) 
        : base(options) 
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<UsuarioFoto> UsuarioFotos => Set<UsuarioFoto>();
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
        modelBuilder.Entity<Pedido>().Property(p => p.CostoEnvio).HasPrecision(18, 2);
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

        // Relación Usuario y Foto (1 a 1)
        modelBuilder.Entity<Usuario>()
            .HasOne(u => u.Foto)
            .WithOne(f => f.Usuario)
            .HasForeignKey<UsuarioFoto>(f => f.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);

        // Índices
        modelBuilder.Entity<Producto>().HasIndex(p => p.Nombre);
        modelBuilder.Entity<Usuario>().HasIndex(u => u.Email).IsUnique();
        modelBuilder.Entity<Proveedor>().HasIndex(p => p.Nit);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var auditEntries = OnBeforeSaveChanges();
        var result = await base.SaveChangesAsync(cancellationToken);
        if (auditEntries.Count > 0)
        {
            await OnAfterSaveChangesAsync(auditEntries, cancellationToken);
        }
        return result;
    }

    private class InternalAuditEntry
    {
        public EntityEntry Entry { get; set; } = null!;
        public string Tabla { get; set; } = string.Empty;
        public string Accion { get; set; } = string.Empty;
        public string? RegistroId { get; set; }
        public Dictionary<string, object?> OldValues { get; } = new();
        public Dictionary<string, object?> NewValues { get; } = new();
        public List<PropertyEntry> TemporaryProperties { get; } = new();
    }

    private List<InternalAuditEntry> OnBeforeSaveChanges()
    {
        ChangeTracker.DetectChanges();
        var auditEntries = new List<InternalAuditEntry>();

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is Auditoria || entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                continue;

            var auditEntry = new InternalAuditEntry
            {
                Entry = entry,
                Tabla = entry.Metadata.GetTableName() ?? entry.Entity.GetType().Name
            };

            switch (entry.State)
            {
                case EntityState.Added:
                    auditEntry.Accion = "Crear";
                    foreach (var prop in entry.Properties)
                    {
                        if (prop.Metadata.IsPrimaryKey())
                        {
                            if (prop.IsTemporary)
                            {
                                auditEntry.TemporaryProperties.Add(prop);
                                continue;
                            }
                            auditEntry.RegistroId = prop.CurrentValue?.ToString();
                        }
                        auditEntry.NewValues[prop.Metadata.Name] = prop.CurrentValue;
                    }
                    auditEntries.Add(auditEntry);
                    break;

                case EntityState.Deleted:
                    auditEntry.Accion = "Eliminar";
                    foreach (var prop in entry.Properties)
                    {
                        if (prop.Metadata.IsPrimaryKey())
                        {
                            auditEntry.RegistroId = prop.OriginalValue?.ToString();
                        }
                        auditEntry.OldValues[prop.Metadata.Name] = prop.OriginalValue;
                    }
                    auditEntries.Add(auditEntry);
                    break;

                case EntityState.Modified:
                    auditEntry.Accion = "Modificar";
                    foreach (var prop in entry.Properties)
                    {
                        if (prop.Metadata.IsPrimaryKey())
                        {
                            auditEntry.RegistroId = prop.CurrentValue?.ToString();
                            continue;
                        }
                        if (prop.IsModified)
                        {
                            auditEntry.OldValues[prop.Metadata.Name] = prop.OriginalValue;
                            auditEntry.NewValues[prop.Metadata.Name] = prop.CurrentValue;
                        }
                    }
                    auditEntries.Add(auditEntry);
                    break;
            }
        }

        return auditEntries;
    }

    private async Task OnAfterSaveChangesAsync(List<InternalAuditEntry> auditEntries, CancellationToken cancellationToken)
    {
        try
        {
            var httpContext = _httpContextAccessor?.HttpContext;
            var ip = GetClientIp(httpContext);
            var (userId, userEmail, userName, userRole) = GetCurrentUser(httpContext);
            var formulario = GetFormulario(httpContext);

            foreach (var auditEntry in auditEntries)
            {
                foreach (var prop in auditEntry.TemporaryProperties)
                {
                    if (prop.Metadata.IsPrimaryKey())
                    {
                        auditEntry.RegistroId = prop.CurrentValue?.ToString();
                        auditEntry.NewValues[prop.Metadata.Name] = prop.CurrentValue;
                    }
                }

                if (auditEntry.OldValues.ContainsKey("PasswordHash")) auditEntry.OldValues["PasswordHash"] = "********";
                if (auditEntry.NewValues.ContainsKey("PasswordHash")) auditEntry.NewValues["PasswordHash"] = "********";

                TruncateBase64(auditEntry.OldValues);
                TruncateBase64(auditEntry.NewValues);

                var record = new Auditoria
                {
                    UsuarioId = userId,
                    UsuarioNombre = userName,
                    UsuarioEmail = userEmail,
                    UsuarioRol = userRole,
                    Accion = auditEntry.Accion,
                    TablaAfectada = auditEntry.Tabla,
                    RegistroId = auditEntry.RegistroId,
                    Formulario = formulario,
                    ValoresAnteriores = auditEntry.OldValues.Count > 0 ? JsonSerializer.Serialize(auditEntry.OldValues) : null,
                    ValoresNuevos = auditEntry.NewValues.Count > 0 ? JsonSerializer.Serialize(auditEntry.NewValues) : null,
                    Fecha = DateTime.UtcNow,
                    DireccionIp = ip
                };

                Auditorias.Add(record);
            }

            await base.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            // Evitar que el registro de auditoría interrumpa la operación
        }
    }

    private string GetClientIp(HttpContext? context)
    {
        if (context == null) return "127.0.0.1";
        if (context.Request.Headers.TryGetValue("X-Forwarded-For", out var fwd))
        {
            var ip = fwd.FirstOrDefault()?.Split(',')[0].Trim();
            if (!string.IsNullOrEmpty(ip)) return ip;
        }
        if (context.Request.Headers.TryGetValue("CF-Connecting-IP", out var cf))
        {
            var ip = cf.FirstOrDefault()?.Trim();
            if (!string.IsNullOrEmpty(ip)) return ip;
        }
        return context.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
    }

    private (int? id, string email, string? nombre, string? rol) GetCurrentUser(HttpContext? context)
    {
        if (context?.User?.Identity?.IsAuthenticated != true)
        {
            return (null, "Invitado / Sistema", null, null);
        }
        var claims = context.User;
        var idVal = claims.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? claims.FindFirst("nameid")?.Value
            ?? claims.FindFirst("sub")?.Value;
        int? id = int.TryParse(idVal, out var parsedId) ? parsedId : null;
        var email = claims.FindFirst(ClaimTypes.Email)?.Value 
            ?? claims.FindFirst("email")?.Value 
            ?? "Sistema";
        var nombre = claims.FindFirst(ClaimTypes.Name)?.Value 
            ?? claims.FindFirst("nombre")?.Value;
        var rol = claims.FindFirst(ClaimTypes.Role)?.Value 
            ?? claims.FindFirst("role")?.Value;
        return (id, email, nombre, rol);
    }

    private string GetFormulario(HttpContext? context)
    {
        if (context == null) return "Sistema Interno / Tarea";

        if (context.Request.Headers.TryGetValue("X-Form-Origin", out var formHeader) && !string.IsNullOrWhiteSpace(formHeader))
        {
            return formHeader.ToString();
        }

        var path = context.Request.Path.Value?.ToLower() ?? "";

        if (path.Contains("/api/auth/login")) return "Formulario de Login";
        if (path.Contains("/api/auth/register")) return "Formulario de Registro";
        if (path.Contains("/api/auth/reset-password")) return "Formulario de Recuperar Contraseña";
        if (path.Contains("/api/auth/profile")) return "Formulario de Perfil de Usuario";
        if (path.Contains("/api/productos")) return "Formulario de Productos (Admin Tablas)";
        if (path.Contains("/api/categorias")) return "Formulario de Categorías (Admin Tablas)";
        if (path.Contains("/api/usuarios")) return "Formulario de Usuarios (Admin Tablas)";
        if (path.Contains("/api/insumos")) return "Formulario de Insumos (Producción & Stock)";
        if (path.Contains("/api/produccion")) return "Módulo de Producción (Órdenes & Recetas)";
        if (path.Contains("/api/pedidos") && path.Contains("entregar")) return "Módulo de Despacho de Pedidos";
        if (path.Contains("/api/pedidos")) return "Carrito de Compras / Ventas";
        if (path.Contains("/api/pos")) return "Caja Rápida POS";
        if (path.Contains("/api/compras")) return "Formulario de Compras Proveedores";
        if (path.Contains("/api/gastos")) return "Formulario de Servicios & Nómina";
        if (path.Contains("/api/bajas")) return "Formulario de Bajas & Mermas";
        if (path.Contains("/api/deudas")) return "Gestión de Crédito & Mis Deudas";

        return $"Ruta: {context.Request.Method} {path}";
    }

    private void TruncateBase64(Dictionary<string, object?> dict)
    {
        foreach (var key in dict.Keys.ToList())
        {
            if (dict[key] is string s && s.Length > 200 && (key.ToLower().Contains("foto") || key.ToLower().Contains("base64") || key.ToLower().Contains("imagen") || key.ToLower().Contains("comprobante")))
            {
                dict[key] = $"[Imagen Base64 - {s.Length} caracteres]";
            }
        }
    }
}