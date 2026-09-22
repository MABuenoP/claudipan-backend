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

    public async Task RegistrarAccionAsync(int? usuarioId, string usuarioEmail, string accion, string tabla, string? registroId, string? anterior = null, string? nuevo = null, string? ip = null)
    {
        try
        {
            var audit = new Auditoria
            {
                UsuarioId = usuarioId,
                UsuarioEmail = string.IsNullOrWhiteSpace(usuarioEmail) ? "Sistema" : usuarioEmail,
                Accion = accion,
                TablaAfectada = tabla,
                RegistroId = registroId,
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

    public async Task<IEnumerable<AuditoriaDto>> GetAllAsync(string? tabla = null, string? accion = null)
    {
        var query = _context.Auditorias.AsQueryable();

        if (!string.IsNullOrWhiteSpace(tabla))
            query = query.Where(a => a.TablaAfectada.Contains(tabla));

        if (!string.IsNullOrWhiteSpace(accion))
            query = query.Where(a => a.Accion == accion);

        var list = await query.OrderByDescending(a => a.Fecha).Take(200).ToListAsync();
        return _mapper.Map<IEnumerable<AuditoriaDto>>(list);
    }
}
