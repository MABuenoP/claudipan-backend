using ClaudipanAPI.Helpers;
using ClaudipanAPI.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Serilog;

namespace ClaudipanAPI.Data;

/// <summary>
/// Inicializa la base de datos de Claudipan con usuarios de los 6 roles, catálogo completo de panadería,
/// bebidas, lácteos, insumos, fórmulas de producción, proveedores, compras, gastos y bajas para el SENA ADSO.
/// </summary>
public static class DbInitializer
{
    public static void Initialize(AppDbContext context)
    {
        // 0. Si la base de datos ya fue inicializada, omitir todas las consultas repetitivas de semillero
        if (context.Usuarios.Any())
        {
            // Sincronizar cualquier pedido fiado huérfano para vincularlo al cliente y registrar su deuda
            var cliente = context.Usuarios.FirstOrDefault(u => u.Rol == "Cliente");
            if (cliente != null)
            {
                var pedidosFiadosSinUsuario = context.Pedidos
                    .Where(p => p.TipoPago == "Credito_Fiado" && (p.UsuarioId == null || p.UsuarioId == 0))
                    .ToList();

                foreach (var p in pedidosFiadosSinUsuario)
                {
                    p.UsuarioId = cliente.Id;
                    p.EsInvitado = false;
                    cliente.DeudaActual += p.Total;

                    context.TransaccionesDeuda.Add(new TransaccionDeuda
                    {
                        UsuarioId = cliente.Id,
                        PedidoId = p.Id,
                        Monto = p.Total,
                        SaldoAnterior = cliente.DeudaActual - p.Total,
                        SaldoNuevo = cliente.DeudaActual,
                        Tipo = "Cargo_Credito",
                        Concepto = $"Compra a crédito (fiado) - Pedido #{p.Id}",
                        Fecha = p.FechaPedido
                    });
                }

                if (pedidosFiadosSinUsuario.Any())
                {
                    context.SaveChanges();
                }
            }
            return;
        }

        // 1. USUARIOS CON LOS 6 ROLES DEL SISTEMA
        var defaultPasswordHash = PasswordHelper.HashPassword("Admin123*");

        if (!context.Usuarios.Any(u => u.Email == "admin@claudipan.com"))
        {
            context.Usuarios.Add(new Usuario
            {
                Nombre = "Claudia Patricia Ramos (Administradora)",
                Cedula = "1098765432",
                Email = "admin@claudipan.com",
                PasswordHash = defaultPasswordHash,
                Rol = "Administrador",
                Telefono = "3101234567",
                Direccion = "Calle Principal # 10-20",
                RedesSociales = "@claudipan_oficial",
                Activo = true,
                FechaCreacion = DateTime.UtcNow
            });
        }

        if (!context.Usuarios.Any(u => u.Email == "gerente@claudipan.com"))
        {
            context.Usuarios.Add(new Usuario
            {
                Nombre = "Pedro Leyva (Gerente General)",
                Cedula = "1012345678",
                Email = "gerente@claudipan.com",
                PasswordHash = PasswordHelper.HashPassword("Gerente123*"),
                Rol = "Gerente",
                Telefono = "3129876543",
                Direccion = "Carrera 15 # 45-67",
                RedesSociales = "@pedroleyva_claudipan",
                Activo = true,
                FechaCreacion = DateTime.UtcNow
            });
        }

        if (!context.Usuarios.Any(u => u.Email == "contable@claudipan.com"))
        {
            context.Usuarios.Add(new Usuario
            {
                Nombre = "Nancy Serrano (Contadora)",
                Cedula = "1023456789",
                Email = "contable@claudipan.com",
                PasswordHash = PasswordHelper.HashPassword("Conta123*"),
                Rol = "Contable",
                Telefono = "3007654321",
                Direccion = "Carrera 5 # 12-34",
                RedesSociales = "@nancyserrano_contable",
                Activo = true,
                FechaCreacion = DateTime.UtcNow
            });
        }

        if (!context.Usuarios.Any(u => u.Email == "panadero@claudipan.com"))
        {
            context.Usuarios.Add(new Usuario
            {
                Nombre = "Maestro Panadero Carlos Gómez",
                Cedula = "1034567890",
                Email = "panadero@claudipan.com",
                PasswordHash = PasswordHelper.HashPassword("Panadero123*"),
                Rol = "Panadero",
                Telefono = "3158889900",
                Direccion = "Av. Panadería Local Central",
                RedesSociales = "@panaderiacarlos",
                Activo = true,
                FechaCreacion = DateTime.UtcNow
            });
        }

        if (!context.Usuarios.Any(u => u.Email == "vendedor@claudipan.com"))
        {
            context.Usuarios.Add(new Usuario
            {
                Nombre = "María Fernanda López (Vendedora)",
                Cedula = "1045678901",
                Email = "vendedor@claudipan.com",
                PasswordHash = PasswordHelper.HashPassword("Vendedor123*"),
                Rol = "Vendedor",
                Telefono = "3184443322",
                Direccion = "Calle 8 # 14-22",
                RedesSociales = "@mafe_ventas",
                Activo = true,
                FechaCreacion = DateTime.UtcNow
            });
        }

        if (!context.Usuarios.Any(u => u.Email == "cliente@claudipan.com"))
        {
            context.Usuarios.Add(new Usuario
            {
                Nombre = "Andrés Mauricio Silva (Cliente Fiel)",
                Cedula = "1056789012",
                Email = "cliente@claudipan.com",
                PasswordHash = PasswordHelper.HashPassword("Cliente123*"),
                Rol = "Cliente",
                Telefono = "3104445566",
                Direccion = "Calle 45 # 23-11 Barrio Los Pinos",
                RedesSociales = "@andres_silva",
                LimiteCredito = 500000m, // Cupo para fiarle de $500.000
                DeudaActual = 35000m,    // Deuda actual de ejemplo
                Activo = true,
                FechaCreacion = DateTime.UtcNow
            });
        }

        context.SaveChanges();

        // 2. CATEGORÍAS COMPLETAS
        if (!context.Categorias.Any())
        {
            var catPanes500 = new Categoria { Nombre = "Panes de $500", Descripcion = "Variedad de panes económicos y tradicionales a $500 pesos" };
            var catPanes1000 = new Categoria { Nombre = "Panes de $1.000", Descripcion = "Panes medianos redondos, cemas, queso e integrales a $1.000 pesos" };
            var catPanesFam = new Categoria { Nombre = "Panes de $2.000 y $5.000", Descripcion = "Panes familiares y grandes de leche, dulce, campesino e integral" };
            var catPanesEsp = new Categoria { Nombre = "Panes Especiales (Perros y Hamburguesas)", Descripcion = "Pan especial artesanal para perros calientes y hamburguesas" };
            var catGaseosas = new Categoria { Nombre = "Gaseosas y Bebidas Frías", Descripcion = "Gaseosas Coca-Cola, Postobón, BigCola en todos los sabores y tamaños" };
            var catLacteos = new Categoria { Nombre = "Productos Lácteos", Descripcion = "Leches, yogures, quesos y avenas de Alpina, NorLeche, LecheSan y Colanta" };
            var catCafes = new Categoria { Nombre = "Cafés y Bebidas Calientes", Descripcion = "Capuchinos, café tradicional campesino y bebidas calientes" };
            var catReposteria = new Categoria { Nombre = "Repostería y Tortas", Descripcion = "Tortas finas, bizcochos y postres artesanales" };

            context.Categorias.AddRange(catPanes500, catPanes1000, catPanesFam, catPanesEsp, catGaseosas, catLacteos, catCafes, catReposteria);
            context.SaveChanges();

            // 3. PRODUCTOS DETALLADOS DEL CATÁLOGO
            if (!context.Productos.Any())
            {
                context.Productos.AddRange(
                    // === PANES DE $500 ===
                    new Producto
                    {
                        Nombre = "Pan Bolita de Dulce",
                        Descripcion = "Suave y deliciosa bolita de pan dulce con toque de anís y azúcar espolvoreada.",
                        Precio = 500m,
                        CostoBaseProduccion = 220m,
                        Stock = 120,
                        Presentacion = "Bolitas",
                        Marca = "Claudipan",
                        Sabor = "Dulce",
                        Tamano = "$500 (Pequeño)",
                        ImagenUrl = "https://images.unsplash.com/photo-1509440159596-0249088772ff?w=500&auto=format&fit=crop",
                        Disponible = true,
                        CategoriaId = catPanes500.Id
                    },
                    new Producto
                    {
                        Nombre = "Pan Bolita de Sal",
                        Descripcion = "Tradicional bolita de pan de sal con corteza dorada y miga esponjosa.",
                        Precio = 500m,
                        CostoBaseProduccion = 210m,
                        Stock = 100,
                        Presentacion = "Bolitas",
                        Marca = "Claudipan",
                        Sabor = "Sal",
                        Tamano = "$500 (Pequeño)",
                        ImagenUrl = "https://images.unsplash.com/photo-1509440159596-0249088772ff?w=500&auto=format&fit=crop",
                        Disponible = true,
                        CategoriaId = catPanes500.Id
                    },
                    new Producto
                    {
                        Nombre = "Pan Bolita de Leche",
                        Descripcion = "Bolita de pan amasada con leche entera y mantequilla fresca.",
                        Precio = 500m,
                        CostoBaseProduccion = 240m,
                        Stock = 90,
                        Presentacion = "Bolitas",
                        Marca = "Claudipan",
                        Sabor = "Leche",
                        Tamano = "$500 (Pequeño)",
                        ImagenUrl = "https://images.unsplash.com/photo-1509440159596-0249088772ff?w=500&auto=format&fit=crop",
                        Disponible = true,
                        CategoriaId = catPanes500.Id
                    },
                    new Producto
                    {
                        Nombre = "Pan Cema Pequeña",
                        Descripcion = "Cema clásica santandereana con dulce de panela y textura crocante.",
                        Precio = 500m,
                        CostoBaseProduccion = 230m,
                        Stock = 85,
                        Presentacion = "Cemas",
                        Marca = "Claudipan",
                        Sabor = "Dulce",
                        Tamano = "$500 (Pequeño)",
                        ImagenUrl = "https://images.unsplash.com/photo-1555507036-ab1f4038808a?w=500&auto=format&fit=crop",
                        Disponible = true,
                        CategoriaId = catPanes500.Id
                    },
                    new Producto
                    {
                        Nombre = "Pan Alargado de Queso",
                        Descripcion = "Pan alargado horneado con queso costeño rallado encima.",
                        Precio = 500m,
                        CostoBaseProduccion = 260m,
                        Stock = 110,
                        Presentacion = "Alargados",
                        Marca = "Claudipan",
                        Sabor = "Queso",
                        Tamano = "$500 (Pequeño)",
                        ImagenUrl = "https://images.unsplash.com/photo-1555507036-ab1f4038808a?w=500&auto=format&fit=crop",
                        Disponible = true,
                        CategoriaId = catPanes500.Id
                    },
                    new Producto
                    {
                        Nombre = "Pan Alargado de Leche",
                        Descripcion = "Pan alargado suave con leche, ideal para mojar en chocolate caliente.",
                        Precio = 500m,
                        CostoBaseProduccion = 230m,
                        Stock = 95,
                        Presentacion = "Alargados",
                        Marca = "Claudipan",
                        Sabor = "Leche",
                        Tamano = "$500 (Pequeño)",
                        ImagenUrl = "https://images.unsplash.com/photo-1509440159596-0249088772ff?w=500&auto=format&fit=crop",
                        Disponible = true,
                        CategoriaId = catPanes500.Id
                    },
                    new Producto
                    {
                        Nombre = "Pan Alargado de Dulce",
                        Descripcion = "Pan alargado acaramelado con dulce de guayaba o miel de panela.",
                        Precio = 500m,
                        CostoBaseProduccion = 230m,
                        Stock = 80,
                        Presentacion = "Alargados",
                        Marca = "Claudipan",
                        Sabor = "Dulce",
                        Tamano = "$500 (Pequeño)",
                        ImagenUrl = "https://images.unsplash.com/photo-1509440159596-0249088772ff?w=500&auto=format&fit=crop",
                        Disponible = true,
                        CategoriaId = catPanes500.Id
                    },
                    new Producto
                    {
                        Nombre = "Pan Cascaritas Tostadas",
                        Descripcion = "Pan tipo cascarita tostada y crujiente, perfecto para el desayuno.",
                        Precio = 500m,
                        CostoBaseProduccion = 200m,
                        Stock = 75,
                        Presentacion = "Cascaritas",
                        Marca = "Claudipan",
                        Sabor = "Sal",
                        Tamano = "$500 (Pequeño)",
                        ImagenUrl = "https://images.unsplash.com/photo-1509440159596-0249088772ff?w=500&auto=format&fit=crop",
                        Disponible = true,
                        CategoriaId = catPanes500.Id
                    },
                    new Producto
                    {
                        Nombre = "Pan Alargado de Sal",
                        Descripcion = "Pan baguette individual alargado con sal y masa crujiente.",
                        Precio = 500m,
                        CostoBaseProduccion = 210m,
                        Stock = 85,
                        Presentacion = "Alargados",
                        Marca = "Claudipan",
                        Sabor = "Sal",
                        Tamano = "$500 (Pequeño)",
                        ImagenUrl = "https://images.unsplash.com/photo-1509440159596-0249088772ff?w=500&auto=format&fit=crop",
                        Disponible = true,
                        CategoriaId = catPanes500.Id
                    },

                    // === PANES DE $1.000 ===
                    new Producto
                    {
                        Nombre = "Pan Redondo de Dulce",
                        Descripcion = "Pan redondo mediano glaseado con miel y azúcar cristalizada.",
                        Precio = 1000m,
                        CostoBaseProduccion = 450m,
                        Stock = 80,
                        Presentacion = "Redondos",
                        Marca = "Claudipan",
                        Sabor = "Dulce",
                        Tamano = "$1000 (Mediano)",
                        ImagenUrl = "https://images.unsplash.com/photo-1509440159596-0249088772ff?w=500&auto=format&fit=crop",
                        Disponible = true,
                        CategoriaId = catPanes1000.Id
                    },
                    new Producto
                    {
                        Nombre = "Pan Cema Mediana",
                        Descripcion = "Cema con panela y queso, masa esponjosa de alta frescura.",
                        Precio = 1000m,
                        CostoBaseProduccion = 480m,
                        Stock = 70,
                        Presentacion = "Cemas",
                        Marca = "Claudipan",
                        Sabor = "Dulce",
                        Tamano = "$1000 (Mediano)",
                        ImagenUrl = "https://images.unsplash.com/photo-1555507036-ab1f4038808a?w=500&auto=format&fit=crop",
                        Disponible = true,
                        CategoriaId = catPanes1000.Id
                    },
                    new Producto
                    {
                        Nombre = "Pan Redondo de Queso",
                        Descripcion = "Rico pan redondo horneado con doble porción de queso campesino.",
                        Precio = 1000m,
                        CostoBaseProduccion = 520m,
                        Stock = 90,
                        Presentacion = "Redondos",
                        Marca = "Claudipan",
                        Sabor = "Queso",
                        Tamano = "$1000 (Mediano)",
                        ImagenUrl = "https://images.unsplash.com/photo-1555507036-ab1f4038808a?w=500&auto=format&fit=crop",
                        Disponible = true,
                        CategoriaId = catPanes1000.Id
                    },
                    new Producto
                    {
                        Nombre = "Pan Alargado de Dulce Mediano",
                        Descripcion = "Pan alargado suave de dulce relleno con suave toque de arequipe.",
                        Precio = 1000m,
                        CostoBaseProduccion = 470m,
                        Stock = 65,
                        Presentacion = "Alargados",
                        Marca = "Claudipan",
                        Sabor = "Dulce",
                        Tamano = "$1000 (Mediano)",
                        ImagenUrl = "https://images.unsplash.com/photo-1509440159596-0249088772ff?w=500&auto=format&fit=crop",
                        Disponible = true,
                        CategoriaId = catPanes1000.Id
                    },
                    new Producto
                    {
                        Nombre = "Pan Alargado de Leche Mediano",
                        Descripcion = "Pan alargado mediano preparado con mantequilla y leche pura.",
                        Precio = 1000m,
                        CostoBaseProduccion = 460m,
                        Stock = 70,
                        Presentacion = "Alargados",
                        Marca = "Claudipan",
                        Sabor = "Leche",
                        Tamano = "$1000 (Mediano)",
                        ImagenUrl = "https://images.unsplash.com/photo-1509440159596-0249088772ff?w=500&auto=format&fit=crop",
                        Disponible = true,
                        CategoriaId = catPanes1000.Id
                    },
                    new Producto
                    {
                        Nombre = "Pan Campesino Tradicional",
                        Descripcion = "Pan campesino de corteza rústica y masa densa tradicional.",
                        Precio = 1000m,
                        CostoBaseProduccion = 440m,
                        Stock = 60,
                        Presentacion = "Campesino",
                        Marca = "Claudipan",
                        Sabor = "Tradicional",
                        Tamano = "$1000 (Mediano)",
                        ImagenUrl = "https://images.unsplash.com/photo-1509440159596-0249088772ff?w=500&auto=format&fit=crop",
                        Disponible = true,
                        CategoriaId = catPanes1000.Id
                    },
                    new Producto
                    {
                        Nombre = "Pan Integral Individual",
                        Descripcion = "Pan integral saludable con salvado de trigo y semillas de ajonjolí.",
                        Precio = 1000m,
                        CostoBaseProduccion = 480m,
                        Stock = 50,
                        Presentacion = "Integral",
                        Marca = "Claudipan",
                        Sabor = "Integral",
                        Tamano = "$1000 (Mediano)",
                        ImagenUrl = "https://images.unsplash.com/photo-1509440159596-0249088772ff?w=500&auto=format&fit=crop",
                        Disponible = true,
                        CategoriaId = catPanes1000.Id
                    },

                    // === PANES DE $2.000 Y $5.000 ===
                    new Producto
                    {
                        Nombre = "Pan de Leche Mediano Familiar",
                        Descripcion = "Pan mediano familiar enriquecido con leche fresca y huevo.",
                        Precio = 2000m,
                        CostoBaseProduccion = 950m,
                        Stock = 40,
                        Presentacion = "Familiar",
                        Marca = "Claudipan",
                        Sabor = "Leche",
                        Tamano = "$2000 (Familiar)",
                        ImagenUrl = "https://images.unsplash.com/photo-1509440159596-0249088772ff?w=500&auto=format&fit=crop",
                        Disponible = true,
                        CategoriaId = catPanesFam.Id
                    },
                    new Producto
                    {
                        Nombre = "Pan de Dulce Mediano Familiar",
                        Descripcion = "Pan trenzado de dulce familiar con queso y azúcar.",
                        Precio = 2000m,
                        CostoBaseProduccion = 980m,
                        Stock = 35,
                        Presentacion = "Familiar",
                        Marca = "Claudipan",
                        Sabor = "Dulce",
                        Tamano = "$2000 (Familiar)",
                        ImagenUrl = "https://images.unsplash.com/photo-1509440159596-0249088772ff?w=500&auto=format&fit=crop",
                        Disponible = true,
                        CategoriaId = catPanesFam.Id
                    },
                    new Producto
                    {
                        Nombre = "Pan Campesino Grande Especial",
                        Descripcion = "Pan campesino grande artesanal para toda la familia, corteza crujiente.",
                        Precio = 5000m,
                        CostoBaseProduccion = 2400m,
                        Stock = 25,
                        Presentacion = "Campesino",
                        Marca = "Claudipan",
                        Sabor = "Campesino",
                        Tamano = "$5000 (Grande)",
                        ImagenUrl = "https://images.unsplash.com/photo-1509440159596-0249088772ff?w=500&auto=format&fit=crop",
                        Disponible = true,
                        CategoriaId = catPanesFam.Id
                    },
                    new Producto
                    {
                        Nombre = "Pan Integral Familiar Grande",
                        Descripcion = "Pan integral 100% grano entero, linaza y chía para compartir.",
                        Precio = 5000m,
                        CostoBaseProduccion = 2500m,
                        Stock = 20,
                        Presentacion = "Integral",
                        Marca = "Claudipan",
                        Sabor = "Integral",
                        Tamano = "$5000 (Grande)",
                        ImagenUrl = "https://images.unsplash.com/photo-1509440159596-0249088772ff?w=500&auto=format&fit=crop",
                        Disponible = true,
                        CategoriaId = catPanesFam.Id
                    },

                    // === PANES ESPECIALES (PERROS Y HAMBURGUESAS) ===
                    new Producto
                    {
                        Nombre = "Pan Especial para Perro Caliente (Paquete x6)",
                        Descripcion = "Paquete de 6 panes especiales alargados ultra suaves para perro caliente.",
                        Precio = 6000m,
                        CostoBaseProduccion = 2800m,
                        Stock = 45,
                        Presentacion = "Paquete x6",
                        Marca = "Claudipan",
                        Sabor = "Mantequilla",
                        Tamano = "Paquete x6",
                        ImagenUrl = "https://images.unsplash.com/photo-1627308595229-7830a5c91f9f?w=500&auto=format&fit=crop",
                        Disponible = true,
                        CategoriaId = catPanesEsp.Id
                    },
                    new Producto
                    {
                        Nombre = "Pan Especial para Hamburguesa con Ajonjolí (Paquete x6)",
                        Descripcion = "Paquete de 6 panes redondos tipo brioche con abundante ajonjolí tostado.",
                        Precio = 7000m,
                        CostoBaseProduccion = 3200m,
                        Stock = 40,
                        Presentacion = "Paquete x6",
                        Marca = "Claudipan",
                        Sabor = "Brioche Ajonjolí",
                        Tamano = "Paquete x6",
                        ImagenUrl = "https://images.unsplash.com/photo-1568901346375-23c9450c58cd?w=500&auto=format&fit=crop",
                        Disponible = true,
                        CategoriaId = catPanesEsp.Id
                    },
                    new Producto
                    {
                        Nombre = "Pan de Perro Caliente Unidad",
                        Descripcion = "Pan individual suave especial para perro caliente.",
                        Precio = 1200m,
                        CostoBaseProduccion = 550m,
                        Stock = 60,
                        Presentacion = "Unidad",
                        Marca = "Claudipan",
                        Sabor = "Mantequilla",
                        Tamano = "Unidad",
                        ImagenUrl = "https://images.unsplash.com/photo-1627308595229-7830a5c91f9f?w=500&auto=format&fit=crop",
                        Disponible = true,
                        CategoriaId = catPanesEsp.Id
                    },
                    new Producto
                    {
                        Nombre = "Pan de Hamburguesa Ajonjolí Unidad",
                        Descripcion = "Pan redondo con ajonjolí especial para hamburguesa.",
                        Precio = 1500m,
                        CostoBaseProduccion = 650m,
                        Stock = 55,
                        Presentacion = "Unidad",
                        Marca = "Claudipan",
                        Sabor = "Ajonjolí",
                        Tamano = "Unidad",
                        ImagenUrl = "https://images.unsplash.com/photo-1568901346375-23c9450c58cd?w=500&auto=format&fit=crop",
                        Disponible = true,
                        CategoriaId = catPanesEsp.Id
                    },

                    // === GASEOSAS Y BEBIDAS FRÍAS ===
                    new Producto
                    {
                        Nombre = "Coca-Cola Sabor Original Mini 250ml",
                        Descripcion = "Gaseosa Coca-Cola en botella mini refrescante.",
                        Precio = 2000m,
                        CostoBaseProduccion = 1400m,
                        Stock = 120,
                        Marca = "Coca-Cola",
                        Sabor = "Original Cola",
                        Tamano = "Mini (250ml)",
                        ImagenUrl = "https://images.unsplash.com/photo-1622483767028-3f66f32aef97?w=500&auto=format&fit=crop",
                        Disponible = true,
                        CategoriaId = catGaseosas.Id
                    },
                    new Producto
                    {
                        Nombre = "Coca-Cola Personal 350ml",
                        Descripcion = "Gaseosa Coca-Cola botella personal 350ml fría.",
                        Precio = 3000m,
                        CostoBaseProduccion = 2100m,
                        Stock = 150,
                        Marca = "Coca-Cola",
                        Sabor = "Original Cola",
                        Tamano = "Personal (350ml)",
                        ImagenUrl = "https://images.unsplash.com/photo-1622483767028-3f66f32aef97?w=500&auto=format&fit=crop",
                        Disponible = true,
                        CategoriaId = catGaseosas.Id
                    },
                    new Producto
                    {
                        Nombre = "Coca-Cola 1.5 Litros",
                        Descripcion = "Botella no retornable 1.5 litros para disfrutar en familia.",
                        Precio = 5500m,
                        CostoBaseProduccion = 4100m,
                        Stock = 60,
                        Marca = "Coca-Cola",
                        Sabor = "Original Cola",
                        Tamano = "Litro y medio (1.5L)",
                        ImagenUrl = "https://images.unsplash.com/photo-1622483767028-3f66f32aef97?w=500&auto=format&fit=crop",
                        Disponible = true,
                        CategoriaId = catGaseosas.Id
                    },
                    new Producto
                    {
                        Nombre = "Coca-Cola 3 Litros Mega",
                        Descripcion = "Presentación familiar mega 3 litros Coca-Cola.",
                        Precio = 10000m,
                        CostoBaseProduccion = 7500m,
                        Stock = 30,
                        Marca = "Coca-Cola",
                        Sabor = "Original Cola",
                        Tamano = "Tres litros (3L)",
                        ImagenUrl = "https://images.unsplash.com/photo-1622483767028-3f66f32aef97?w=500&auto=format&fit=crop",
                        Disponible = true,
                        CategoriaId = catGaseosas.Id
                    },
                    new Producto
                    {
                        Nombre = "Postobón Manzana Personal 350ml",
                        Descripcion = "Deliciosa gaseosa Postobón sabor manzana fría.",
                        Precio = 2500m,
                        CostoBaseProduccion = 1700m,
                        Stock = 80,
                        Marca = "Postobón",
                        Sabor = "Manzana",
                        Tamano = "Personal (350ml)",
                        ImagenUrl = "https://images.unsplash.com/photo-1622483767028-3f66f32aef97?w=500&auto=format&fit=crop",
                        Disponible = true,
                        CategoriaId = catGaseosas.Id
                    },
                    new Producto
                    {
                        Nombre = "Postobón Manzana 2.5 Litros",
                        Descripcion = "Gaseosa Postobón manzana 2.5 litros familiar.",
                        Precio = 7500m,
                        CostoBaseProduccion = 5400m,
                        Stock = 45,
                        Marca = "Postobón",
                        Sabor = "Manzana",
                        Tamano = "Dos litros y medio (2.5L)",
                        ImagenUrl = "https://images.unsplash.com/photo-1622483767028-3f66f32aef97?w=500&auto=format&fit=crop",
                        Disponible = true,
                        CategoriaId = catGaseosas.Id
                    },
                    new Producto
                    {
                        Nombre = "Postobón Uva Personal 350ml",
                        Descripcion = "Gaseosa Postobón con todo el sabor dulce a Uva.",
                        Precio = 2500m,
                        CostoBaseProduccion = 1700m,
                        Stock = 60,
                        Marca = "Postobón",
                        Sabor = "Uva",
                        Tamano = "Personal (350ml)",
                        ImagenUrl = "https://images.unsplash.com/photo-1622483767028-3f66f32aef97?w=500&auto=format&fit=crop",
                        Disponible = true,
                        CategoriaId = catGaseosas.Id
                    },
                    new Producto
                    {
                        Nombre = "Postobón Piña Personal 350ml",
                        Descripcion = "Refrescante gaseosa Postobón sabor Piña tropical.",
                        Precio = 2500m,
                        CostoBaseProduccion = 1700m,
                        Stock = 50,
                        Marca = "Postobón",
                        Sabor = "Piña",
                        Tamano = "Personal (350ml)",
                        ImagenUrl = "https://images.unsplash.com/photo-1622483767028-3f66f32aef97?w=500&auto=format&fit=crop",
                        Disponible = true,
                        CategoriaId = catGaseosas.Id
                    },
                    new Producto
                    {
                        Nombre = "Colombiana La Nuestra 2.5 Litros",
                        Descripcion = "Kola Colombiana tradicional 2.5 litros para compartir.",
                        Precio = 7500m,
                        CostoBaseProduccion = 5400m,
                        Stock = 40,
                        Marca = "Postobón",
                        Sabor = "Roja / Kola Colombiana",
                        Tamano = "Dos litros y medio (2.5L)",
                        ImagenUrl = "https://images.unsplash.com/photo-1622483767028-3f66f32aef97?w=500&auto=format&fit=crop",
                        Disponible = true,
                        CategoriaId = catGaseosas.Id
                    },
                    new Producto
                    {
                        Nombre = "Big Cola 3 Litros",
                        Descripcion = "Gaseosa Big Cola sabor original en mega tamaño de 3 litros.",
                        Precio = 7000m,
                        CostoBaseProduccion = 4800m,
                        Stock = 35,
                        Marca = "BigCola",
                        Sabor = "Cola Negra",
                        Tamano = "Tres litros (3L)",
                        ImagenUrl = "https://images.unsplash.com/photo-1622483767028-3f66f32aef97?w=500&auto=format&fit=crop",
                        Disponible = true,
                        CategoriaId = catGaseosas.Id
                    },

                    // === PRODUCTOS LÁCTEOS ===
                    new Producto
                    {
                        Nombre = "Leche Entera Alpina 1 Litro",
                        Descripcion = "Leche entera fresca y nutritiva marca Alpina en bolsa de 1L.",
                        Precio = 4800m,
                        CostoBaseProduccion = 3700m,
                        Stock = 50,
                        Marca = "Alpina",
                        Sabor = "Entera Natural",
                        Tamano = "Litro (1L)",
                        ImagenUrl = "https://images.unsplash.com/photo-1563636619-e9143da7973b?w=500&auto=format&fit=crop",
                        Disponible = true,
                        CategoriaId = catLacteos.Id
                    },
                    new Producto
                    {
                        Nombre = "Leche Deslactosada Colanta 1 Litro",
                        Descripcion = "Leche deslactosada de fácil digestión marca Colanta 1L.",
                        Precio = 5200m,
                        CostoBaseProduccion = 4000m,
                        Stock = 45,
                        Marca = "Colanta",
                        Sabor = "Deslactosada",
                        Tamano = "Litro (1L)",
                        ImagenUrl = "https://images.unsplash.com/photo-1563636619-e9143da7973b?w=500&auto=format&fit=crop",
                        Disponible = true,
                        CategoriaId = catLacteos.Id
                    },
                    new Producto
                    {
                        Nombre = "Leche Pasteurizada NorLeche 1 Litro",
                        Descripcion = "Leche entera pasteurizada del campo de NorLeche.",
                        Precio = 4200m,
                        CostoBaseProduccion = 3300m,
                        Stock = 40,
                        Marca = "NorLeche",
                        Sabor = "Entera",
                        Tamano = "Litro (1L)",
                        ImagenUrl = "https://images.unsplash.com/photo-1563636619-e9143da7973b?w=500&auto=format&fit=crop",
                        Disponible = true,
                        CategoriaId = catLacteos.Id
                    },
                    new Producto
                    {
                        Nombre = "Yogurt Alpina Fresa 1 Litro",
                        Descripcion = "Yogurt cremoso con trozos de fresa natural Alpina 1 Litro.",
                        Precio = 8500m,
                        CostoBaseProduccion = 6200m,
                        Stock = 30,
                        Marca = "Alpina",
                        Sabor = "Fresa",
                        Tamano = "Litro (1L)",
                        ImagenUrl = "https://images.unsplash.com/photo-1563636619-e9143da7973b?w=500&auto=format&fit=crop",
                        Disponible = true,
                        CategoriaId = catLacteos.Id
                    },
                    new Producto
                    {
                        Nombre = "Avena LecheSan Vaso 250ml",
                        Descripcion = "Nutritiva avena con canela y leche fresca marca LecheSan.",
                        Precio = 3000m,
                        CostoBaseProduccion = 2100m,
                        Stock = 40,
                        Marca = "LecheSan",
                        Sabor = "Avena Canela",
                        Tamano = "Personal (250ml)",
                        ImagenUrl = "https://images.unsplash.com/photo-1563636619-e9143da7973b?w=500&auto=format&fit=crop",
                        Disponible = true,
                        CategoriaId = catLacteos.Id
                    },
                    new Producto
                    {
                        Nombre = "Queso Costeño Campesino 500g",
                        Descripcion = "Bloque de 500g de queso costeño salado para picar o acompañar pan.",
                        Precio = 15000m,
                        CostoBaseProduccion = 11000m,
                        Stock = 20,
                        Marca = "Colanta",
                        Sabor = "Costeño Salado",
                        Tamano = "Medio Kilo (500g)",
                        ImagenUrl = "https://images.unsplash.com/photo-1552767059-ce182ead6c1b?w=500&auto=format&fit=crop",
                        Disponible = true,
                        CategoriaId = catLacteos.Id
                    },

                    // === PRODUCTOS EN OFERTA ESPECIAL ===
                    new Producto
                    {
                        Nombre = "Pan Hojaldrado Tajado Especial",
                        Descripcion = "Pan hojaldrado suave tajado para tostadas. ¡Oferta especial hasta agotar existencias!",
                        Precio = 4000m,
                        PrecioOferta = 2800m,
                        EnOferta = true,
                        CostoBaseProduccion = 1800m,
                        Stock = 25,
                        Presentacion = "Tajado",
                        Marca = "Claudipan",
                        Sabor = "Mantequilla",
                        Tamano = "Mediano",
                        ImagenUrl = "https://images.unsplash.com/photo-1509440159596-0249088772ff?w=500&auto=format&fit=crop",
                        Disponible = true,
                        CategoriaId = catPanesFam.Id
                    },
                    new Producto
                    {
                        Nombre = "Combo Desayuno: Pan de Bono + Café Capuchino",
                        Descripcion = "Pan de bono recién horneado con capuchino de 12oz. ¡Oferta imperdible!",
                        Precio = 9500m,
                        PrecioOferta = 6900m,
                        EnOferta = true,
                        CostoBaseProduccion = 3800m,
                        Stock = 18,
                        Presentacion = "Combo",
                        Marca = "Claudipan",
                        Sabor = "Queso y Vainilla",
                        Tamano = "Especial",
                        ImagenUrl = "https://images.unsplash.com/photo-1514432324607-a09d9b4aefdd?w=500&auto=format&fit=crop",
                        Disponible = true,
                        CategoriaId = catCafes.Id
                    }
                );

                context.SaveChanges();
            }

            // 4. INSUMOS DE PANADERÍA PARA PRODUCCIÓN
            if (!context.Insumos.Any())
            {
                var insHarina = new Insumo { Nombre = "Harina de Trigo Fortificada", Descripcion = "Harina especial panadera", UnidadMedida = "Kg", StockActual = 250m, CostoUnitario = 3200m, StockMinimo = 50m, ProveedorPrincipal = "Harinera del Valle" };
                var insLevadura = new Insumo { Nombre = "Levadura Fresca Levapan", Descripcion = "Levadura prensada para leudado", UnidadMedida = "Kg", StockActual = 30m, CostoUnitario = 9500m, StockMinimo = 5m, ProveedorPrincipal = "Levapan S.A." };
                var insAzucar = new Insumo { Nombre = "Azúcar Refinada Blanca", Descripcion = "Bulto de azúcar", UnidadMedida = "Kg", StockActual = 100m, CostoUnitario = 4200m, StockMinimo = 20m, ProveedorPrincipal = "Ingenio San Carlos" };
                var insMantequilla = new Insumo { Nombre = "Mantequilla Repostera con Sal", Descripcion = "Grasa vegetal panadera", UnidadMedida = "Kg", StockActual = 40m, CostoUnitario = 11000m, StockMinimo = 10m, ProveedorPrincipal = "Alpina" };
                var insQuesoCosteno = new Insumo { Nombre = "Queso Costeño Rallado", Descripcion = "Queso duro salado para panes y cemas", UnidadMedida = "Kg", StockActual = 35m, CostoUnitario = 22000m, StockMinimo = 8m, ProveedorPrincipal = "Lácteos del Campo" };
                var insHuevos = new Insumo { Nombre = "Huevos Tipo AAA", Descripcion = "Huevos frescos de granja", UnidadMedida = "Unidad", StockActual = 360m, CostoUnitario = 600m, StockMinimo = 60m, ProveedorPrincipal = "Avícola Santa Rita" };
                var insSal = new Insumo { Nombre = "Sal Marina Refinada", Descripcion = "Sal para masas panaderas", UnidadMedida = "Kg", StockActual = 25m, CostoUnitario = 1800m, StockMinimo = 5m, ProveedorPrincipal = "Refisal" };

                context.Insumos.AddRange(insHarina, insLevadura, insAzucar, insMantequilla, insQuesoCosteno, insHuevos, insSal);
                context.SaveChanges();

                // 5. RECETAS / FÓRMULAS DE PRODUCCIÓN
                var pan500 = context.Productos.FirstOrDefault(p => p.Nombre == "Pan Bolita de Dulce");
                if (pan500 != null)
                {
                    var recetaPan500 = new RecetaProduccion
                    {
                        ProductoId = pan500.Id,
                        NombreReceta = "Fórmula Estándar Bolitas de Dulce x 100 Unidades",
                        Descripcion = "Mezcla de harina, azúcar, levadura, mantequilla y huevos para 100 bolitas",
                        RendimientoUnidades = 100,
                        CostoTotalInsumos = 22000m,
                        CostoUnitarioEstimado = 220m,
                        Activo = true,
                        FechaCreacion = DateTime.UtcNow
                    };

                    recetaPan500.Detalles.Add(new DetalleReceta { InsumoId = insHarina.Id, CantidadNecesaria = 3.5m, UnidadMedida = "Kg" });
                    recetaPan500.Detalles.Add(new DetalleReceta { InsumoId = insAzucar.Id, CantidadNecesaria = 0.8m, UnidadMedida = "Kg" });
                    recetaPan500.Detalles.Add(new DetalleReceta { InsumoId = insLevadura.Id, CantidadNecesaria = 0.15m, UnidadMedida = "Kg" });
                    recetaPan500.Detalles.Add(new DetalleReceta { InsumoId = insMantequilla.Id, CantidadNecesaria = 0.4m, UnidadMedida = "Kg" });
                    recetaPan500.Detalles.Add(new DetalleReceta { InsumoId = insHuevos.Id, CantidadNecesaria = 4m, UnidadMedida = "Unidad" });

                    context.RecetasProduccion.Add(recetaPan500);
                    context.SaveChanges();
                }
            }

            // 6. PROVEEDORES
            if (!context.Proveedores.Any())
            {
                var provCoca = new Proveedor { Nombre = "Coca-Cola FEMSA Colombia S.A.", Nit = "860000001-1", Contacto = "Juan Carlos Pérez", Telefono = "3151112233", Email = "pedidos@femsa.com.co", Ciudad = "Bucaramanga", TipoInsumos = "Gaseosas y Aguas" };
                var provPosto = new Proveedor { Nombre = "Postobón S.A.", Nit = "890900050-2", Contacto = "Luz Marina Ruiz", Telefono = "3172223344", Email = "ventas@postobon.com.co", Ciudad = "Bucaramanga", TipoInsumos = "Gaseosas y Jugos" };
                var provAlpina = new Proveedor { Nombre = "Alpina Productos Alimenticios S.A.", Nit = "860025900-3", Contacto = "Mauricio Estrada", Telefono = "3183334455", Email = "distribucion@alpina.com", Ciudad = "Floridablanca", TipoInsumos = "Lácteos, Quesos y Avenas" };
                var provHarina = new Proveedor { Nombre = "Harinera del Valle S.A.", Nit = "890300120-4", Contacto = "Jorge Mendoza", Telefono = "3104445566", Email = "harinas@hvalle.com.co", Ciudad = "Cali / Distribuidora Oriente", TipoInsumos = "Harinas de Trigo e Insumos" };
                var provLevapan = new Proveedor { Nombre = "Levapan S.A.", Nit = "860000450-5", Contacto = "Sonia Gómez", Telefono = "3165556677", Email = "insumos@levapan.com", Ciudad = "Bucaramanga", TipoInsumos = "Levaduras y Mejoradores" };

                context.Proveedores.AddRange(provCoca, provPosto, provAlpina, provHarina, provLevapan);
                context.SaveChanges();

                // 7. COMPRA DE PRUEBA A PROVEEDOR
                var compraCoca = new Compra
                {
                    ProveedorId = provCoca.Id,
                    NumeroFactura = "FAC-FEMSA-98231",
                    FechaCompra = DateTime.UtcNow.AddDays(-3),
                    MetodoPago = "Transferencia",
                    EstadoPago = "Pagado",
                    Total = 210000m,
                    Observaciones = "Pedido semanal de gaseosas Coca-Cola surtidas"
                };
                context.Compras.Add(compraCoca);
                context.SaveChanges();
            }

            // 8. GASTOS DE SERVICIOS PÚBLICOS, NÓMINA E INSUMOS
            if (!context.Gastos.Any())
            {
                var adminUser = context.Usuarios.FirstOrDefault(u => u.Rol == "Administrador");

                context.Gastos.AddRange(
                    new Gasto
                    {
                        TipoGasto = "ServicioPublico",
                        CategoriaGasto = "Energía Eléctrica (Luz)",
                        Descripcion = "Recibo de energía ESSA / Enel del mes de hornos y refrigeradores",
                        Monto = 380000m,
                        Beneficiario = "Electrificadora de Santander ESSA",
                        MetodoPago = "Transferencia",
                        NumeroComprobante = "REC-ESSA-2026-09",
                        FechaGasto = DateTime.UtcNow.AddDays(-10),
                        ResponsableUsuarioId = adminUser?.Id
                    },
                    new Gasto
                    {
                        TipoGasto = "ServicioPublico",
                        CategoriaGasto = "Gas Natural",
                        Descripcion = "Recibo de gas natural Vanti para los hornos de panadería",
                        Monto = 195000m,
                        Beneficiario = "Gasoriente / Vanti",
                        MetodoPago = "Efectivo",
                        NumeroComprobante = "REC-GAS-44910",
                        FechaGasto = DateTime.UtcNow.AddDays(-8),
                        ResponsableUsuarioId = adminUser?.Id
                    },
                    new Gasto
                    {
                        TipoGasto = "ServicioPublico",
                        CategoriaGasto = "Agua y Alcantarillado",
                        Descripcion = "Recibo mensual de agua Acueducto Metropolitano",
                        Monto = 85000m,
                        Beneficiario = "Acueducto Metropolitano AMB",
                        MetodoPago = "Transferencia",
                        NumeroComprobante = "REC-AMB-8831",
                        FechaGasto = DateTime.UtcNow.AddDays(-7),
                        ResponsableUsuarioId = adminUser?.Id
                    },
                    new Gasto
                    {
                        TipoGasto = "Nomina",
                        CategoriaGasto = "Salario Maestro Panadero",
                        Descripcion = "Quincena del Maestro Panadero Carlos Gómez",
                        Monto = 950000m,
                        Beneficiario = "Carlos Gómez",
                        MetodoPago = "Transferencia",
                        NumeroComprobante = "NOM-2026-Q1",
                        FechaGasto = DateTime.UtcNow.AddDays(-5),
                        ResponsableUsuarioId = adminUser?.Id
                    },
                    new Gasto
                    {
                        TipoGasto = "Nomina",
                        CategoriaGasto = "Salario Vendedora Mostrador",
                        Descripcion = "Quincena de Vendedora María Fernanda López",
                        Monto = 750000m,
                        Beneficiario = "María Fernanda López",
                        MetodoPago = "Transferencia",
                        NumeroComprobante = "NOM-2026-Q1-V",
                        FechaGasto = DateTime.UtcNow.AddDays(-5),
                        ResponsableUsuarioId = adminUser?.Id
                    },
                    new Gasto
                    {
                        TipoGasto = "Insumo",
                        CategoriaGasto = "Compra de Azúcar y Esencias en Tienda Local",
                        Descripcion = "Compra de emergencia de 5kg de azúcar y canela en plaza de mercado",
                        Monto = 32000m,
                        Beneficiario = "Distribuidora La Esquina",
                        MetodoPago = "Efectivo",
                        NumeroComprobante = "TKT-LOCAL-551",
                        FechaGasto = DateTime.UtcNow.AddDays(-2),
                        ResponsableUsuarioId = adminUser?.Id
                    }
                );
                context.SaveChanges();
            }

            // 9. BAJAS / MERMAS DE PRODUCTOS (VENCIMIENTOS Y DAÑOS)
            if (!context.BajasProductos.Any())
            {
                var panCascarita = context.Productos.FirstOrDefault(p => p.Nombre == "Pan Cascaritas Tostadas");
                var lecheAlpina = context.Productos.FirstOrDefault(p => p.Nombre == "Leche Entera Alpina 1 Litro");
                var panaderoUser = context.Usuarios.FirstOrDefault(u => u.Rol == "Panadero");

                if (panCascarita != null)
                {
                    context.BajasProductos.Add(new BajaProducto
                    {
                        ProductoId = panCascarita.Id,
                        Cantidad = 6,
                        Motivo = "Vencimiento",
                        CostoUnitario = panCascarita.CostoBaseProduccion,
                        CostoPerdidaTotal = 6 * panCascarita.CostoBaseProduccion,
                        FechaBaja = DateTime.UtcNow.AddDays(-1),
                        UsuarioId = panaderoUser?.Id,
                        Observaciones = "Pan endurecido por fecha de vencimiento cumplida en vitrina"
                    });
                }

                if (lecheAlpina != null)
                {
                    context.BajasProductos.Add(new BajaProducto
                    {
                        ProductoId = lecheAlpina.Id,
                        Cantidad = 2,
                        Motivo = "Danado",
                        CostoUnitario = lecheAlpina.CostoBaseProduccion,
                        CostoPerdidaTotal = 2 * lecheAlpina.CostoBaseProduccion,
                        FechaBaja = DateTime.UtcNow.AddDays(-2),
                        UsuarioId = panaderoUser?.Id,
                        Observaciones = "Bolsa pinchada durante descargue del camión distribuidor"
                    });
                }

                context.SaveChanges();
            }

            // 10. PEDIDO DE MUESTRA CON FIADO / CRÉDITO Y COMPRADOR GENÉRICO
            if (!context.Pedidos.Any())
            {
                var clienteUser = context.Usuarios.FirstOrDefault(u => u.Email == "cliente@claudipan.com");
                var panBono = context.Productos.FirstOrDefault(p => p.Nombre == "Pan Redondo de Queso");
                var cocaCola = context.Productos.FirstOrDefault(p => p.Nombre == "Coca-Cola Personal 350ml");

                if (clienteUser != null && panBono != null && cocaCola != null)
                {
                    var pedidoFiado = new Pedido
                    {
                        UsuarioId = clienteUser.Id,
                        EsInvitado = false,
                        FechaPedido = DateTime.UtcNow.AddDays(-2),
                        Total = 35000m,
                        Estado = "Entregado",
                        TipoPago = "Credito_Fiado",
                        EstadoPago = "Pendiente_Credito",
                        MontoFiado = 35000m,
                        DireccionEntrega = clienteUser.Direccion,
                        Observaciones = "Compra autorizada con cargo a cupo de crédito"
                    };

                    pedidoFiado.Detalles.Add(new DetallePedido { ProductoId = panBono.Id, Cantidad = 20, PrecioUnitario = panBono.Precio });
                    pedidoFiado.Detalles.Add(new DetallePedido { ProductoId = cocaCola.Id, Cantidad = 5, PrecioUnitario = cocaCola.Precio });

                    context.Pedidos.Add(pedidoFiado);

                    context.TransaccionesDeuda.Add(new TransaccionDeuda
                    {
                        UsuarioId = clienteUser.Id,
                        Pedido = pedidoFiado,
                        Monto = 35000m,
                        SaldoAnterior = 0m,
                        SaldoNuevo = 35000m,
                        Tipo = "Cargo_Credito",
                        Concepto = "Compra a crédito (fiado) inicial",
                        Fecha = DateTime.UtcNow.AddDays(-2)
                    });

                    // Pedido de comprador genérico al contado
                    var pedidoContado = new Pedido
                    {
                        EsInvitado = true,
                        InvitadoNombre = "Comprador de Paso Mostrador",
                        FechaPedido = DateTime.UtcNow.AddDays(-1),
                        Total = 15000m,
                        Estado = "Entregado",
                        TipoPago = "Efectivo",
                        EstadoPago = "Pagado",
                        MontoFiado = 0m,
                        Observaciones = "Venta directa de mostrador en efectivo"
                    };

                    pedidoContado.Detalles.Add(new DetallePedido { ProductoId = panBono.Id, Cantidad = 10, PrecioUnitario = panBono.Precio });
                    pedidoContado.Detalles.Add(new DetallePedido { ProductoId = cocaCola.Id, Cantidad = 1, PrecioUnitario = cocaCola.Precio });
                    context.Pedidos.Add(pedidoContado);

                    context.SaveChanges();
                }
            }
        }
    }
}