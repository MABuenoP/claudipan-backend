using System.Security.Claims;
using ClaudipanAPI.Interfaces;
using ClaudipanAPI.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClaudipanAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PedidosController : ControllerBase
{
    private readonly IPedidoService _pedidoService;

    public PedidosController(IPedidoService pedidoService) 
        => _pedidoService = pedidoService;

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAll([FromQuery] string? estado, [FromQuery] string? tipoPago)
    {
        var role = User.FindFirst(ClaimTypes.Role)?.Value;
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        int? filterUserId = null;
        if (role == "Cliente" && int.TryParse(userIdClaim, out var userId))
        {
            filterUserId = userId;
        }

        var result = await _pedidoService.GetAllAsync(filterUserId, estado, tipoPago);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _pedidoService.GetByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Create([FromBody] PedidoCreateDto dto)
    {
        if (User.Identity?.IsAuthenticated == true && !dto.EsInvitado)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out var userId))
            {
                dto.UsuarioId = userId;
            }
        }

        var result = await _pedidoService.CreateAsync(dto);
        return result.Success ? CreatedAtAction(nameof(GetById), new { id = result.Data?.Id ?? 0 }, result) : BadRequest(result);
    }

    [HttpPatch("{id}/estado")]
    [Authorize(Roles = "Administrador,Gerente,Vendedor,Panadero,Contable")]
    public async Task<IActionResult> UpdateEstado(int id, [FromBody] string nuevoEstado)
    {
        var result = await _pedidoService.UpdateEstadoAsync(id, nuevoEstado);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost("{id}/cancelar")]
    [Authorize(Roles = "Administrador,Gerente,Vendedor,Contable")]
    public async Task<IActionResult> Cancelar(int id)
    {
        var result = await _pedidoService.CancelarPedidoAsync(id);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("deudas")]
    [Authorize]
    public async Task<IActionResult> GetDeudas([FromQuery] int? usuarioId)
    {
        var role = User.FindFirst(ClaimTypes.Role)?.Value;
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (role == "Cliente" && int.TryParse(userIdClaim, out var clientId))
        {
            usuarioId = clientId;
        }

        var result = await _pedidoService.GetTransaccionesDeudaAsync(usuarioId);
        return Ok(result);
    }

    [HttpPost("abonar")]
    [Authorize(Roles = "Administrador,Gerente,Contable,Vendedor,Cliente")]
    public async Task<IActionResult> RegistrarAbono([FromBody] RegistrarAbonoDto dto)
    {
        var role = User.FindFirst(ClaimTypes.Role)?.Value;
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (role == "Cliente" && int.TryParse(userIdClaim, out var clientId))
        {
            dto.UsuarioId = clientId;
        }

        var result = await _pedidoService.RegistrarAbonoClienteAsync(dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}