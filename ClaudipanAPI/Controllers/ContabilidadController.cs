using System.Security.Claims;
using ClaudipanAPI.Interfaces;
using ClaudipanAPI.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClaudipanAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ContabilidadController : ControllerBase
{
    private readonly IContabilidadService _contabilidadService;

    public ContabilidadController(IContabilidadService contabilidadService)
    {
        _contabilidadService = contabilidadService;
    }

    private int? GetCurrentUserId()
    {
        var val = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("nameid")?.Value
            ?? User.FindFirst("sub")?.Value
            ?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

        return int.TryParse(val, out var id) ? id : null;
    }

    /// <summary>
    /// Resumen contable general: ventas, compras, cartera, gastos, utilidad bruta y neta.
    /// </summary>
    [HttpGet("resumen")]
    [Authorize(Roles = "Administrador,Gerente,Contable")]
    public async Task<IActionResult> GetResumenContable()
    {
        var result = await _contabilidadService.GetResumenContableAsync();
        return Ok(result);
    }

    /// <summary>
    /// Estado de Resultados (P&G - Pérdidas y Ganancias) con detalle de servicios, nómina y bajas.
    /// </summary>
    [HttpGet("estado-resultados")]
    [Authorize(Roles = "Administrador,Gerente,Contable")]
    public async Task<IActionResult> GetEstadoResultados([FromQuery] DateTime? fechaInicio, [FromQuery] DateTime? fechaFin)
    {
        var result = await _contabilidadService.GetEstadoResultadosAsync(fechaInicio, fechaFin);
        return Ok(result);
    }

    /// <summary>
    /// Cuadro de productos más vendidos (top ventas y rotación).
    /// </summary>
    [HttpGet("mas-vendidos")]
    [Authorize(Roles = "Administrador,Gerente,Contable,Vendedor")]
    public async Task<IActionResult> GetProductosMasVendidos([FromQuery] int top = 10)
    {
        var result = await _contabilidadService.GetProductosMasVendidosAsync(top);
        return Ok(result);
    }

    /// <summary>
    /// Cuadro de productos que presentan rezago o pérdidas (mermas, bajas y baja rotación).
    /// </summary>
    [HttpGet("rezagos-perdidas")]
    [Authorize(Roles = "Administrador,Gerente,Contable,Panadero")]
    public async Task<IActionResult> GetProductosConRezagoOPerdidas()
    {
        var result = await _contabilidadService.GetProductosConRezagoOPerdidasAsync();
        return Ok(result);
    }

    /// <summary>
    /// Historial de movimientos de créditos y fiados de clientes.
    /// </summary>
    [HttpGet("creditos")]
    [Authorize(Roles = "Administrador,Gerente,Contable,Vendedor")]
    public async Task<IActionResult> GetHistorialCreditos([FromQuery] int? usuarioId)
    {
        var result = await _contabilidadService.GetHistorialCreditosAsync(usuarioId);
        return Ok(result);
    }

    /// <summary>
    /// Obtiene el resumen de crédito, saldo, compras (contado/fiado) y transacciones del cliente autenticado.
    /// </summary>
    [HttpGet("mis-deudas")]
    public async Task<IActionResult> GetMisDeudas()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        var result = await _contabilidadService.GetMisDeudasResumenAsync(userId.Value);
        return Ok(result);
    }
}
