using ClaudipanAPI.Interfaces;
using ClaudipanAPI.Models.DTOs;
using ClaudipanAPI.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClaudipanAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrador")]
public class AuditoriaController : ControllerBase
{
    private readonly IAuditoriaService _auditoriaService;

    public AuditoriaController(IAuditoriaService auditoriaService)
    {
        _auditoriaService = auditoriaService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? tabla, 
        [FromQuery] string? accion,
        [FromQuery] string? formulario,
        [FromQuery] string? busqueda)
    {
        var result = await _auditoriaService.GetAllAsync(tabla, accion, formulario, busqueda);
        return Ok(ApiResponse<IEnumerable<AuditoriaDto>>.Ok(result));
    }
}
