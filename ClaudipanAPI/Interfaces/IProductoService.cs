using ClaudipanAPI.Models.DTOs;
using ClaudipanAPI.Models.Responses;

namespace ClaudipanAPI.Interfaces;

public interface IProductoService
{
    Task<ApiResponse<List<ProductoDto>>> GetAllAsync(
        int? categoriaId = null, 
        string? search = null, 
        decimal? minPrecio = null, 
        decimal? maxPrecio = null, 
        bool? soloOfertas = null, 
        string? marca = null, 
        string? sabor = null, 
        string? tamano = null,
        string? presentacion = null);

    Task<ApiResponse<ProductoDto>> GetByIdAsync(int id);
    Task<ApiResponse<ProductoDto>> CreateAsync(ProductoCreateDto dto);
    Task<ApiResponse<ProductoDto>> UpdateAsync(int id, ProductoUpdateDto dto);
    Task<ApiResponse<bool>> DeleteAsync(int id);
    Task<ApiResponse<bool>> UpdateStockAsync(int id, int cantidad);
}