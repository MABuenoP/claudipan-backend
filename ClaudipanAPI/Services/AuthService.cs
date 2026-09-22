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

    public async Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginRequestDto request)
    {
        var usuario = await _context.Usuarios
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
            Cedula = usuario.Cedula,
            Rol = usuario.Rol,
            Telefono = usuario.Telefono,
            Direccion = usuario.Direccion,
            RedesSociales = usuario.RedesSociales,
            LimiteCredito = usuario.LimiteCredito,
            DeudaActual = usuario.DeudaActual
        }, "Login exitoso");
    }

    public async Task<ApiResponse<AuthResponseDto>> RegisterAsync(RegisterRequestDto request)
    {
        if (await _context.Usuarios.AnyAsync(u => u.Email.ToLower() == request.Email.ToLower()))
            return ApiResponse<AuthResponseDto>.Fail("El correo ya está registrado");

        var usuario = new Usuario
        {
            Nombre = request.Nombre,
            Cedula = request.Cedula,
            Email = request.Email,
            PasswordHash = PasswordHelper.HashPassword(request.Password),
            Telefono = request.Telefono,
            Direccion = request.Direccion,
            RedesSociales = request.RedesSociales,
            Rol = "Cliente",
            LimiteCredito = request.LimiteCredito ?? 500000m, // Cupo inicial por defecto para fiarle
            DeudaActual = 0m,
            Activo = true,
            FechaCreacion = DateTime.UtcNow
        };

        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();

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
            Cedula = usuario.Cedula,
            Rol = usuario.Rol,
            Telefono = usuario.Telefono,
            Direccion = usuario.Direccion,
            RedesSociales = usuario.RedesSociales,
            LimiteCredito = usuario.LimiteCredito,
            DeudaActual = usuario.DeudaActual
        }, "Cliente registrado exitosamente");
    }

    public async Task<ApiResponse<AuthResponseDto>> RefreshTokenAsync(RefreshTokenRequestDto request)
    {
        var usuario = await _context.Usuarios
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
            Cedula = usuario.Cedula,
            Rol = usuario.Rol,
            Telefono = usuario.Telefono,
            Direccion = usuario.Direccion,
            RedesSociales = usuario.RedesSociales,
            LimiteCredito = usuario.LimiteCredito,
            DeudaActual = usuario.DeudaActual
        }, "Token renovado exitosamente");
    }

    public async Task<ApiResponse<UserProfileDto>> GetProfileAsync(int userId)
    {
        var u = await _context.Usuarios.FindAsync(userId);
        if (u == null) return ApiResponse<UserProfileDto>.Fail("Usuario no encontrado");

        return ApiResponse<UserProfileDto>.Ok(new UserProfileDto
        {
            Id = u.Id,
            Nombre = u.Nombre,
            Cedula = u.Cedula,
            Email = u.Email,
            Rol = u.Rol,
            Telefono = u.Telefono,
            Direccion = u.Direccion,
            RedesSociales = u.RedesSociales,
            LimiteCredito = u.LimiteCredito,
            DeudaActual = u.DeudaActual
        });
    }

    public async Task<ApiResponse<UserProfileDto>> UpdateProfileAsync(int userId, UpdateProfileDto dto)
    {
        var u = await _context.Usuarios.FindAsync(userId);
        if (u == null) return ApiResponse<UserProfileDto>.Fail("Usuario no encontrado");

        u.Nombre = dto.Nombre;
        u.Cedula = dto.Cedula ?? u.Cedula;
        u.Telefono = dto.Telefono;
        u.Direccion = dto.Direccion;
        u.RedesSociales = dto.RedesSociales ?? u.RedesSociales;

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
            Cedula = u.Cedula,
            Email = u.Email,
            Rol = u.Rol,
            Telefono = u.Telefono,
            Direccion = u.Direccion,
            RedesSociales = u.RedesSociales,
            LimiteCredito = u.LimiteCredito,
            DeudaActual = u.DeudaActual
        }, "Perfil actualizado con éxito");
    }

    public async Task<ApiResponse<List<UsuarioAdminDto>>> GetAllUsersAsync()
    {
        var users = await _context.Usuarios
            .OrderBy(u => u.Nombre)
            .Select(u => new UsuarioAdminDto
            {
                Id = u.Id,
                Nombre = u.Nombre,
                Cedula = u.Cedula,
                Email = u.Email,
                Rol = u.Rol,
                Telefono = u.Telefono,
                Direccion = u.Direccion,
                RedesSociales = u.RedesSociales,
                LimiteCredito = u.LimiteCredito,
                DeudaActual = u.DeudaActual,
                Activo = u.Activo,
                FechaCreacion = u.FechaCreacion
            })
            .ToListAsync();

        return ApiResponse<List<UsuarioAdminDto>>.Ok(users);
    }

    public async Task<ApiResponse<UsuarioAdminDto>> CreateUserAdminAsync(UpdateUsuarioAdminDto dto)
    {
        if (await _context.Usuarios.AnyAsync(u => u.Email.ToLower() == dto.Email.ToLower()))
            return ApiResponse<UsuarioAdminDto>.Fail("El correo ya se encuentra registrado");

        var password = string.IsNullOrWhiteSpace(dto.Password) ? "Claudipan123*" : dto.Password;
        var usuario = new Usuario
        {
            Nombre = dto.Nombre,
            Cedula = dto.Cedula,
            Email = dto.Email,
            Rol = dto.Rol,
            Telefono = dto.Telefono,
            Direccion = dto.Direccion,
            RedesSociales = dto.RedesSociales,
            LimiteCredito = dto.LimiteCredito,
            Activo = dto.Activo,
            PasswordHash = PasswordHelper.HashPassword(password),
            FechaCreacion = DateTime.UtcNow
        };

        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();

        return ApiResponse<UsuarioAdminDto>.Ok(new UsuarioAdminDto
        {
            Id = usuario.Id,
            Nombre = usuario.Nombre,
            Cedula = usuario.Cedula,
            Email = usuario.Email,
            Rol = usuario.Rol,
            Telefono = usuario.Telefono,
            Direccion = usuario.Direccion,
            RedesSociales = usuario.RedesSociales,
            LimiteCredito = usuario.LimiteCredito,
            DeudaActual = usuario.DeudaActual,
            Activo = usuario.Activo,
            FechaCreacion = usuario.FechaCreacion
        }, "Usuario creado correctamente");
    }

    public async Task<ApiResponse<UsuarioAdminDto>> UpdateUserAdminAsync(int id, UpdateUsuarioAdminDto dto)
    {
        var u = await _context.Usuarios.FindAsync(id);
        if (u == null) return ApiResponse<UsuarioAdminDto>.Fail("Usuario no encontrado");

        u.Nombre = dto.Nombre;
        u.Cedula = dto.Cedula ?? u.Cedula;
        u.Email = dto.Email;
        u.Rol = dto.Rol;
        u.Telefono = dto.Telefono;
        u.Direccion = dto.Direccion;
        u.RedesSociales = dto.RedesSociales ?? u.RedesSociales;
        u.LimiteCredito = dto.LimiteCredito;
        u.Activo = dto.Activo;

        if (!string.IsNullOrWhiteSpace(dto.Password))
        {
            u.PasswordHash = PasswordHelper.HashPassword(dto.Password);
        }

        await _context.SaveChangesAsync();

        return ApiResponse<UsuarioAdminDto>.Ok(new UsuarioAdminDto
        {
            Id = u.Id,
            Nombre = u.Nombre,
            Cedula = u.Cedula,
            Email = u.Email,
            Rol = u.Rol,
            Telefono = u.Telefono,
            Direccion = u.Direccion,
            RedesSociales = u.RedesSociales,
            LimiteCredito = u.LimiteCredito,
            DeudaActual = u.DeudaActual,
            Activo = u.Activo,
            FechaCreacion = u.FechaCreacion
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