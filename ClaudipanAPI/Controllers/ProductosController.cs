using ClaudipanAPI.Interfaces;
using ClaudipanAPI.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClaudipanAPI.Controllers;

/// <summary>
/// Controlador de productos - CRUD completo y filtros de panadería y bebidas.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ProductosController : ControllerBase
{
    private readonly IProductoService _productoService;

    public ProductosController(IProductoService productoService) 
        => _productoService = productoService;

    /// <summary>
    /// Obtiene productos con filtros de categoría, precio, ofertas, tamaño, marca y sabor (público).
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll(
        [FromQuery] int? categoriaId,
        [FromQuery] string? search,
        [FromQuery] decimal? minPrecio,
        [FromQuery] decimal? maxPrecio,
        [FromQuery] bool? soloOfertas,
        [FromQuery] string? marca,
        [FromQuery] string? sabor,
        [FromQuery] string? tamano,
        [FromQuery] string? presentacion)
    {
        var result = await _productoService.GetAllAsync(
            categoriaId, search, minPrecio, maxPrecio, soloOfertas, marca, sabor, tamano, presentacion);
        return Ok(result);
    }

    /// <summary>
    /// Obtiene un producto por ID (público).
    /// </summary>
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _productoService.GetByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Crea un nuevo producto (Administrador, Gerente, Panadero).
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Administrador,Gerente,Panadero")]
    public async Task<IActionResult> Create([FromBody] ProductoCreateDto dto)
    {
        var result = await _productoService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }

    /// <summary>
    /// Actualiza un producto existente (Administrador, Gerente, Panadero).
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Administrador,Gerente,Panadero")]
    public async Task<IActionResult> Update(int id, [FromBody] ProductoUpdateDto dto)
    {
        var result = await _productoService.UpdateAsync(id, dto);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Elimina un producto (Administrador, Gerente).
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Administrador,Gerente")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _productoService.DeleteAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Actualiza el stock de un producto (Administrador, Panadero, Vendedor).
    /// </summary>
    [HttpPatch("{id}/stock")]
    [Authorize(Roles = "Administrador,Gerente,Panadero,Vendedor")]
    public async Task<IActionResult> UpdateStock(int id, [FromQuery] int cantidad)
    {
        var result = await _productoService.UpdateStockAsync(id, cantidad);
        return result.Success ? Ok(result) : NotFound(result);
    }
}