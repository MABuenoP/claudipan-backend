using ClaudipanAPI.Data;
using ClaudipanAPI.Helpers;
using ClaudipanAPI.Interfaces;
using ClaudipanAPI.Middleware;
using ClaudipanAPI.Models.Entities;
using ClaudipanAPI.Services;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;

// Cargar variables de entorno desde .env si existe
Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Forzar puerto 5160 para compatibilidad total con Frontend y Swagger
var defaultUrls = Environment.GetEnvironmentVariable("ASPNETCORE_URLS") ?? "http://0.0.0.0:5160;http://localhost:5160";
builder.WebHost.UseUrls(defaultUrls.Split(';'));

// Configuración de Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/claudipan-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();
builder.Host.UseSerilog();

// Connection String de SQL Server desde .env o appsettings
var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")
    ?? builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=windows21.rootservers.co\\MSSQLSERVER2019;Database=pedroley_clau;User Id=pedroley_claudi;Password=Mc98Jm03Jp04!;TrustServerCertificate=True;Encrypt=False;MultipleActiveResultSets=true";

// Entity Framework Core con SQL Server (y fallback SQLite si el servidor remoto no responde)
builder.Services.AddDbContext<AppDbContext>(options =>
{
    if (!string.IsNullOrWhiteSpace(connectionString) && connectionString.Contains("Server="))
    {
        options.UseSqlServer(connectionString, sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(5), errorNumbersToAdd: null);
        });
    }
    else
    {
        options.UseSqlite(builder.Configuration.GetConnectionString("SqliteFallback") ?? "Data Source=ClaudipanDB.db");
    }
});

// JWT Settings
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKeyString = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") 
    ?? jwtSettings["SecretKey"] 
    ?? "Claudipan_SuperSecretKey_2026_Panaderia_Artesanal_PedroLeyva_SENA_ADSO_!@#$%";
var secretKey = Encoding.UTF8.GetBytes(secretKeyString);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"] ?? "ClaudipanAPI",
        ValidAudience = jwtSettings["Audience"] ?? "ClaudipanFrontend",
        IssuerSigningKey = new SymmetricSecurityKey(secretKey),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// Servicios de aplicación
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IProductoService, ProductoService>();
builder.Services.AddScoped<ICategoriaService, CategoriaService>();
builder.Services.AddScoped<IProveedorService, ProveedorService>();
builder.Services.AddScoped<IInsumoService, InsumoService>();
builder.Services.AddScoped<ICompraService, CompraService>();
builder.Services.AddScoped<IProduccionService, ProduccionService>();
builder.Services.AddScoped<IGastoService, GastoService>();
builder.Services.AddScoped<IBajaService, BajaService>();
builder.Services.AddScoped<IPedidoService, PedidoService>();
builder.Services.AddScoped<IContabilidadService, ContabilidadService>();
builder.Services.AddScoped<IAuditoriaService, AuditoriaService>();
builder.Services.AddHttpContextAccessor();

// AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

// Controladores
builder.Services.AddControllers();

// CORS flexible para localhost y servidor de producción
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.SetIsOriginAllowed(_ => true)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Swagger con soporte JWT
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Claudipan API - SENA ADSO",
        Version = "v1",
        Description = "API REST integral para la Panadería Claudipan: Catálogo, Pedidos, Fiados, Compras, Insumos, Producción, Gastos, Bajas, P&G y Auditoría",
        Contact = new OpenApiContact
        {
            Name = "Claudipan - SENA ADSO",
            Email = "contacto@claudipan.com"
        }
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Ingrese el token JWT. Ejemplo: Bearer {token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Middleware de manejo global de errores
app.UseMiddleware<ExceptionMiddleware>();

// Swagger disponible tanto en desarrollo como en producción
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Claudipan API v1");
    c.RoutePrefix = "swagger";
});

// Redirección HTTPS solo fuera de desarrollo local
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Manejo unificado de 404 (NotFound) para rutas y archivos no habilitados
app.MapFallback(async (HttpContext context, IConfiguration config) =>
{
    context.Response.StatusCode = StatusCodes.Status404NotFound;

    var isApi = context.Request.Path.StartsWithSegments("/api");
    var acceptsJson = context.Request.Headers.Accept.Any(a => a != null && a.Contains("application/json", StringComparison.OrdinalIgnoreCase));

    if (isApi || acceptsJson)
    {
        context.Response.ContentType = "application/json; charset=utf-8";
        await context.Response.WriteAsJsonAsync(new
        {
            success = false,
            statusCode = 404,
            message = "Upsss, estás tratando de ir a un archivo que no se encuentra. Usa nuestro enlace para regresarte a nuestra pagina de inicio y gracias por tu visita.",
            inicioUrl = config["FrontendUrl"] ?? "/"
        });
        return;
    }

    context.Response.ContentType = "text/html; charset=utf-8";
    var frontendUrl = config["FrontendUrl"] ?? "/";
    await context.Response.WriteAsync(ClaudipanAPI.Helpers.NotFoundPageHelper.Render(frontendUrl));
});

// Asegurar columnas de recuperación de contraseña en tabla Usuarios
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        if (db.Database.IsSqlServer())
        {
            db.Database.ExecuteSqlRaw(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Usuarios') AND name = 'PasswordResetToken')
                BEGIN
                    ALTER TABLE Usuarios ADD PasswordResetToken NVARCHAR(200) NULL;
                END
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Usuarios') AND name = 'PasswordResetExpiry')
                BEGIN
                    ALTER TABLE Usuarios ADD PasswordResetExpiry DATETIME2 NULL;
                END
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Usuarios') AND name = 'PasswordResetHash')
                BEGIN
                    ALTER TABLE Usuarios ADD PasswordResetHash NVARCHAR(500) NULL;
                END

                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Pedidos') AND name = 'CodigoTicket')
                BEGIN
                    ALTER TABLE Pedidos ADD CodigoTicket NVARCHAR(50) NULL;
                END
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Pedidos') AND name = 'MetodoEntrega')
                BEGIN
                    ALTER TABLE Pedidos ADD MetodoEntrega NVARCHAR(50) NOT NULL DEFAULT 'Mostrador';
                END
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Pedidos') AND name = 'CostoEnvio')
                BEGIN
                    ALTER TABLE Pedidos ADD CostoEnvio DECIMAL(18,2) NOT NULL DEFAULT 0;
                END

                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Auditorias')
                BEGIN
                    CREATE TABLE Auditorias (
                        Id INT IDENTITY(1,1) PRIMARY KEY,
                        UsuarioId INT NULL,
                        UsuarioNombre NVARCHAR(150) NULL,
                        UsuarioEmail NVARCHAR(150) NOT NULL DEFAULT 'Sistema',
                        UsuarioRol NVARCHAR(50) NULL,
                        Accion NVARCHAR(100) NOT NULL,
                        TablaAfectada NVARCHAR(100) NOT NULL,
                        RegistroId NVARCHAR(100) NULL,
                        Formulario NVARCHAR(150) NULL,
                        ValoresAnteriores NVARCHAR(MAX) NULL,
                        ValoresNuevos NVARCHAR(MAX) NULL,
                        Fecha DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
                        DireccionIp NVARCHAR(50) NULL
                    );
                END
                ELSE
                BEGIN
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Auditorias') AND name = 'Formulario')
                        ALTER TABLE Auditorias ADD Formulario NVARCHAR(150) NULL;
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Auditorias') AND name = 'UsuarioNombre')
                        ALTER TABLE Auditorias ADD UsuarioNombre NVARCHAR(150) NULL;
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Auditorias') AND name = 'UsuarioRol')
                        ALTER TABLE Auditorias ADD UsuarioRol NVARCHAR(50) NULL;
                END
            ");
        }
        else if (db.Database.IsSqlite())
        {
            try { db.Database.ExecuteSqlRaw("ALTER TABLE Usuarios ADD COLUMN PasswordResetToken TEXT NULL;"); } catch { }
            try { db.Database.ExecuteSqlRaw("ALTER TABLE Usuarios ADD COLUMN PasswordResetExpiry TEXT NULL;"); } catch { }
            try { db.Database.ExecuteSqlRaw("ALTER TABLE Usuarios ADD COLUMN PasswordResetHash TEXT NULL;"); } catch { }
            try { db.Database.ExecuteSqlRaw("ALTER TABLE Pedidos ADD COLUMN CodigoTicket TEXT NULL;"); } catch { }
            try { db.Database.ExecuteSqlRaw("ALTER TABLE Pedidos ADD COLUMN MetodoEntrega TEXT DEFAULT 'Mostrador';"); } catch { }
            try { db.Database.ExecuteSqlRaw("ALTER TABLE Pedidos ADD COLUMN CostoEnvio REAL DEFAULT 0;"); } catch { }
            try {
                db.Database.ExecuteSqlRaw(@"
                    CREATE TABLE IF NOT EXISTS Auditorias (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        UsuarioId INTEGER NULL,
                        UsuarioNombre TEXT NULL,
                        UsuarioEmail TEXT NOT NULL DEFAULT 'Sistema',
                        UsuarioRol TEXT NULL,
                        Accion TEXT NOT NULL,
                        TablaAfectada TEXT NOT NULL,
                        RegistroId TEXT NULL,
                        Formulario TEXT NULL,
                        ValoresAnteriores TEXT NULL,
                        ValoresNuevos TEXT NULL,
                        Fecha TEXT NOT NULL,
                        DireccionIp TEXT NULL
                    );
                ");
            } catch { }
            try { db.Database.ExecuteSqlRaw("ALTER TABLE Auditorias ADD COLUMN Formulario TEXT NULL;"); } catch { }
            try { db.Database.ExecuteSqlRaw("ALTER TABLE Auditorias ADD COLUMN UsuarioNombre TEXT NULL;"); } catch { }
            try { db.Database.ExecuteSqlRaw("ALTER TABLE Auditorias ADD COLUMN UsuarioRol TEXT NULL;"); } catch { }
        }

        // Asegurar que el usuario 'Cliente de Paso' exista en la tabla Usuarios
        if (!db.Usuarios.Any(u => u.Email == "clientedepaso@claudipan.com"))
        {
            db.Usuarios.Add(new Usuario
            {
                Nombre = "Cliente de Paso",
                PrimerNombre = "Cliente",
                PrimerApellido = "de Paso",
                Email = "clientedepaso@claudipan.com",
                PasswordHash = PasswordHelper.HashPassword("Paso123*"),
                Rol = "Cliente",
                Direccion = "Mostrador Panadería Claudipan - Cra 5 # 10-20",
                Telefono = "3000000000",
                LimiteCredito = 0m,
                DeudaActual = 0m,
                Activo = true,
                FechaCreacion = DateTime.UtcNow
            });
            db.SaveChanges();
            Log.Information("Usuario 'Cliente de Paso' creado exitosamente en BD");
        }
    }
    catch (Exception ex)
    {
        Log.Warning("Aviso al verificar esquema y datos iniciales: {Message}", ex.Message);
    }
}

app.Run();