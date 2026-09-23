using ClaudipanAPI.Interfaces;
using ClaudipanAPI.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClaudipanAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InsumosController : ControllerBase
{
    private readonly IInsumoService _insumoService;

    public InsumosController(IInsumoService insumoService)
    {
        _insumoService = insumoService;
    }

    [HttpGet]
    [Authorize(Roles = "Administrador,Gerente,Contable,Panadero")]
    public async Task<IActionResult> GetAll([FromQuery] bool? soloActivos = true)
    {
        var result = await _insumoService.GetAllAsync(soloActivos);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Administrador,Gerente,Contable,Panadero")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _insumoService.GetByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost]
    [Authorize(Roles = "Administrador,Gerente,Panadero,Contable")]
    public async Task<IActionResult> Create([FromBody] InsumoCreateDto dto)
    {
        var result = await _insumoService.CreateAsync(dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{id}")]
    [Authorize(Roles = "Administrador,Gerente,Panadero,Contable")]
    public async Task<IActionResult> Update(int id, [FromBody] InsumoUpdateDto dto)
    {
        var result = await _insumoService.UpdateAsync(id, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Administrador,Gerente")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _insumoService.DeleteAsync(id);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPatch("{id}/stock")]
    [Authorize(Roles = "Administrador,Gerente,Panadero")]
    public async Task<IActionResult> AjustarStock(int id, [FromQuery] decimal delta, [FromQuery] decimal? nuevoCosto)
    {
        var result = await _insumoService.AjustarStockAsync(id, delta, nuevoCosto);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
