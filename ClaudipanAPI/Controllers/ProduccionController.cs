using System.Security.Claims;
using ClaudipanAPI.Interfaces;
using ClaudipanAPI.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClaudipanAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrador,Gerente,Panadero")]
public class ProduccionController : ControllerBase
{
    private readonly IProduccionService _produccionService;

    public ProduccionController(IProduccionService produccionService)
    {
        _produccionService = produccionService;
    }

    // --- RECETAS DE PRODUCCIÓN (FÓRMULAS DE PANADERÍA) ---

    [HttpGet("recetas")]
    public async Task<IActionResult> GetAllRecetas()
    {
        var result = await _produccionService.GetAllRecetasAsync();
        return Ok(result);
    }

    [HttpGet("recetas/{id}")]
    public async Task<IActionResult> GetRecetaById(int id)
    {
        var result = await _produccionService.GetRecetaByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost("recetas")]
    public async Task<IActionResult> CreateReceta([FromBody] RecetaCreateDto dto)
    {
        var result = await _produccionService.CreateRecetaAsync(dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("recetas/{id}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> DeleteReceta(int id)
    {
        var result = await _produccionService.DeleteRecetaAsync(id);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // --- ÓRDENES DE PRODUCCIÓN Y ENTREGA DE PAN ---

    [HttpGet("ordenes")]
    public async Task<IActionResult> GetAllOrdenes([FromQuery] int? panaderoId, [FromQuery] string? estado)
    {
        // Si el usuario es Panadero y no es Admin/Gerente, solo ve sus propias órdenes
        var rol = User.FindFirst(ClaimTypes.Role)?.Value;
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (rol == "Panadero" && int.TryParse(userIdClaim, out var panaderoActualId))
        {
            panaderoId = panaderoActualId;
        }

        var result = await _produccionService.GetAllOrdenesAsync(panaderoId, estado);
        return Ok(result);
    }

    [HttpGet("ordenes/{id}")]
    public async Task<IActionResult> GetOrdenById(int id)
    {
        var result = await _produccionService.GetOrdenByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost("ordenes")]
    public async Task<IActionResult> CreateOrden([FromBody] OrdenProduccionCreateDto dto)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var panaderoId))
            return Unauthorized();

        var result = await _produccionService.CreateOrdenAsync(panaderoId, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPatch("ordenes/{id}/iniciar")]
    public async Task<IActionResult> IniciarOrden(int id)
    {
        var result = await _produccionService.IniciarOrdenAsync(id);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("ordenes/{id}/entregar")]
    public async Task<IActionResult> EntregarProduccion(int id, [FromBody] EntregarProduccionDto dto)
    {
        var result = await _produccionService.EntregarProduccionAsync(id, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPatch("ordenes/{id}/cancelar")]
    [Authorize(Roles = "Administrador,Gerente")]
    public async Task<IActionResult> CancelarOrden(int id)
    {
        var result = await _produccionService.CancelarOrdenAsync(id);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
