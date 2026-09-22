using System.Security.Claims;
using ClaudipanAPI.Interfaces;
using ClaudipanAPI.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClaudipanAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrador,Gerente,Contable,Panadero,Vendedor")]
public class BajasController : ControllerBase
{
    private readonly IBajaService _bajaService;

    public BajasController(IBajaService bajaService)
    {
        _bajaService = bajaService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? productoId, [FromQuery] string? motivo)
    {
        var result = await _bajaService.GetAllAsync(productoId, motivo);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _bajaService.GetByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] BajaProductoCreateDto dto)
    {
        int? usuarioId = null;
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(userIdClaim, out var id))
            usuarioId = id;

        var result = await _bajaService.CreateAsync(usuarioId, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Administrador,Gerente")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _bajaService.DeleteAsync(id);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
