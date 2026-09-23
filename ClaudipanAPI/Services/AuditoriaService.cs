using AutoMapper;
using ClaudipanAPI.Data;
using ClaudipanAPI.Interfaces;
using ClaudipanAPI.Models.DTOs;
using ClaudipanAPI.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClaudipanAPI.Services;

public class AuditoriaService : IAuditoriaService
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public AuditoriaService(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task RegistrarAccionAsync(
        int? usuarioId, 
        string usuarioEmail, 
        string accion, 
        string tabla, 
        string? registroId, 
        string? anterior = null, 
        string? nuevo = null, 
        string? ip = null,
        string? formulario = null,
        string? usuarioNombre = null,
        string? usuarioRol = null)
    {
        try
        {
            var audit = new Auditoria
            {
                UsuarioId = usuarioId,
                UsuarioNombre = usuarioNombre,
                UsuarioEmail = string.IsNullOrWhiteSpace(usuarioEmail) ? "Sistema" : usuarioEmail,
                UsuarioRol = usuarioRol,
                Accion = accion,
                TablaAfectada = tabla,
                RegistroId = registroId,
                Formulario = formulario,
                ValoresAnteriores = anterior,
                ValoresNuevos = nuevo,
                Fecha = DateTime.UtcNow,
                DireccionIp = ip
            };

            _context.Auditorias.Add(audit);
            await _context.SaveChangesAsync();
        }
        catch
        {
            // Evitar que un fallo de auditoría rompa la transacción principal
        }
    }

    public async Task<IEnumerable<AuditoriaDto>> GetAllAsync(
        string? tabla = null, 
        string? accion = null,
        string? formulario = null,
        string? busqueda = null)
    {
        var query = _context.Auditorias.AsQueryable();

        if (!string.IsNullOrWhiteSpace(tabla))
            query = query.Where(a => a.TablaAfectada.ToLower().Contains(tabla.ToLower()));

        if (!string.IsNullOrWhiteSpace(accion))
            query = query.Where(a => a.Accion == accion);

        if (!string.IsNullOrWhiteSpace(formulario))
            query = query.Where(a => a.Formulario != null && a.Formulario.ToLower().Contains(formulario.ToLower()));

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            var b = busqueda.ToLower();
            query = query.Where(a => 
                (a.UsuarioEmail != null && a.UsuarioEmail.ToLower().Contains(b)) ||
                (a.UsuarioNombre != null && a.UsuarioNombre.ToLower().Contains(b)) ||
                (a.DireccionIp != null && a.DireccionIp.ToLower().Contains(b)) ||
                (a.RegistroId != null && a.RegistroId.ToLower().Contains(b)) ||
                (a.Formulario != null && a.Formulario.ToLower().Contains(b)) ||
                (a.ValoresNuevos != null && a.ValoresNuevos.ToLower().Contains(b))
            );
        }

        var list = await query.OrderByDescending(a => a.Fecha).Take(500).ToListAsync();
        return _mapper.Map<IEnumerable<AuditoriaDto>>(list);
    }
}
