using System.Security.Claims;
using ClaudipanAPI.Interfaces;
using ClaudipanAPI.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClaudipanAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrador,Gerente,Contable,Panadero")]
public class GastosController : ControllerBase
{
    private readonly IGastoService _gastoService;

    public GastosController(IGastoService gastoService)
    {
        _gastoService = gastoService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? tipoGasto,
        [FromQuery] string? categoriaGasto,
        [FromQuery] DateTime? fechaInicio,
        [FromQuery] DateTime? fechaFin)
    {
        var result = await _gastoService.GetAllAsync(tipoGasto, categoriaGasto, fechaInicio, fechaFin);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _gastoService.GetByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] GastoCreateDto dto)
    {
        int? usuarioId = null;
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(userIdClaim, out var id))
            usuarioId = id;

        var result = await _gastoService.CreateAsync(usuarioId, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Administrador,Gerente,Contable")]
    public async Task<IActionResult> Update(int id, [FromBody] GastoCreateDto dto)
    {
        var result = await _gastoService.UpdateAsync(id, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Administrador,Gerente")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _gastoService.DeleteAsync(id);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
