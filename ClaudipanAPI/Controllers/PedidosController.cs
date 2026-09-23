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

    private int? GetCurrentUserId()
    {
        var val = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("nameid")?.Value
            ?? User.FindFirst("sub")?.Value
            ?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

        return int.TryParse(val, out var id) ? id : null;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAll([FromQuery] string? estado, [FromQuery] string? tipoPago)
    {
        var role = User.FindFirst(ClaimTypes.Role)?.Value;
        var userId = GetCurrentUserId();
        
        int? filterUserId = null;
        if (role == "Cliente" && userId.HasValue)
        {
            filterUserId = userId.Value;
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
        var userId = GetCurrentUserId();
        if (userId.HasValue)
        {
            dto.UsuarioId = userId.Value;
            dto.EsInvitado = false;
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

    [HttpPost("{id}/entregar")]
    [Authorize(Roles = "Administrador,Gerente,Vendedor")]
    public async Task<IActionResult> Entregar(int id, [FromBody] EntregarPedidoDto dto)
    {
        var result = await _pedidoService.EntregarAsync(id, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{id}/editar")]
    [Authorize(Roles = "Administrador,Gerente,Vendedor")]
    public async Task<IActionResult> UpdatePedido(int id, [FromBody] PedidoCreateDto dto)
    {
        var result = await _pedidoService.UpdatePedidoAsync(id, dto);
        return result.Success ? Ok(result) : BadRequest(result);
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
        var userId = GetCurrentUserId();

        if (role == "Cliente" && userId.HasValue)
        {
            usuarioId = userId.Value;
        }

        var result = await _pedidoService.GetTransaccionesDeudaAsync(usuarioId);
        return Ok(result);
    }

    [HttpPost("abonar")]
    [Authorize(Roles = "Administrador,Gerente,Contable,Vendedor,Cliente")]
    public async Task<IActionResult> RegistrarAbono([FromBody] RegistrarAbonoDto dto)
    {
        var role = User.FindFirst(ClaimTypes.Role)?.Value;
        var userId = GetCurrentUserId();

        if (role == "Cliente" && userId.HasValue)
        {
            dto.UsuarioId = userId.Value;
        }

        var result = await _pedidoService.RegistrarAbonoClienteAsync(dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}