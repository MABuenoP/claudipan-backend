using ClaudipanAPI.Data;
using ClaudipanAPI.Helpers;
using ClaudipanAPI.Interfaces;
using ClaudipanAPI.Models.DTOs;
using ClaudipanAPI.Models.Entities;
using ClaudipanAPI.Models.Responses;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace ClaudipanAPI.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly IEmailService _emailService;

    public AuthService(AppDbContext context, IConfiguration configuration, IEmailService emailService)
    {
        _context = context;
        _configuration = configuration;
        _emailService = emailService;
    }

    private static string GenerateRandomPassword(int length = 8)
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
    }

    private static string BuildFullName(string? primerNombre, string? segundoNombre, string? primerApellido, string? segundoApellido, string? fallbackNombre = null)
    {
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(primerNombre)) parts.Add(primerNombre.Trim());
        if (!string.IsNullOrWhiteSpace(segundoNombre)) parts.Add(segundoNombre.Trim());
        if (!string.IsNullOrWhiteSpace(primerApellido)) parts.Add(primerApellido.Trim());
        if (!string.IsNullOrWhiteSpace(segundoApellido)) parts.Add(segundoApellido.Trim());

        if (parts.Count > 0)
            return string.Join(" ", parts);

        return fallbackNombre?.Trim() ?? string.Empty;
    }

    public async Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginRequestDto request)
    {
        var usuario = await _context.Usuarios
            .Include(u => u.Foto)
            .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower() && u.Activo);

        if (usuario == null || !PasswordHelper.VerifyPassword(request.Password, usuario.PasswordHash))
            return ApiResponse<AuthResponseDto>.Fail("Credenciales inválidas");

        var token = JwtHelper.GenerateToken(usuario, _configuration);
        var refreshToken = JwtHelper.GenerateRefreshToken();
        var expiration = DateTime.UtcNow.AddMinutes(
            double.Parse(_configuration["JwtSettings:ExpirationInMinutes"] ?? "1440"));

        usuario.RefreshToken = refreshToken;
        usuario.RefreshTokenExpiry = DateTime.UtcNow.AddDays(
            double.Parse(_configuration["JwtSettings:RefreshTokenExpirationInDays"] ?? "7"));
        await _context.SaveChangesAsync();

        return ApiResponse<AuthResponseDto>.Ok(new AuthResponseDto
        {
            Id = usuario.Id,
            Token = token,
            RefreshToken = refreshToken,
            Expiracion = expiration,
            Email = usuario.Email,
            Nombre = usuario.Nombre,
            PrimerNombre = usuario.PrimerNombre,
            SegundoNombre = usuario.SegundoNombre,
            PrimerApellido = usuario.PrimerApellido,
            SegundoApellido = usuario.SegundoApellido,
            Cedula = usuario.Cedula,
            Rol = usuario.Rol,
            Telefono = usuario.Telefono,
            Direccion = usuario.Direccion,
            RedesSociales = usuario.RedesSociales,
            LimiteCredito = usuario.LimiteCredito,
            DeudaActual = usuario.DeudaActual,
            FotoBase64 = usuario.Foto?.FotoBase64
        }, "Login exitoso");
    }

    public async Task<ApiResponse<AuthResponseDto>> RegisterAsync(RegisterRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            return ApiResponse<AuthResponseDto>.Fail("El correo es requerido");

        if (await _context.Usuarios.AnyAsync(u => u.Email.ToLower() == request.Email.ToLower()))
            return ApiResponse<AuthResponseDto>.Fail("El correo electrónico ya se encuentra registrado");

        if (!string.IsNullOrWhiteSpace(request.Cedula) && await _context.Usuarios.AnyAsync(u => u.Cedula != null && u.Cedula.Trim() == request.Cedula.Trim()))
            return ApiResponse<AuthResponseDto>.Fail("El número de cédula ya se encuentra registrado");

        if (!string.IsNullOrWhiteSpace(request.Telefono) && await _context.Usuarios.AnyAsync(u => u.Telefono != null && u.Telefono.Trim() == request.Telefono.Trim()))
            return ApiResponse<AuthResponseDto>.Fail("El número de celular ya se encuentra registrado");

        var fullName = BuildFullName(request.PrimerNombre, request.SegundoNombre, request.PrimerApellido, request.SegundoApellido, request.Nombre);

        var usuario = new Usuario
        {
            PrimerNombre = request.PrimerNombre,
            SegundoNombre = request.SegundoNombre,
            PrimerApellido = request.PrimerApellido,
            SegundoApellido = request.SegundoApellido,
            Nombre = fullName,
            Cedula = request.Cedula,
            Email = request.Email,
            PasswordHash = PasswordHelper.HashPassword(request.Password),
            Telefono = request.Telefono,
            Direccion = request.Direccion,
            RedesSociales = request.RedesSociales,
            Rol = "Cliente",
            LimiteCredito = request.LimiteCredito ?? 50000m,
            DeudaActual = 0m,
            Activo = true,
            FechaCreacion = DateTime.UtcNow
        };

        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();

        if (!string.IsNullOrWhiteSpace(request.FotoBase64))
        {
            _context.UsuarioFotos.Add(new UsuarioFoto
            {
                UsuarioId = usuario.Id,
                FotoBase64 = request.FotoBase64,
                FechaActualizacion = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
        }

        var token = JwtHelper.GenerateToken(usuario, _configuration);
        var refreshToken = JwtHelper.GenerateRefreshToken();
        var expiration = DateTime.UtcNow.AddMinutes(
            double.Parse(_configuration["JwtSettings:ExpirationInMinutes"] ?? "1440"));

        usuario.RefreshToken = refreshToken;
        usuario.RefreshTokenExpiry = DateTime.UtcNow.AddDays(
            double.Parse(_configuration["JwtSettings:RefreshTokenExpirationInDays"] ?? "7"));
        await _context.SaveChangesAsync();

        return ApiResponse<AuthResponseDto>.Ok(new AuthResponseDto
        {
            Id = usuario.Id,
            Token = token,
            RefreshToken = refreshToken,
            Expiracion = expiration,
            Email = usuario.Email,
            Nombre = usuario.Nombre,
            PrimerNombre = usuario.PrimerNombre,
            SegundoNombre = usuario.SegundoNombre,
            PrimerApellido = usuario.PrimerApellido,
            SegundoApellido = usuario.SegundoApellido,
            Cedula = usuario.Cedula,
            Rol = usuario.Rol,
            Telefono = usuario.Telefono,
            Direccion = usuario.Direccion,
            RedesSociales = usuario.RedesSociales,
            LimiteCredito = usuario.LimiteCredito,
            DeudaActual = usuario.DeudaActual,
            FotoBase64 = request.FotoBase64
        }, "Cliente registrado exitosamente");
    }

    public async Task<ApiResponse<AuthResponseDto>> RefreshTokenAsync(RefreshTokenRequestDto request)
    {
        var usuario = await _context.Usuarios
            .Include(u => u.Foto)
            .FirstOrDefaultAsync(u => u.RefreshToken == request.RefreshToken 
                && u.RefreshTokenExpiry > DateTime.UtcNow);

        if (usuario == null)
            return ApiResponse<AuthResponseDto>.Fail("Refresh token inválido o expirado");

        var newToken = JwtHelper.GenerateToken(usuario, _configuration);
        var newRefreshToken = JwtHelper.GenerateRefreshToken();
        var expiration = DateTime.UtcNow.AddMinutes(
            double.Parse(_configuration["JwtSettings:ExpirationInMinutes"] ?? "1440"));

        usuario.RefreshToken = newRefreshToken;
        usuario.RefreshTokenExpiry = DateTime.UtcNow.AddDays(
            double.Parse(_configuration["JwtSettings:RefreshTokenExpirationInDays"] ?? "7"));
        await _context.SaveChangesAsync();

        return ApiResponse<AuthResponseDto>.Ok(new AuthResponseDto
        {
            Id = usuario.Id,
            Token = newToken,
            RefreshToken = newRefreshToken,
            Expiracion = expiration,
            Email = usuario.Email,
            Nombre = usuario.Nombre,
            PrimerNombre = usuario.PrimerNombre,
            SegundoNombre = usuario.SegundoNombre,
            PrimerApellido = usuario.PrimerApellido,
            SegundoApellido = usuario.SegundoApellido,
            Cedula = usuario.Cedula,
            Rol = usuario.Rol,
            Telefono = usuario.Telefono,
            Direccion = usuario.Direccion,
            RedesSociales = usuario.RedesSociales,
            LimiteCredito = usuario.LimiteCredito,
            DeudaActual = usuario.DeudaActual,
            FotoBase64 = usuario.Foto?.FotoBase64
        }, "Token renovado exitosamente");
    }

    public async Task<ApiResponse<UserProfileDto>> GetProfileAsync(int userId)
    {
        var u = await _context.Usuarios
            .Include(x => x.Foto)
            .FirstOrDefaultAsync(x => x.Id == userId);

        if (u == null) return ApiResponse<UserProfileDto>.Fail("Usuario no encontrado");

        return ApiResponse<UserProfileDto>.Ok(new UserProfileDto
        {
            Id = u.Id,
            Nombre = u.Nombre,
            PrimerNombre = u.PrimerNombre,
            SegundoNombre = u.SegundoNombre,
            PrimerApellido = u.PrimerApellido,
            SegundoApellido = u.SegundoApellido,
            Cedula = u.Cedula,
            Email = u.Email,
            Rol = u.Rol,
            Telefono = u.Telefono,
            Direccion = u.Direccion,
            RedesSociales = u.RedesSociales,
            LimiteCredito = u.LimiteCredito,
            DeudaActual = u.DeudaActual,
            FotoBase64 = u.Foto?.FotoBase64
        });
    }

    public async Task<ApiResponse<UserProfileDto>> UpdateProfileAsync(int userId, UpdateProfileDto dto)
    {
        var u = await _context.Usuarios
            .Include(x => x.Foto)
            .FirstOrDefaultAsync(x => x.Id == userId);

        if (u == null) return ApiResponse<UserProfileDto>.Fail("Usuario no encontrado");

        if (dto.PrimerNombre != null) u.PrimerNombre = dto.PrimerNombre;
        if (dto.SegundoNombre != null) u.SegundoNombre = dto.SegundoNombre;
        if (dto.PrimerApellido != null) u.PrimerApellido = dto.PrimerApellido;
        if (dto.SegundoApellido != null) u.SegundoApellido = dto.SegundoApellido;

        u.Nombre = BuildFullName(u.PrimerNombre, u.SegundoNombre, u.PrimerApellido, u.SegundoApellido, dto.Nombre ?? u.Nombre);
        if (dto.Cedula != null) u.Cedula = dto.Cedula;
        if (!string.IsNullOrWhiteSpace(dto.Email) && dto.Email.ToLower() != u.Email.ToLower())
        {
            if (await _context.Usuarios.AnyAsync(x => x.Id != userId && x.Email.ToLower() == dto.Email.ToLower()))
                return ApiResponse<UserProfileDto>.Fail("El correo electrónico ya está en uso por otro usuario");
            u.Email = dto.Email;
        }
        if (dto.Telefono != null) u.Telefono = dto.Telefono;
        if (dto.Direccion != null) u.Direccion = dto.Direccion;
        if (dto.RedesSociales != null) u.RedesSociales = dto.RedesSociales;

        // Foto en base64 en tabla relacionada
        if (dto.FotoBase64 != null)
        {
            if (u.Foto == null)
            {
                if (!string.IsNullOrWhiteSpace(dto.FotoBase64))
                {
                    var fotoEntity = new UsuarioFoto
                    {
                        UsuarioId = u.Id,
                        FotoBase64 = dto.FotoBase64,
                        FechaActualizacion = DateTime.UtcNow
                    };
                    _context.UsuarioFotos.Add(fotoEntity);
                    u.Foto = fotoEntity;
                }
            }
            else
            {
                u.Foto.FotoBase64 = dto.FotoBase64;
                u.Foto.FechaActualizacion = DateTime.UtcNow;
            }
        }

        if (!string.IsNullOrWhiteSpace(dto.NewPassword))
        {
            if (string.IsNullOrWhiteSpace(dto.CurrentPassword) || !PasswordHelper.VerifyPassword(dto.CurrentPassword, u.PasswordHash))
            {
                return ApiResponse<UserProfileDto>.Fail("La contraseña actual es incorrecta");
            }
            u.PasswordHash = PasswordHelper.HashPassword(dto.NewPassword);
        }

        await _context.SaveChangesAsync();

        return ApiResponse<UserProfileDto>.Ok(new UserProfileDto
        {
            Id = u.Id,
            Nombre = u.Nombre,
            PrimerNombre = u.PrimerNombre,
            SegundoNombre = u.SegundoNombre,
            PrimerApellido = u.PrimerApellido,
            SegundoApellido = u.SegundoApellido,
            Cedula = u.Cedula,
            Email = u.Email,
            Rol = u.Rol,
            Telefono = u.Telefono,
            Direccion = u.Direccion,
            RedesSociales = u.RedesSociales,
            LimiteCredito = u.LimiteCredito,
            DeudaActual = u.DeudaActual,
            FotoBase64 = u.Foto?.FotoBase64
        }, "Perfil actualizado con éxito");
    }

    public async Task<ApiResponse<bool>> ChangePasswordAsync(int userId, ChangePasswordDto request)
    {
        var u = await _context.Usuarios.FindAsync(userId);
        if (u == null) return ApiResponse<bool>.Fail("Usuario no encontrado");

        if (string.IsNullOrWhiteSpace(request.CurrentPassword) || !PasswordHelper.VerifyPassword(request.CurrentPassword, u.PasswordHash))
        {
            return ApiResponse<bool>.Fail("La contraseña actual es incorrecta");
        }

        if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 6)
        {
            return ApiResponse<bool>.Fail("La nueva contraseña debe tener al menos 6 caracteres");
        }

        u.PasswordHash = PasswordHelper.HashPassword(request.NewPassword);
        await _context.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, "Contraseña actualizada exitosamente");
    }

    public async Task<ApiResponse<List<UsuarioAdminDto>>> GetAllUsersAsync()
    {
        var users = await _context.Usuarios
            .Include(u => u.Foto)
            .OrderBy(u => u.Nombre)
            .Select(u => new UsuarioAdminDto
            {
                Id = u.Id,
                Nombre = u.Nombre,
                PrimerNombre = u.PrimerNombre,
                SegundoNombre = u.SegundoNombre,
                PrimerApellido = u.PrimerApellido,
                SegundoApellido = u.SegundoApellido,
                Cedula = u.Cedula,
                Email = u.Email,
                Rol = u.Rol,
                Telefono = u.Telefono,
                Direccion = u.Direccion,
                RedesSociales = u.RedesSociales,
                LimiteCredito = u.LimiteCredito,
                DeudaActual = u.DeudaActual,
                Activo = u.Activo,
                FechaCreacion = u.FechaCreacion,
                FotoBase64 = u.Foto != null ? u.Foto.FotoBase64 : null
            })
            .ToListAsync();

        return ApiResponse<List<UsuarioAdminDto>>.Ok(users);
    }

    public async Task<ApiResponse<UsuarioAdminDto>> CreateUserAdminAsync(UpdateUsuarioAdminDto dto)
    {
        if (await _context.Usuarios.AnyAsync(u => u.Email.ToLower() == dto.Email.ToLower()))
            return ApiResponse<UsuarioAdminDto>.Fail("El correo ya se encuentra registrado");

        var fullName = BuildFullName(dto.PrimerNombre, dto.SegundoNombre, dto.PrimerApellido, dto.SegundoApellido, dto.Nombre);
        var password = string.IsNullOrWhiteSpace(dto.Password) ? "Claudipan123*" : dto.Password;

        var usuario = new Usuario
        {
            PrimerNombre = dto.PrimerNombre,
            SegundoNombre = dto.SegundoNombre,
            PrimerApellido = dto.PrimerApellido,
            SegundoApellido = dto.SegundoApellido,
            Nombre = fullName,
            Cedula = dto.Cedula,
            Email = dto.Email,
            Rol = string.IsNullOrWhiteSpace(dto.Rol) ? "Cliente" : dto.Rol,
            Telefono = dto.Telefono,
            Direccion = dto.Direccion,
            RedesSociales = dto.RedesSociales,
            LimiteCredito = dto.LimiteCredito ?? 0m,
            Activo = dto.Activo,
            PasswordHash = PasswordHelper.HashPassword(password),
            FechaCreacion = DateTime.UtcNow
        };

        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();

        if (!string.IsNullOrWhiteSpace(dto.FotoBase64))
        {
            var foto = new UsuarioFoto
            {
                UsuarioId = usuario.Id,
                FotoBase64 = dto.FotoBase64,
                FechaActualizacion = DateTime.UtcNow
            };
            _context.UsuarioFotos.Add(foto);
            usuario.Foto = foto;
            await _context.SaveChangesAsync();
        }

        return ApiResponse<UsuarioAdminDto>.Ok(new UsuarioAdminDto
        {
            Id = usuario.Id,
            Nombre = usuario.Nombre,
            PrimerNombre = usuario.PrimerNombre,
            SegundoNombre = usuario.SegundoNombre,
            PrimerApellido = usuario.PrimerApellido,
            SegundoApellido = usuario.SegundoApellido,
            Cedula = usuario.Cedula,
            Email = usuario.Email,
            Rol = usuario.Rol,
            Telefono = usuario.Telefono,
            Direccion = usuario.Direccion,
            RedesSociales = usuario.RedesSociales,
            LimiteCredito = usuario.LimiteCredito,
            DeudaActual = usuario.DeudaActual,
            Activo = usuario.Activo,
            FechaCreacion = usuario.FechaCreacion,
            FotoBase64 = usuario.Foto?.FotoBase64
        }, "Usuario creado correctamente");
    }

    public async Task<ApiResponse<UsuarioAdminDto>> UpdateUserAdminAsync(int id, UpdateUsuarioAdminDto dto)
    {
        var u = await _context.Usuarios
            .Include(x => x.Foto)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (u == null) return ApiResponse<UsuarioAdminDto>.Fail("Usuario no encontrado");

        if (dto.PrimerNombre != null) u.PrimerNombre = dto.PrimerNombre;
        if (dto.SegundoNombre != null) u.SegundoNombre = dto.SegundoNombre;
        if (dto.PrimerApellido != null) u.PrimerApellido = dto.PrimerApellido;
        if (dto.SegundoApellido != null) u.SegundoApellido = dto.SegundoApellido;

        u.Nombre = BuildFullName(u.PrimerNombre, u.SegundoNombre, u.PrimerApellido, u.SegundoApellido, dto.Nombre ?? u.Nombre);
        u.Cedula = dto.Cedula ?? u.Cedula;
        u.Email = dto.Email;
        u.Rol = dto.Rol;
        u.Telefono = dto.Telefono;
        u.Direccion = dto.Direccion;
        u.RedesSociales = dto.RedesSociales ?? u.RedesSociales;
        if (dto.LimiteCredito.HasValue)
        {
            u.LimiteCredito = dto.LimiteCredito.Value;
        }
        u.Activo = dto.Activo;

        if (!string.IsNullOrWhiteSpace(dto.Password))
        {
            u.PasswordHash = PasswordHelper.HashPassword(dto.Password);
        }

        if (dto.FotoBase64 != null)
        {
            if (u.Foto == null)
            {
                if (!string.IsNullOrWhiteSpace(dto.FotoBase64))
                {
                    var fotoEntity = new UsuarioFoto
                    {
                        UsuarioId = u.Id,
                        FotoBase64 = dto.FotoBase64,
                        FechaActualizacion = DateTime.UtcNow
                    };
                    _context.UsuarioFotos.Add(fotoEntity);
                    u.Foto = fotoEntity;
                }
            }
            else
            {
                u.Foto.FotoBase64 = dto.FotoBase64;
                u.Foto.FechaActualizacion = DateTime.UtcNow;
            }
        }

        await _context.SaveChangesAsync();

        return ApiResponse<UsuarioAdminDto>.Ok(new UsuarioAdminDto
        {
            Id = u.Id,
            Nombre = u.Nombre,
            PrimerNombre = u.PrimerNombre,
            SegundoNombre = u.SegundoNombre,
            PrimerApellido = u.PrimerApellido,
            SegundoApellido = u.SegundoApellido,
            Cedula = u.Cedula,
            Email = u.Email,
            Rol = u.Rol,
            Telefono = u.Telefono,
            Direccion = u.Direccion,
            RedesSociales = u.RedesSociales,
            LimiteCredito = u.LimiteCredito,
            DeudaActual = u.DeudaActual,
            Activo = u.Activo,
            FechaCreacion = u.FechaCreacion,
            FotoBase64 = u.Foto?.FotoBase64
        }, "Usuario actualizado correctamente");
    }

    public async Task<ApiResponse<bool>> DeleteUserAdminAsync(int id)
    {
        var u = await _context.Usuarios.FindAsync(id);
        if (u == null) return ApiResponse<bool>.Fail("Usuario no encontrado");

        u.Activo = false;
        await _context.SaveChangesAsync();
        return ApiResponse<bool>.Ok(true, "Usuario desactivado correctamente");
    }

    public async Task<ApiResponse<CheckFieldResponseDto>> CheckFieldAsync(CheckFieldDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Field) || string.IsNullOrWhiteSpace(request.Value))
        {
            return ApiResponse<CheckFieldResponseDto>.Ok(new CheckFieldResponseDto
            {
                Exists = false,
                Message = "Campo o valor vacío"
            });
        }

        var field = request.Field.Trim().ToLowerInvariant();
        var val = request.Value.Trim();
        bool exists = false;
        string fieldLabel = field;

        switch (field)
        {
            case "email":
            case "correo":
                fieldLabel = "correo electrónico";
                exists = await _context.Usuarios.AnyAsync(u => u.Email.ToLower() == val.ToLower());
                break;
            case "cedula":
            case "documento":
            case "documentoidentidad":
                fieldLabel = "número de cédula";
                exists = await _context.Usuarios.AnyAsync(u => u.Cedula != null && u.Cedula.Trim() == val);
                break;
            case "telefono":
            case "celular":
                fieldLabel = "número de celular";
                exists = await _context.Usuarios.AnyAsync(u => u.Telefono != null && u.Telefono.Trim() == val);
                break;
            default:
                return ApiResponse<CheckFieldResponseDto>.Fail($"Campo '{request.Field}' no reconocido para verificación");
        }

        return ApiResponse<CheckFieldResponseDto>.Ok(new CheckFieldResponseDto
        {
            Exists = exists,
            Message = exists ? $"El {fieldLabel} ya se encuentra registrado en Claudipan." : $"El {fieldLabel} está disponible."
        });
    }

    public async Task<ApiResponse<bool>> ForgotPasswordAsync(ForgotPasswordDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            return ApiResponse<bool>.Fail("Por favor ingresa un correo electrónico válido");

        var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.Trim().ToLower());
        if (usuario == null)
        {
            return ApiResponse<bool>.Fail("No se encontró ningún usuario registrado con ese correo electrónico");
        }

        var tempPassword = GenerateRandomPassword(8);
        var token = Guid.NewGuid().ToString("N");

        usuario.PasswordResetToken = token;
        usuario.PasswordResetExpiry = DateTime.UtcNow.AddHours(24);
        usuario.PasswordResetHash = PasswordHelper.HashPassword(tempPassword);

        await _context.SaveChangesAsync();

        var sent = await _emailService.SendPasswordResetEmailAsync(usuario.Email, usuario.Nombre, tempPassword, token);
        if (!sent)
        {
            return ApiResponse<bool>.Fail("No se pudo enviar el correo de recuperación. Por favor verifica la conexión o contacta a soporte.");
        }

        return ApiResponse<bool>.Ok(true, "Se ha enviado un correo con tu nueva contraseña y el enlace de activación.");
    }

    public async Task<ApiResponse<bool>> ResetPasswordAsync(ResetPasswordDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Token))
            return ApiResponse<bool>.Fail("Token y correo son obligatorios");

        var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.Trim().ToLower());
        if (usuario == null)
            return ApiResponse<bool>.Fail("Usuario no encontrado");

        if (string.IsNullOrEmpty(usuario.PasswordResetToken) || usuario.PasswordResetToken != request.Token.Trim())
            return ApiResponse<bool>.Fail("El token de activación es inválido o ya ha sido utilizado");

        if (usuario.PasswordResetExpiry == null || usuario.PasswordResetExpiry < DateTime.UtcNow)
            return ApiResponse<bool>.Fail("El token de activación ha expirado. Por favor solicita uno nuevo.");

        if (string.IsNullOrEmpty(usuario.PasswordResetHash))
            return ApiResponse<bool>.Fail("No hay una nueva contraseña pendiente de activación");

        usuario.PasswordHash = usuario.PasswordResetHash;
        usuario.PasswordResetToken = null;
        usuario.PasswordResetExpiry = null;
        usuario.PasswordResetHash = null;

        await _context.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, "¡Contraseña activada exitosamente! Ya puedes iniciar sesión con tu nueva contraseña.");
    }

    public async Task<ApiResponse<PreRegisterResponseDto>> PreRegisterAsync(RegisterRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            return ApiResponse<PreRegisterResponseDto>.Fail("El correo es requerido");

        if (string.IsNullOrWhiteSpace(request.Password))
            return ApiResponse<PreRegisterResponseDto>.Fail("La contraseña es requerida");

        var emailLower = request.Email.Trim().ToLower();

        if (await _context.Usuarios.AnyAsync(u => u.Email.ToLower() == emailLower))
            return ApiResponse<PreRegisterResponseDto>.Fail("El correo electrónico ya se encuentra registrado");

        if (!string.IsNullOrWhiteSpace(request.Cedula) && await _context.Usuarios.AnyAsync(u => u.Cedula != null && u.Cedula.Trim() == request.Cedula.Trim()))
            return ApiResponse<PreRegisterResponseDto>.Fail("El número de cédula ya se encuentra registrado");

        if (!string.IsNullOrWhiteSpace(request.Telefono) && await _context.Usuarios.AnyAsync(u => u.Telefono != null && u.Telefono.Trim() == request.Telefono.Trim()))
            return ApiResponse<PreRegisterResponseDto>.Fail("El número de celular ya se encuentra registrado");

        // Cancelar prerregistros pendientes previos para este mismo correo
        var pendingPrevious = await _context.PreRegistros
            .Where(p => p.Email.ToLower() == emailLower && p.Estado == "Pendiente")
            .ToListAsync();
        foreach (var prev in pendingPrevious)
        {
            prev.Estado = "Cancelado";
        }

        var fullName = BuildFullName(request.PrimerNombre, request.SegundoNombre, request.PrimerApellido, request.SegundoApellido, request.Nombre);

        var tokenValidacion = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
        var tokenCancelacion = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");

        var preregistro = new PreRegistro
        {
            PrimerNombre = request.PrimerNombre,
            SegundoNombre = request.SegundoNombre,
            PrimerApellido = request.PrimerApellido,
            SegundoApellido = request.SegundoApellido,
            Nombre = fullName,
            Cedula = request.Cedula,
            Email = request.Email.Trim(),
            PasswordPlana = request.Password,
            Telefono = request.Telefono,
            Direccion = request.Direccion,
            RedesSociales = request.RedesSociales,
            Rol = "Cliente",
            LimiteCredito = request.LimiteCredito ?? 50000m,
            FotoBase64 = request.FotoBase64,
            TokenValidacion = tokenValidacion,
            TokenCancelacion = tokenCancelacion,
            FechaCreacion = DateTime.UtcNow,
            FechaExpiracion = DateTime.UtcNow.AddHours(48),
            Estado = "Pendiente"
        };

        _context.PreRegistros.Add(preregistro);
        await _context.SaveChangesAsync();

        // Enviar correo con token y datos
        var sent = await _emailService.SendPreRegisterEmailAsync(preregistro);
        if (!sent)
        {
            Log.Warning("No se pudo enviar el correo de prerregistro a {Email}", preregistro.Email);
        }

        return ApiResponse<PreRegisterResponseDto>.Ok(new PreRegisterResponseDto
        {
            Email = preregistro.Email,
            Nombre = preregistro.Nombre,
            Mensaje = "¡Prerregistro completado con éxito! Hemos enviado un correo con todos tus datos y el enlace para validar o cancelar tu registro."
        }, "Prerregistro exitoso");
    }

    public async Task<ApiResponse<AuthResponseDto>> ConfirmPreRegisterAsync(ConfirmPreRegisterDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Token) || string.IsNullOrWhiteSpace(request.Email))
            return ApiResponse<AuthResponseDto>.Fail("Token y correo electrónico son requeridos");

        var emailLower = request.Email.Trim().ToLower();

        var preregistro = await _context.PreRegistros
            .FirstOrDefaultAsync(p => p.Email.ToLower() == emailLower 
                                   && p.TokenValidacion == request.Token.Trim() 
                                   && p.Estado == "Pendiente");

        if (preregistro == null)
        {
            return ApiResponse<AuthResponseDto>.Fail("El enlace de validación es inválido, ya fue utilizado o ha sido cancelado.");
        }

        if (DateTime.UtcNow > preregistro.FechaExpiracion)
        {
            preregistro.Estado = "Expirado";
            await _context.SaveChangesAsync();
            return ApiResponse<AuthResponseDto>.Fail("El enlace de validación ha expirado. Por favor realiza un nuevo registro.");
        }

        // Verificar que no se haya registrado mientras tanto
        if (await _context.Usuarios.AnyAsync(u => u.Email.ToLower() == emailLower))
        {
            preregistro.Estado = "Validado";
            await _context.SaveChangesAsync();
            return ApiResponse<AuthResponseDto>.Fail("El correo ya se encuentra registrado y activo como usuario.");
        }

        // Crear el usuario definitivo convirtiendo la contraseña en MD5 como se solicitó
        var usuario = new Usuario
        {
            PrimerNombre = preregistro.PrimerNombre,
            SegundoNombre = preregistro.SegundoNombre,
            PrimerApellido = preregistro.PrimerApellido,
            SegundoApellido = preregistro.SegundoApellido,
            Nombre = preregistro.Nombre,
            Cedula = preregistro.Cedula,
            Email = preregistro.Email,
            PasswordHash = PasswordHelper.HashMD5(preregistro.PasswordPlana),
            Telefono = preregistro.Telefono,
            Direccion = preregistro.Direccion,
            RedesSociales = preregistro.RedesSociales,
            Rol = "Cliente",
            LimiteCredito = preregistro.LimiteCredito,
            DeudaActual = 0m,
            Activo = true,
            FechaCreacion = DateTime.UtcNow
        };

        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();

        if (!string.IsNullOrWhiteSpace(preregistro.FotoBase64))
        {
            _context.UsuarioFotos.Add(new UsuarioFoto
            {
                UsuarioId = usuario.Id,
                FotoBase64 = preregistro.FotoBase64,
                FechaActualizacion = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
        }

        // Marcar prerregistro como Validado
        preregistro.Estado = "Validado";
        await _context.SaveChangesAsync();

        // Generar tokens de sesión
        var token = JwtHelper.GenerateToken(usuario, _configuration);
        var refreshToken = JwtHelper.GenerateRefreshToken();
        var expiration = DateTime.UtcNow.AddMinutes(
            double.Parse(_configuration["JwtSettings:ExpirationInMinutes"] ?? "1440"));

        usuario.RefreshToken = refreshToken;
        usuario.RefreshTokenExpiry = DateTime.UtcNow.AddDays(
            double.Parse(_configuration["JwtSettings:RefreshTokenExpirationInDays"] ?? "7"));
        await _context.SaveChangesAsync();

        return ApiResponse<AuthResponseDto>.Ok(new AuthResponseDto
        {
            Id = usuario.Id,
            Token = token,
            RefreshToken = refreshToken,
            Expiracion = expiration,
            Email = usuario.Email,
            Nombre = usuario.Nombre,
            PrimerNombre = usuario.PrimerNombre,
            SegundoNombre = usuario.SegundoNombre,
            PrimerApellido = usuario.PrimerApellido,
            SegundoApellido = usuario.SegundoApellido,
            Cedula = usuario.Cedula,
            Rol = usuario.Rol,
            Telefono = usuario.Telefono,
            Direccion = usuario.Direccion,
            RedesSociales = usuario.RedesSociales,
            LimiteCredito = usuario.LimiteCredito,
            DeudaActual = usuario.DeudaActual,
            FotoBase64 = preregistro.FotoBase64
        }, "¡Tu registro ha sido confirmado exitosamente! Bienvenido a Claudipan.");
    }

    public async Task<ApiResponse<bool>> CancelPreRegisterAsync(CancelPreRegisterDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Token) || string.IsNullOrWhiteSpace(request.Email))
            return ApiResponse<bool>.Fail("Token y correo electrónico son requeridos");

        var emailLower = request.Email.Trim().ToLower();

        var preregistro = await _context.PreRegistros
            .FirstOrDefaultAsync(p => p.Email.ToLower() == emailLower 
                                   && p.TokenCancelacion == request.Token.Trim() 
                                   && p.Estado == "Pendiente");

        if (preregistro == null)
        {
            return ApiResponse<bool>.Fail("La solicitud no existe, ya fue cancelada o ya fue validada previamente.");
        }

        preregistro.Estado = "Cancelado";
        await _context.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, "Tu registro ha sido cancelado con éxito. Tus datos no fueron almacenados como usuario activo.");
    }
}