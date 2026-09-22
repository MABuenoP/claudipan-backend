using ClaudipanAPI.Data;
using ClaudipanAPI.Interfaces;
using ClaudipanAPI.Models.DTOs;
using ClaudipanAPI.Models.Entities;
using ClaudipanAPI.Models.Responses;
using Microsoft.EntityFrameworkCore;

namespace ClaudipanAPI.Services;

public class ClienteService : IClienteService
{
    private readonly AppDbContext _context;

    public ClienteService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<List<ClienteDto>>> GetAllAsync()
    {
        var clientes = await _context.Clientes
            .Select(c => new ClienteDto
            {
                Id = c.Id,
                Nombre = c.Nombre,
                Apellido = c.Apellido,
                Email = c.Email,
                Telefono = c.Telefono,
                Activo = c.Activo
            })
            .ToListAsync();

        return ApiResponse<List<ClienteDto>>.Ok(clientes);
    }

    public async Task<ApiResponse<ClienteDto>> GetByIdAsync(int id)
    {
        var cliente = await _context.Clientes.FindAsync(id);
        if (cliente == null)
            return ApiResponse<ClienteDto>.Fail("Cliente no encontrado");

        return ApiResponse<ClienteDto>.Ok(new ClienteDto
        {
            Id = cliente.Id,
            Nombre = cliente.Nombre,
            Apellido = cliente.Apellido,
            Email = cliente.Email,
            Telefono = cliente.Telefono,
            Activo = cliente.Activo
        });
    }

    public async Task<ApiResponse<ClienteDto>> CreateAsync(ClienteCreateDto dto)
    {
        var existe = await _context.Clientes.AnyAsync(c => c.Email == dto.Email);
        if (existe)
            return ApiResponse<ClienteDto>.Fail("Ya existe un cliente con ese correo");

        var cliente = new Cliente
        {
            Nombre = dto.Nombre,
            Apellido = dto.Apellido,
            Email = dto.Email,
            Telefono = dto.Telefono,
            Activo = true
        };

        _context.Clientes.Add(cliente);
        await _context.SaveChangesAsync();

        return ApiResponse<ClienteDto>.Ok(new ClienteDto
        {
            Id = cliente.Id,
            Nombre = cliente.Nombre,
            Apellido = cliente.Apellido,
            Email = cliente.Email,
            Telefono = cliente.Telefono,
            Activo = cliente.Activo
        }, "Cliente creado exitosamente");
    }

    public async Task<ApiResponse<ClienteDto>> UpdateAsync(int id, ClienteUpdateDto dto)
    {
        var cliente = await _context.Clientes.FindAsync(id);
        if (cliente == null)
            return ApiResponse<ClienteDto>.Fail("Cliente no encontrado");

        if (!string.IsNullOrWhiteSpace(dto.Nombre)) cliente.Nombre = dto.Nombre;
        if (!string.IsNullOrWhiteSpace(dto.Apellido)) cliente.Apellido = dto.Apellido;
        if (!string.IsNullOrWhiteSpace(dto.Email)) cliente.Email = dto.Email;
        if (dto.Telefono != null) cliente.Telefono = dto.Telefono;
        if (dto.Activo.HasValue) cliente.Activo = dto.Activo.Value;

        await _context.SaveChangesAsync();

        return ApiResponse<ClienteDto>.Ok(new ClienteDto
        {
            Id = cliente.Id,
            Nombre = cliente.Nombre,
            Apellido = cliente.Apellido,
            Email = cliente.Email,
            Telefono = cliente.Telefono,
            Activo = cliente.Activo
        }, "Cliente actualizado exitosamente");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int id)
    {
        var cliente = await _context.Clientes.FindAsync(id);
        if (cliente == null)
            return ApiResponse<bool>.Fail("Cliente no encontrado");

        _context.Clientes.Remove(cliente);
        await _context.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, "Cliente eliminado exitosamente");
    }
}
