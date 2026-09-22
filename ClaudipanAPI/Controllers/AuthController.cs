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

    public AuthController(IAuthService authService) => _authService = authService;

    /// <summary>
    /// Inicia sesión y devuelve un token JWT.
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        var result = await _authService.LoginAsync(request);
        return result.Success ? Ok(result) : Unauthorized(result);
    }

    /// <summary>
    /// Registra un nuevo usuario cliente.
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        var result = await _authService.RegisterAsync(request);
        return result.Success ? Ok(result) : BadRequest(result);
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
    /// Obtiene la información del perfil del usuario autenticado.
    /// </summary>
    [HttpGet("profile")]
    [Authorize]
    public async Task<IActionResult> GetProfile()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var result = await _authService.GetProfileAsync(userId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Actualiza el perfil del usuario autenticado.
    /// </summary>
    [HttpPut("profile")]
    [Authorize]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var result = await _authService.UpdateProfileAsync(userId, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Obtiene la lista completa de usuarios (Administrador y Técnico).
    /// </summary>
    [HttpGet("users")]
    [Authorize(Roles = "Administrador,Tecnico")]
    public async Task<IActionResult> GetAllUsers()
    {
        var result = await _authService.GetAllUsersAsync();
        return Ok(result);
    }

    /// <summary>
    /// Crea un usuario nuevo con rol específico (Administrador y Técnico).
    /// </summary>
    [HttpPost("users")]
    [Authorize(Roles = "Administrador,Tecnico")]
    public async Task<IActionResult> CreateUserAdmin([FromBody] UpdateUsuarioAdminDto dto)
    {
        var result = await _authService.CreateUserAdminAsync(dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Actualiza rol, datos o crédito de un usuario (Administrador y Técnico).
    /// </summary>
    [HttpPut("users/{id}")]
    [Authorize(Roles = "Administrador,Tecnico")]
    public async Task<IActionResult> UpdateUserAdmin(int id, [FromBody] UpdateUsuarioAdminDto dto)
    {
        var result = await _authService.UpdateUserAdminAsync(id, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Desactiva a un usuario (Administrador).
    /// </summary>
    [HttpDelete("users/{id}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> DeleteUserAdmin(int id)
    {
        var result = await _authService.DeleteUserAdminAsync(id);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}