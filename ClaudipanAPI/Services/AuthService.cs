using ClaudipanAPI.Data;
using ClaudipanAPI.Helpers;
using ClaudipanAPI.Interfaces;
using ClaudipanAPI.Models.DTOs;
using ClaudipanAPI.Models.Entities;
using ClaudipanAPI.Models.Responses;
using Microsoft.EntityFrameworkCore;

namespace ClaudipanAPI.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
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
        if (await _context.Usuarios.AnyAsync(u => u.Email.ToLower() == request.Email.ToLower()))
            return ApiResponse<AuthResponseDto>.Fail("El correo ya está registrado");

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
            LimiteCredito = request.LimiteCredito ?? 500000m,
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
}