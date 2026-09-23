using System.Security.Claims;
using ClaudipanAPI.Interfaces;
using ClaudipanAPI.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClaudipanAPI.Controllers;

/// <summary>
/// Controlador de autenticación y gestión de usuarios.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IAuditoriaService _auditoriaService;

    public AuthController(IAuthService authService, IAuditoriaService auditoriaService)
    {
        _authService = authService;
        _auditoriaService = auditoriaService;
    }

    private string GetClientIp()
    {
        if (Request.Headers.TryGetValue("X-Forwarded-For", out var fwd))
        {
            var ip = fwd.FirstOrDefault()?.Split(',')[0].Trim();
            if (!string.IsNullOrEmpty(ip)) return ip;
        }
        if (Request.Headers.TryGetValue("CF-Connecting-IP", out var cf))
        {
            var ip = cf.FirstOrDefault()?.Trim();
            if (!string.IsNullOrEmpty(ip)) return ip;
        }
        return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
    }

    private int? GetCurrentUserId()
    {
        var val = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("nameid")?.Value
            ?? User.FindFirst("sub")?.Value
            ?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

        return int.TryParse(val, out var id) ? id : null;
    }

    /// <summary>
    /// Inicia sesión y devuelve un token JWT.
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        var ip = GetClientIp();
        var result = await _authService.LoginAsync(request);
        if (result.Success && result.Data != null)
        {
            var u = result.Data;
            await _auditoriaService.RegistrarAccionAsync(
                usuarioId: u.Id,
                usuarioEmail: u.Email,
                accion: "Inicio de Sesión",
                tabla: "Usuarios",
                registroId: u.Id.ToString(),
                anterior: null,
                nuevo: $"Acceso concedido al sistema. Rol: {u.Rol}",
                ip: ip,
                formulario: "Formulario de Login",
                usuarioNombre: u.Nombre,
                usuarioRol: u.Rol
            );
            return Ok(result);
        }
        else
        {
            await _auditoriaService.RegistrarAccionAsync(
                usuarioId: null,
                usuarioEmail: request.Email ?? "Desconocido",
                accion: "Intento Fallido de Login",
                tabla: "Usuarios",
                registroId: null,
                anterior: null,
                nuevo: $"Credenciales incorrectas: {result.Message}",
                ip: ip,
                formulario: "Formulario de Login",
                usuarioNombre: null,
                usuarioRol: null
            );
            return Unauthorized(result);
        }
    }

    /// <summary>
    /// Registra un nuevo usuario cliente.
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        var ip = GetClientIp();
        var result = await _authService.RegisterAsync(request);
        if (result.Success && result.Data != null)
        {
            var u = result.Data;
            await _auditoriaService.RegistrarAccionAsync(
                usuarioId: u.Id,
                usuarioEmail: u.Email,
                accion: "Registro de Usuario",
                tabla: "Usuarios",
                registroId: u.Id.ToString(),
                anterior: null,
                nuevo: $"Usuario registrado: {u.Nombre} ({u.Email}) con cupo asignado",
                ip: ip,
                formulario: "Formulario de Registro",
                usuarioNombre: u.Nombre,
                usuarioRol: u.Rol
            );
            return Ok(result);
        }
        return BadRequest(result);
    }

    /// <summary>
    /// Renueva el token JWT usando un refresh token válido.
    /// </summary>
    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto request)
    {
        var result = await _authService.RefreshTokenAsync(request);
        return result.Success ? Ok(result) : Unauthorized(result);
    }

    /// <summary>
    /// Verifica si un campo (email, cédula, teléfono) ya existe en la base de datos.
    /// </summary>
    [HttpPost("check-field")]
    public async Task<IActionResult> CheckField([FromBody] CheckFieldDto request)
    {
        var result = await _authService.CheckFieldAsync(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Genera contraseña temporal de 8 dígitos y envía correo con enlace/token de activación.
    /// </summary>
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto request)
    {
        var result = await _authService.ForgotPasswordAsync(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Activa la nueva contraseña temporal si el token y correo coinciden.
    /// </summary>
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto request)
    {
        var result = await _authService.ResetPasswordAsync(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Obtiene la información del perfil del usuario autenticado.
    /// </summary>
    [HttpGet("profile")]
    [Authorize]
    public async Task<IActionResult> GetProfile()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        var result = await _authService.GetProfileAsync(userId.Value);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Actualiza el perfil del usuario autenticado (incluye cambio de foto y datos personales).
    /// </summary>
    [HttpPost("profile")]
    [Authorize]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        var result = await _authService.UpdateProfileAsync(userId.Value, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Cambia la contraseña del usuario autenticado de forma directa.
    /// </summary>
    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        var result = await _authService.ChangePasswordAsync(userId.Value, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Obtiene la lista completa de usuarios (Solo Administrador y Gerente).
    /// </summary>
    [HttpGet("users")]
    [Authorize(Roles = "Administrador,Gerente")]
    public async Task<IActionResult> GetAllUsers()
    {
        var result = await _authService.GetAllUsersAsync();
        return Ok(result);
    }

    /// <summary>
    /// Crea un usuario nuevo con rol específico (Solo Administrador y Gerente).
    /// </summary>
    [HttpPost("users")]
    [Authorize(Roles = "Administrador,Gerente")]
    public async Task<IActionResult> CreateUserAdmin([FromBody] UpdateUsuarioAdminDto dto)
    {
        var result = await _authService.CreateUserAdminAsync(dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Actualiza rol, datos, foto o tope de crédito de un usuario (Solo Administrador y Gerente).
    /// </summary>
    [HttpPost("users/{id}")]
    [Authorize(Roles = "Administrador,Gerente")]
    public async Task<IActionResult> UpdateUserAdmin(int id, [FromBody] UpdateUsuarioAdminDto dto)
    {
        var result = await _authService.UpdateUserAdminAsync(id, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Desactiva a un usuario (Solo Administrador y Gerente).
    /// </summary>
    [HttpDelete("users/{id}")]
    [Authorize(Roles = "Administrador,Gerente")]
    public async Task<IActionResult> DeleteUserAdmin(int id)
    {
        var result = await _authService.DeleteUserAdminAsync(id);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}