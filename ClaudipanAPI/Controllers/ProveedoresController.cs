using ClaudipanAPI.Interfaces;
using ClaudipanAPI.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClaudipanAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProveedoresController : ControllerBase
{
    private readonly IProveedorService _proveedorService;

    public ProveedoresController(IProveedorService proveedorService)
    {
        _proveedorService = proveedorService;
    }

    [HttpGet]
    [Authorize(Roles = "Administrador,Gerente,Contable,Panadero")]
    public async Task<IActionResult> GetAll([FromQuery] bool? soloActivos = true)
    {
        var result = await _proveedorService.GetAllAsync(soloActivos);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Administrador,Gerente,Contable,Panadero")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _proveedorService.GetByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost]
    [Authorize(Roles = "Administrador,Gerente,Contable")]
    public async Task<IActionResult> Create([FromBody] ProveedorCreateDto dto)
    {
        var result = await _proveedorService.CreateAsync(dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Administrador,Gerente,Contable")]
    public async Task<IActionResult> Update(int id, [FromBody] ProveedorUpdateDto dto)
    {
        var result = await _proveedorService.UpdateAsync(id, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Administrador,Gerente")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _proveedorService.DeleteAsync(id);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
