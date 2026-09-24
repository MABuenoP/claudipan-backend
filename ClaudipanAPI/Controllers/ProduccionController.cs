using System.Security.Claims;
using ClaudipanAPI.Interfaces;
using ClaudipanAPI.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClaudipanAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProduccionController : ControllerBase
{
    private readonly IProduccionService _produccionService;

    public ProduccionController(IProduccionService produccionService)
    {
        _produccionService = produccionService;
    }

    // --- RECETAS DE PRODUCCIÓN (FÓRMULAS DE PANADERÍA) ---

    [HttpGet("recetas")]
    [Authorize(Roles = "Administrador,Gerente,Panadero,Contable")]
    public async Task<IActionResult> GetAllRecetas()
    {
        var result = await _produccionService.GetAllRecetasAsync();
        return Ok(result);
    }

    [HttpGet("recetas/{id}")]
    [Authorize(Roles = "Administrador,Gerente,Panadero,Contable")]
    public async Task<IActionResult> GetRecetaById(int id)
    {
        var result = await _produccionService.GetRecetaByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost("recetas")]
    [Authorize(Roles = "Administrador,Gerente")]
    public async Task<IActionResult> CreateReceta([FromBody] RecetaCreateDto dto)
    {
        var result = await _produccionService.CreateRecetaAsync(dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("recetas/{id}"), HttpPut("recetas/{id}")]
    [Authorize(Roles = "Administrador,Gerente")]
    public async Task<IActionResult> UpdateReceta(int id, [FromBody] RecetaUpdateDto dto)
    {
        var result = await _produccionService.UpdateRecetaAsync(id, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("recetas/{id}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> DeleteReceta(int id)
    {
        var result = await _produccionService.DeleteRecetaAsync(id);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // --- PRE-CHEQUEO DE INSUMOS (SOLO GERENTE Y ADMINISTRADOR) ---

    [HttpPost("ordenes/pre-chequeo")]
    [Authorize(Roles = "Administrador,Gerente")]
    public async Task<IActionResult> PreChequeoInsumos([FromBody] PreChequeoInsumosRequestDto dto)
    {
        var result = await _produccionService.PreChequeoInsumosAsync(dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // --- ÓRDENES DE PRODUCCIÓN ---

    [HttpGet("ordenes")]
    [Authorize(Roles = "Administrador,Gerente,Panadero,Contable")]
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
    [Authorize(Roles = "Administrador,Gerente,Panadero,Contable")]
    public async Task<IActionResult> GetOrdenById(int id)
    {
        var result = await _produccionService.GetOrdenByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    // SOLO GERENTE Y ADMINISTRADOR PUEDEN CREAR ÓRDENES
    [HttpPost("ordenes")]
    [Authorize(Roles = "Administrador,Gerente")]
    public async Task<IActionResult> CreateOrden([FromBody] OrdenProduccionCreateDto dto)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var creadorId))
            return Unauthorized();

        var result = await _produccionService.CreateOrdenAsync(creadorId, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // --- PASO 1: CARGAR INSUMOS (SOLO PANADERO Y ADMIN) ---
    [HttpPatch("ordenes/{id}/cargar-insumos")]
    [Authorize(Roles = "Panadero,Administrador")]
    public async Task<IActionResult> CargarInsumos(int id, [FromBody] CargarInsumosDto? dto)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        int.TryParse(userIdClaim, out var panaderoId);

        var result = await _produccionService.CargarInsumosAsync(id, panaderoId, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // --- PASO 2: PASAR A HORNEANDO (SOLO PANADERO Y ADMIN) ---
    [HttpPatch("ordenes/{id}/pasar-horneando")]
    [Authorize(Roles = "Panadero,Administrador")]
    public async Task<IActionResult> PasarHorneando(int id, [FromBody] PasarHorneandoDto? dto)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        int.TryParse(userIdClaim, out var panaderoId);

        var result = await _produccionService.PasarHorneandoAsync(id, panaderoId, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // --- PASO 3: FINALIZAR Y CUANTIFICAR (SOLO PANADERO Y ADMIN) ---
    [HttpPost("ordenes/{id}/finalizar-cuantificar")]
    [Authorize(Roles = "Panadero,Administrador")]
    public async Task<IActionResult> FinalizarYCuantificar(int id, [FromBody] CuantificarProduccionDto dto)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        int.TryParse(userIdClaim, out var panaderoId);

        var result = await _produccionService.FinalizarYCuantificarAsync(id, panaderoId, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // --- COMPATIBILIDAD LEGACY ---
    [HttpPatch("ordenes/{id}/iniciar")]
    [Authorize(Roles = "Panadero,Administrador")]
    public async Task<IActionResult> IniciarOrden(int id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        int.TryParse(userIdClaim, out var panaderoId);
        var result = await _produccionService.CargarInsumosAsync(id, panaderoId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("ordenes/{id}/entregar")]
    [Authorize(Roles = "Panadero,Administrador")]
    public async Task<IActionResult> EntregarProduccion(int id, [FromBody] EntregarProduccionDto dto)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        int.TryParse(userIdClaim, out var panaderoId);
        var cuantificarDto = new CuantificarProduccionDto
        {
            CantOptima = dto.CantidadProducida,
            CantBuenasCondiciones = 0,
            CantMalasCondiciones = 0,
            DestinoMalasCondiciones = "Ninguno",
            Observaciones = dto.Observaciones
        };
        var result = await _produccionService.FinalizarYCuantificarAsync(id, panaderoId, cuantificarDto);
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

