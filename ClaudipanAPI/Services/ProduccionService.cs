using AutoMapper;
using ClaudipanAPI.Data;
using ClaudipanAPI.Interfaces;
using ClaudipanAPI.Models.DTOs;
using ClaudipanAPI.Models.Entities;
using ClaudipanAPI.Models.Responses;
using Microsoft.EntityFrameworkCore;

namespace ClaudipanAPI.Services;

public class ProduccionService : IProduccionService
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public ProduccionService(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<ApiResponse<List<RecetaProduccionDto>>> GetAllRecetasAsync()
    {
        await GarantizarInsumoYRecetasTransformacionAsync();

        var recetas = await _context.RecetasProduccion
            .Include(r => r.Producto)
            .Include(r => r.Detalles)
                .ThenInclude(d => d.Insumo)
            .Where(r => r.Activo)
            .OrderBy(r => r.NombreReceta)
            .ToListAsync();

        return ApiResponse<List<RecetaProduccionDto>>.Ok(_mapper.Map<List<RecetaProduccionDto>>(recetas));
    }

    public async Task<ApiResponse<RecetaProduccionDto>> GetRecetaByIdAsync(int id)
    {
        var receta = await _context.RecetasProduccion
            .Include(r => r.Producto)
            .Include(r => r.Detalles)
                .ThenInclude(d => d.Insumo)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (receta == null) return ApiResponse<RecetaProduccionDto>.Fail("Receta no encontrada");
        return ApiResponse<RecetaProduccionDto>.Ok(_mapper.Map<RecetaProduccionDto>(receta));
    }

    public async Task<ApiResponse<RecetaProduccionDto>> CreateRecetaAsync(RecetaCreateDto dto)
    {
        var producto = await _context.Productos.FindAsync(dto.ProductoId);
        if (producto == null) return ApiResponse<RecetaProduccionDto>.Fail("Producto no encontrado");

        var receta = new RecetaProduccion
        {
            ProductoId = dto.ProductoId,
            NombreReceta = dto.NombreReceta,
            Descripcion = dto.Descripcion,
            RendimientoUnidades = dto.RendimientoUnidades > 0 ? dto.RendimientoUnidades : 50,
            FechaCreacion = DateTime.UtcNow,
            Activo = true
        };

        decimal costoTotal = 0m;

        foreach (var d in dto.Detalles)
        {
            var insumo = await _context.Insumos.FindAsync(d.InsumoId);
            if (insumo != null)
            {
                costoTotal += d.CantidadNecesaria * insumo.CostoUnitario;
            }

            receta.Detalles.Add(new DetalleReceta
            {
                InsumoId = d.InsumoId,
                CantidadNecesaria = d.CantidadNecesaria,
                UnidadMedida = d.UnidadMedida
            });
        }

        receta.CostoTotalInsumos = costoTotal;
        receta.CostoUnitarioEstimado = receta.RendimientoUnidades > 0 ? (costoTotal / receta.RendimientoUnidades) : 0m;

        // Actualizar el costo base del producto
        producto.CostoBaseProduccion = receta.CostoUnitarioEstimado;

        _context.RecetasProduccion.Add(receta);
        await _context.SaveChangesAsync();

        await _context.Entry(receta).Reference(r => r.Producto).LoadAsync();
        foreach (var det in receta.Detalles)
        {
            await _context.Entry(det).Reference(d => d.Insumo).LoadAsync();
        }

        return ApiResponse<RecetaProduccionDto>.Ok(_mapper.Map<RecetaProduccionDto>(receta), "Receta creada y costo base de producto actualizado");
    }

    public async Task<ApiResponse<RecetaProduccionDto>> UpdateRecetaAsync(int id, RecetaUpdateDto dto)
    {
        var receta = await _context.RecetasProduccion
            .Include(r => r.Detalles)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (receta == null) return ApiResponse<RecetaProduccionDto>.Fail("Receta no encontrada");

        var producto = await _context.Productos.FindAsync(dto.ProductoId);
        if (producto == null) return ApiResponse<RecetaProduccionDto>.Fail("Producto no encontrado");

        receta.ProductoId = dto.ProductoId;
        receta.NombreReceta = dto.NombreReceta;
        receta.Descripcion = dto.Descripcion;
        receta.RendimientoUnidades = dto.RendimientoUnidades > 0 ? dto.RendimientoUnidades : 50;
        if (dto.Activo.HasValue)
        {
            receta.Activo = dto.Activo.Value;
        }

        // Limpiar detalles anteriores y reasignar
        _context.DetallesReceta.RemoveRange(receta.Detalles);
        receta.Detalles.Clear();

        decimal costoTotal = 0m;

        foreach (var d in dto.Detalles)
        {
            var insumo = await _context.Insumos.FindAsync(d.InsumoId);
            if (insumo != null)
            {
                costoTotal += d.CantidadNecesaria * insumo.CostoUnitario;
            }

            receta.Detalles.Add(new DetalleReceta
            {
                InsumoId = d.InsumoId,
                CantidadNecesaria = d.CantidadNecesaria,
                UnidadMedida = d.UnidadMedida
            });
        }

        receta.CostoTotalInsumos = costoTotal;
        receta.CostoUnitarioEstimado = receta.RendimientoUnidades > 0 ? (costoTotal / receta.RendimientoUnidades) : 0m;

        // Actualizar el costo base del producto
        producto.CostoBaseProduccion = receta.CostoUnitarioEstimado;

        await _context.SaveChangesAsync();

        await _context.Entry(receta).Reference(r => r.Producto).LoadAsync();
        foreach (var det in receta.Detalles)
        {
            await _context.Entry(det).Reference(d => d.Insumo).LoadAsync();
        }

        return ApiResponse<RecetaProduccionDto>.Ok(_mapper.Map<RecetaProduccionDto>(receta), "Fórmula maestra actualizada y costo base recalculado");
    }

    public async Task<ApiResponse<bool>> DeleteRecetaAsync(int id)
    {
        var receta = await _context.RecetasProduccion.FindAsync(id);
        if (receta == null) return ApiResponse<bool>.Fail("Receta no encontrada");

        receta.Activo = false;
        await _context.SaveChangesAsync();
        return ApiResponse<bool>.Ok(true, "Receta desactivada");
    }

    // --- PRE-CHEQUEO DE INSUMOS PARA GERENCIA Y ADMINISTRACIÓN ---
    public async Task<ApiResponse<PreChequeoInsumosResponseDto>> PreChequeoInsumosAsync(PreChequeoInsumosRequestDto dto)
    {
        var producto = await _context.Productos.FindAsync(dto.ProductoId);
        if (producto == null) return ApiResponse<PreChequeoInsumosResponseDto>.Fail("Producto no encontrado");

        RecetaProduccion? receta = null;
        if (dto.RecetaId.HasValue && dto.RecetaId.Value > 0)
        {
            receta = await _context.RecetasProduccion
                .Include(r => r.Detalles)
                    .ThenInclude(d => d.Insumo)
                .FirstOrDefaultAsync(r => r.Id == dto.RecetaId.Value);
        }
        else
        {
            receta = await _context.RecetasProduccion
                .Include(r => r.Detalles)
                    .ThenInclude(d => d.Insumo)
                .FirstOrDefaultAsync(r => r.ProductoId == dto.ProductoId && r.Activo);
        }

        var rendimiento = (receta != null && receta.RendimientoUnidades > 0) ? receta.RendimientoUnidades : 50;
        var cantidadProg = dto.CantidadProgramada > 0 ? dto.CantidadProgramada : rendimiento;
        var factorLote = (decimal)cantidadProg / rendimiento;

        var response = new PreChequeoInsumosResponseDto
        {
            ProductoId = producto.Id,
            ProductoNombre = producto.Nombre,
            RecetaId = receta?.Id,
            RecetaNombre = receta?.NombreReceta ?? $"Fórmula Estándar para {producto.Nombre}",
            CantidadProgramada = cantidadProg,
            RendimientoBaseReceta = rendimiento,
            FactorLote = factorLote,
            TieneDeficit = false
        };

        decimal costoTotal = 0m;
        var faltantesList = new List<string>();

        if (receta != null && receta.Detalles.Any())
        {
            foreach (var det in receta.Detalles)
            {
                var insumo = det.Insumo ?? await _context.Insumos.FindAsync(det.InsumoId);
                var req = det.CantidadNecesaria * factorLote;
                var stockActual = insumo?.StockActual ?? 0m;
                var stockResultante = stockActual - req;
                var esDeficit = stockResultante < 0;
                var costoUnit = insumo?.CostoUnitario ?? 0m;
                var costoSub = req * costoUnit;
                costoTotal += costoSub;

                if (esDeficit)
                {
                    response.TieneDeficit = true;
                    faltantesList.Add($"{insumo?.Nombre ?? "Insumo"}: Requiere {req:F2} {det.UnidadMedida}, Stock {stockActual:F2} (Faltan {Math.Abs(stockResultante):F2})");
                }

                response.Insumos.Add(new ItemPreChequeoInsumoDto
                {
                    InsumoId = det.InsumoId,
                    InsumoNombre = insumo?.Nombre ?? $"Insumo #{det.InsumoId}",
                    UnidadMedida = det.UnidadMedida,
                    CantidadRequerida = req,
                    StockActualBodega = stockActual,
                    StockResultante = stockResultante,
                    EsDeficit = esDeficit,
                    CostoUnitario = costoUnit
                });
            }
        }
        else
        {
            // Sin receta vinculada: estimar con costo base
            costoTotal = cantidadProg * producto.CostoBaseProduccion;
        }

        response.CostoEstimadoTotal = costoTotal;
        response.InsumosFaltantesResumen = faltantesList.Any() 
            ? string.Join(" | ", faltantesList) 
            : "Todos los insumos disponibles en bodega";

        return ApiResponse<PreChequeoInsumosResponseDto>.Ok(response);
    }

    // --- ÓRDENES DE PRODUCCIÓN ---
    public async Task<ApiResponse<List<OrdenProduccionDto>>> GetAllOrdenesAsync(int? panaderoId = null, string? estado = null)
    {
        var query = _context.OrdenesProduccion
            .Include(o => o.PanaderoUsuario)
            .Include(o => o.Producto)
            .Include(o => o.Receta)
            .AsQueryable();

        if (panaderoId.HasValue && panaderoId.Value > 0)
            query = query.Where(o => o.PanaderoUsuarioId == panaderoId.Value);

        if (!string.IsNullOrWhiteSpace(estado))
            query = query.Where(o => o.Estado == estado);

        var list = await query.OrderByDescending(o => o.FechaOrden).ToListAsync();
        
        var dtos = list.Select(o =>
        {
            var dto = _mapper.Map<OrdenProduccionDto>(o);
            dto.RecetaNombre = o.Receta?.NombreReceta;
            return dto;
        }).ToList();

        return ApiResponse<List<OrdenProduccionDto>>.Ok(dtos);
    }

    public async Task<ApiResponse<OrdenProduccionDto>> GetOrdenByIdAsync(int id)
    {
        var orden = await _context.OrdenesProduccion
            .Include(o => o.PanaderoUsuario)
            .Include(o => o.Producto)
            .Include(o => o.Receta)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (orden == null) return ApiResponse<OrdenProduccionDto>.Fail("Orden de producción no encontrada");
        var dto = _mapper.Map<OrdenProduccionDto>(orden);
        dto.RecetaNombre = orden.Receta?.NombreReceta;
        return ApiResponse<OrdenProduccionDto>.Ok(dto);
    }

    // SOLO GERENTE O ADMINISTRADOR CREA LA ORDEN
    public async Task<ApiResponse<OrdenProduccionDto>> CreateOrdenAsync(int creadorUsuarioId, OrdenProduccionCreateDto dto)
    {
        var producto = await _context.Productos.FindAsync(dto.ProductoId);
        if (producto == null) return ApiResponse<OrdenProduccionDto>.Fail("Producto no encontrado");

        // Buscar receta asociada si no se especificó
        var recetaId = dto.RecetaId;
        if (!recetaId.HasValue || recetaId.Value <= 0)
        {
            var receta = await _context.RecetasProduccion.FirstOrDefaultAsync(r => r.ProductoId == dto.ProductoId && r.Activo);
            if (receta != null) recetaId = receta.Id;
        }

        // Asignar panadero: si se especifica en dto, o buscar un usuario con rol Panadero, o el creador
        int panaderoId = dto.PanaderoAsignadoId ?? 0;
        if (panaderoId <= 0)
        {
            var panaderoUser = await _context.Usuarios.FirstOrDefaultAsync(u => u.Rol == "Panadero" && u.Activo);
            panaderoId = panaderoUser?.Id ?? creadorUsuarioId;
        }

        var count = await _context.OrdenesProduccion.CountAsync() + 1;
        var orden = new OrdenProduccion
        {
            CodigoOrden = $"ORD-{DateTime.UtcNow:yyyyMM}-{count:D4}",
            PanaderoUsuarioId = panaderoId,
            ProductoId = dto.ProductoId,
            RecetaId = recetaId,
            CantidadProgramada = dto.CantidadProgramada > 0 ? dto.CantidadProgramada : 50,
            CantidadProducida = 0,
            CostoInsumos = 0m,
            Estado = "Pendiente",
            FechaOrden = DateTime.UtcNow,
            Observaciones = dto.Observaciones
        };

        _context.OrdenesProduccion.Add(orden);
        await _context.SaveChangesAsync();

        await _context.Entry(orden).Reference(o => o.PanaderoUsuario).LoadAsync();
        await _context.Entry(orden).Reference(o => o.Producto).LoadAsync();
        if (orden.RecetaId.HasValue)
            await _context.Entry(orden).Reference(o => o.Receta).LoadAsync();

        var resultDto = _mapper.Map<OrdenProduccionDto>(orden);
        resultDto.RecetaNombre = orden.Receta?.NombreReceta;

        return ApiResponse<OrdenProduccionDto>.Ok(resultDto, "Orden de producción creada por Gerencia/Administración. Lista para que el Panadero inicie el cargue.");
    }

    // --- PASO 1 (PANADERO): CARGUE DE INSUMOS & DESCUENTO (PERMITE GIRO EN NEGATIVO) ---
    public async Task<ApiResponse<OrdenProduccionDto>> CargarInsumosAsync(int id, int panaderoId, CargarInsumosDto? dto = null)
    {
        var orden = await _context.OrdenesProduccion
            .Include(o => o.PanaderoUsuario)
            .Include(o => o.Producto)
            .Include(o => o.Receta)
                .ThenInclude(r => r!.Detalles)
                    .ThenInclude(d => d.Insumo)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (orden == null) return ApiResponse<OrdenProduccionDto>.Fail("Orden no encontrada");
        if (orden.Estado != "Pendiente") return ApiResponse<OrdenProduccionDto>.Fail($"La orden ya se encuentra en estado '{orden.Estado}'");

        // Si el usuario ejecutor es panadero, asignarlo a la orden
        if (panaderoId > 0)
        {
            orden.PanaderoUsuarioId = panaderoId;
        }

        decimal costoInsumosTotal = 0m;
        var deficitInsumos = new List<string>();

        // Descontar insumos de bodega permitiendo giro en negativo
        if (orden.Receta != null && orden.Receta.Detalles.Any())
        {
            var factorLote = (decimal)orden.CantidadProgramada / (orden.Receta.RendimientoUnidades > 0 ? orden.Receta.RendimientoUnidades : 50m);

            foreach (var det in orden.Receta.Detalles)
            {
                var insumoRequerido = det.CantidadNecesaria * factorLote;
                var insumoDb = await _context.Insumos.FindAsync(det.InsumoId);
                if (insumoDb != null)
                {
                    // Giro en negativo permitido: se descuenta el stock real necesario
                    insumoDb.StockActual -= insumoRequerido;
                    insumoDb.FechaActualizacion = DateTime.UtcNow;

                    if (insumoDb.StockActual < 0)
                    {
                        deficitInsumos.Add($"{insumoDb.Nombre} (Saldo: {insumoDb.StockActual:F2} {insumoDb.UnidadMedida})");
                    }

                    costoInsumosTotal += insumoRequerido * insumoDb.CostoUnitario;
                }
            }
        }
        else
        {
            costoInsumosTotal = orden.CantidadProgramada * (orden.Producto?.CostoBaseProduccion ?? 0m);
        }

        orden.CostoInsumos = costoInsumosTotal;
        orden.Estado = "En Proceso";

        var notas = new List<string>();
        if (!string.IsNullOrWhiteSpace(orden.Observaciones)) notas.Add(orden.Observaciones);
        if (!string.IsNullOrWhiteSpace(dto?.Observaciones)) notas.Add(dto.Observaciones);

        if (deficitInsumos.Any())
        {
            notas.Add($"[ALERTA COMPRA GERENCIA] Insumos girados en negativo: {string.Join(", ", deficitInsumos)}. Adquirir materia prima urgentemente.");
        }
        else
        {
            notas.Add("[CARGUE EXITOSO] Insumos descontados de inventario satisfactoriamente.");
        }

        orden.Observaciones = string.Join(" | ", notas);

        await _context.SaveChangesAsync();

        var resultDto = _mapper.Map<OrdenProduccionDto>(orden);
        resultDto.RecetaNombre = orden.Receta?.NombreReceta;
        resultDto.TieneInsumosFaltantes = deficitInsumos.Any();
        resultDto.InsumosFaltantesDetalle = string.Join(", ", deficitInsumos);

        return ApiResponse<OrdenProduccionDto>.Ok(resultDto, 
            deficitInsumos.Any() 
                ? $"Insumos cargados y descontados. ¡Atención!: {deficitInsumos.Count} insumos giraron en negativo para compra urgente de Gerencia. Orden en proceso." 
                : "Insumos cargados exitosamente. Orden en estado: En proceso.");
    }

    // --- PASO 2 (PANADERO): MASA A PUNTO -> PASAR A HORNEANDO ---
    public async Task<ApiResponse<OrdenProduccionDto>> PasarHorneandoAsync(int id, int panaderoId, PasarHorneandoDto? dto = null)
    {
        var orden = await _context.OrdenesProduccion
            .Include(o => o.PanaderoUsuario)
            .Include(o => o.Producto)
            .Include(o => o.Receta)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (orden == null) return ApiResponse<OrdenProduccionDto>.Fail("Orden no encontrada");
        if (orden.Estado != "Preparando" && orden.Estado != "Pendiente" && orden.Estado != "En Proceso")
            return ApiResponse<OrdenProduccionDto>.Fail($"La orden no está en preparación (Estado actual: '{orden.Estado}')");

        orden.Estado = "Horneando";

        var horaStr = DateTime.UtcNow.AddHours(-5).ToString("HH:mm"); // Hora Colombia
        var notaHorno = $"[HORNO] Masa a punto. Entró a cámara de horneado a las {horaStr}.";
        
        if (!string.IsNullOrWhiteSpace(dto?.Observaciones))
            notaHorno += $" Nota: {dto.Observaciones}";

        orden.Observaciones = string.IsNullOrWhiteSpace(orden.Observaciones) 
            ? notaHorno 
            : $"{orden.Observaciones} | {notaHorno}";

        await _context.SaveChangesAsync();

        var resultDto = _mapper.Map<OrdenProduccionDto>(orden);
        resultDto.RecetaNombre = orden.Receta?.NombreReceta;
        return ApiResponse<OrdenProduccionDto>.Ok(resultDto, "Masa a punto confirmada. La orden ha pasado a fase de HORNEANDO.");
    }

    // --- PASO 3 (PANADERO / GERENTE / ADMIN): CARGAR PRODUCCIÓN Y CULMINAR LOTE ---
    public async Task<ApiResponse<OrdenProduccionDto>> FinalizarYCuantificarAsync(int id, int panaderoId, CuantificarProduccionDto dto)
    {
        var orden = await _context.OrdenesProduccion
            .Include(o => o.PanaderoUsuario)
            .Include(o => o.Producto)
            .Include(o => o.Receta)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (orden == null) return ApiResponse<OrdenProduccionDto>.Fail("Orden no encontrada");
        if (orden.Estado == "Terminado" || orden.Estado == "Entregada") 
            return ApiResponse<OrdenProduccionDto>.Fail("Esta orden ya fue finalizada anteriormente.");

        var totalProducido = dto.CantOptima + dto.CantBuenasCondiciones + dto.CantMalasCondiciones;
        if (totalProducido <= 0)
        {
            dto.CantOptima = orden.CantidadProgramada;
            totalProducido = orden.CantidadProgramada;
        }

        var totalAceptable = dto.CantOptima + dto.CantBuenasCondiciones;
        var malasCondiciones = dto.CantMalasCondiciones;

        orden.CantidadProducida = totalAceptable;
        orden.FechaEntrega = DateTime.UtcNow;
        orden.Estado = "Terminado";

        // 1. Sumar unidades aceptables al inventario del producto en vitrina
        if (orden.Producto != null && totalAceptable > 0)
        {
            orden.Producto.Stock += totalAceptable;
            orden.Producto.Disponible = true;
        }

        // 2. Procesar pérdidas: registrar en bajas/mermas Y sumar a insumos del Pan de Transformación
        if (malasCondiciones > 0)
        {
            // Sumar al insumo de transformación (Miga / Pan de Transformación)
            var insumoTransformacion = await ObtenerOCrearInsumoTransformacionAsync();
            var kgTransformacion = malasCondiciones * 0.08m; // Aprox 80g por unidad
            insumoTransformacion.StockActual += kgTransformacion;
            insumoTransformacion.FechaActualizacion = DateTime.UtcNow;

            // Registrar en bajas o mermas
            var costoUnit = orden.Producto?.CostoBaseProduccion ?? 0m;
            if (costoUnit <= 0 && orden.Producto != null) costoUnit = orden.Producto.Precio * 0.55m;

            var baja = new BajaProducto
            {
                ProductoId = orden.ProductoId,
                Cantidad = malasCondiciones,
                Motivo = "Transformacion",
                CostoUnitario = costoUnit,
                CostoPerdidaTotal = malasCondiciones * costoUnit,
                FechaBaja = DateTime.UtcNow,
                UsuarioId = (panaderoId > 0) ? panaderoId : orden.PanaderoUsuarioId,
                Observaciones = $"[ORDEN {orden.CodigoOrden}] Merma de producción ({malasCondiciones} panes / {kgTransformacion:F2} Kg) recuperada para insumo '{insumoTransformacion.Nombre}'."
            };

            _context.BajasProductos.Add(baja);

            var notaTransf = $"[PRODUCCIÓN / TRANSFORMACIÓN] {totalAceptable} unidades a vitrina | {malasCondiciones} pérdidas ({kgTransformacion:F2} Kg) sumadas al insumo '{insumoTransformacion.Nombre}' y registradas en bajas contables.";
            orden.Observaciones = string.IsNullOrWhiteSpace(orden.Observaciones) 
                ? notaTransf 
                : $"{orden.Observaciones} | {notaTransf}";
        }
        else
        {
            var notaAcept = $"[PRODUCCIÓN CULMINADA] {totalAceptable} unidades ingresadas a inventario de vitrina al 100% óptimas.";
            orden.Observaciones = string.IsNullOrWhiteSpace(orden.Observaciones) 
                ? notaAcept 
                : $"{orden.Observaciones} | {notaAcept}";
        }

        if (!string.IsNullOrWhiteSpace(dto.Observaciones))
        {
            orden.Observaciones += $" | Nota: {dto.Observaciones}";
        }

        await _context.SaveChangesAsync();

        var resultDto = _mapper.Map<OrdenProduccionDto>(orden);
        resultDto.RecetaNombre = orden.Receta?.NombreReceta;
        resultDto.CantOptima = dto.CantOptima;
        resultDto.CantBuenasCondiciones = dto.CantBuenasCondiciones;
        resultDto.CantMalasCondiciones = dto.CantMalasCondiciones;
        resultDto.DestinoMalasCondiciones = "Transformacion";

        return ApiResponse<OrdenProduccionDto>.Ok(resultDto, 
            $"¡Producción culminada! Se ingresaron {totalAceptable} unidades a vitrina y {malasCondiciones} pérdidas registradas en bajas contables y sumadas al inventario de Pan de Transformación.");
    }

    public async Task<ApiResponse<OrdenProduccionDto>> IniciarOrdenAsync(int id)
    {
        return await CargarInsumosAsync(id, 0);
    }

    public async Task<ApiResponse<OrdenProduccionDto>> EntregarProduccionAsync(int id, EntregarProduccionDto dto)
    {
        var cuantDto = new CuantificarProduccionDto
        {
            CantOptima = dto.CantidadProducida,
            CantBuenasCondiciones = 0,
            CantMalasCondiciones = 0,
            DestinoMalasCondiciones = "Ninguno",
            Observaciones = dto.Observaciones
        };
        return await FinalizarYCuantificarAsync(id, 0, cuantDto);
    }

    public async Task<ApiResponse<bool>> CancelarOrdenAsync(int id)
    {
        var orden = await _context.OrdenesProduccion.FindAsync(id);
        if (orden == null) return ApiResponse<bool>.Fail("Orden no encontrada");

        orden.Estado = "Cancelada";
        await _context.SaveChangesAsync();
        return ApiResponse<bool>.Ok(true, "Orden cancelada");
    }

    // --- MÉTODOS AUXILIARES PARA TRANSFORMACIÓN ---
    private async Task<Insumo> ObtenerOCrearInsumoTransformacionAsync()
    {
        var insumo = await _context.Insumos.FirstOrDefaultAsync(i => i.Nombre.Contains("Transformación") || i.Nombre.Contains("Miga de Pan"));
        if (insumo != null) return insumo;

        insumo = new Insumo
        {
            Nombre = "Pan de Transformación (Harina de Pan / Pastas Negras)",
            Descripcion = "Miga y piezas de pan recuperadas de horneado en mala condición para reciclaje y transformación gastronómica.",
            UnidadMedida = "Kg",
            StockActual = 0m,
            StockMinimo = 5m,
            CostoUnitario = 1500m,
            ProveedorPrincipal = "Producción Interna Claudipan",
            Activo = true,
            FechaActualizacion = DateTime.UtcNow
        };

        _context.Insumos.Add(insumo);
        await _context.SaveChangesAsync();
        return insumo;
    }

    private async Task GarantizarInsumoYRecetasTransformacionAsync()
    {
        var insumoTransf = await ObtenerOCrearInsumoTransformacionAsync();

        // 1. Verificar o Crear Producto "Harina de Pan Rallado"
        var prodHarina = await _context.Productos.FirstOrDefaultAsync(p => p.Nombre.Contains("Harina de Pan"));
        if (prodHarina == null)
        {
            var cat = await _context.Categorias.FirstOrDefaultAsync() ?? new Categoria { Nombre = "Especiales Claudipan" };
            if (cat.Id == 0) { _context.Categorias.Add(cat); await _context.SaveChangesAsync(); }

            prodHarina = new Producto
            {
                Nombre = "Harina de Pan Claudipan (500g)",
                Descripcion = "Harina de pan tostado artesanal elaborada a partir de transformación de panes seleccionados.",
                Precio = 3500m,
                CostoBaseProduccion = 1200m,
                Stock = 10,
                CategoriaId = cat.Id,
                Disponible = true,
                FechaCreacion = DateTime.UtcNow
            };
            _context.Productos.Add(prodHarina);
            await _context.SaveChangesAsync();
        }

        // 2. Verificar o Crear Receta de Harina de Pan
        var recetaHarina = await _context.RecetasProduccion.FirstOrDefaultAsync(r => r.ProductoId == prodHarina.Id && r.Activo);
        if (recetaHarina == null)
        {
            recetaHarina = new RecetaProduccion
            {
                ProductoId = prodHarina.Id,
                NombreReceta = "Fórmula Maestra - Harina de Pan",
                Descripcion = "Tostado y molienda de pan de transformación para apanados y cocina.",
                RendimientoUnidades = 20,
                CostoTotalInsumos = 10000m,
                CostoUnitarioEstimado = 500m,
                Activo = true,
                FechaCreacion = DateTime.UtcNow
            };
            recetaHarina.Detalles.Add(new DetalleReceta
            {
                InsumoId = insumoTransf.Id,
                CantidadNecesaria = 5m,
                UnidadMedida = "Kg"
            });
            _context.RecetasProduccion.Add(recetaHarina);
            await _context.SaveChangesAsync();
        }

        // 3. Verificar o Crear Producto "Pastas Negras Tradicionales"
        var prodPastas = await _context.Productos.FirstOrDefaultAsync(p => p.Nombre.Contains("Pastas Negras"));
        if (prodPastas == null)
        {
            var cat = await _context.Categorias.FirstOrDefaultAsync() ?? new Categoria { Nombre = "Dulces y Repostería" };
            if (cat.Id == 0) { _context.Categorias.Add(cat); await _context.SaveChangesAsync(); }

            prodPastas = new Producto
            {
                Nombre = "Pastas Negras Tradicionales (Unidad)",
                Descripcion = "Dulce tradicional de panadería elaborado con masa enriquecida de pan de transformación, panela y canela.",
                Precio = 2000m,
                CostoBaseProduccion = 800m,
                Stock = 15,
                CategoriaId = cat.Id,
                Disponible = true,
                FechaCreacion = DateTime.UtcNow
            };
            _context.Productos.Add(prodPastas);
            await _context.SaveChangesAsync();
        }

        // 4. Verificar o Crear Receta de Pastas Negras
        var recetaPastas = await _context.RecetasProduccion.FirstOrDefaultAsync(r => r.ProductoId == prodPastas.Id && r.Activo);
        if (recetaPastas == null)
        {
            recetaPastas = new RecetaProduccion
            {
                ProductoId = prodPastas.Id,
                NombreReceta = "Fórmula Maestra - Pastas Negras",
                Descripcion = "Cocción de pan de transformación en melado de panela, especias y horneado en moldes.",
                RendimientoUnidades = 30,
                CostoTotalInsumos = 15000m,
                CostoUnitarioEstimado = 500m,
                Activo = true,
                FechaCreacion = DateTime.UtcNow
            };
            recetaPastas.Detalles.Add(new DetalleReceta
            {
                InsumoId = insumoTransf.Id,
                CantidadNecesaria = 4m,
                UnidadMedida = "Kg"
            });
            _context.RecetasProduccion.Add(recetaPastas);
            await _context.SaveChangesAsync();
        }
    }
}
