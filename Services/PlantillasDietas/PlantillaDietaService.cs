using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using NutriApi.DTOs.Dietas;
using NutriApi.DTOs.PlantillasDietas;
using NutriApi.Services.Dietas;
using NutriApp.Data;
using NutriApp.Enums;
using NutriApp.Models.Dietas;
using NutriApp.Models.PlantillasDietas;

namespace NutriApi.Services.PlantillasDietas;

public class PlantillaDietaService : IPlantillaDietaService
{
    private readonly NutriAppDbContext _context;

    private readonly IDietaService _dietaService;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public PlantillaDietaService(NutriAppDbContext context, IDietaService dietaService)
    {
        _context = context;

        _dietaService = dietaService;
    }

    // ==========================================
    // CREAR DESDE DIETA
    // ==========================================

    public async Task<ResultadoDieta<PlantillaDietaDetalleDto>> CrearDesdeDietaAsync(
        int nutricionistaId,
        int pacienteId,
        int dietaId,
        CrearPlantillaDietaDto dto
    )
    {
        if (string.IsNullOrWhiteSpace(dto.Nombre))
        {
            return Error<PlantillaDietaDetalleDto>(
                "El nombre de la plantilla es obligatorio.",
                TipoErrorDieta.Validacion
            );
        }

        if (dto.Nombre.Trim().Length > 150)
        {
            return Error<PlantillaDietaDetalleDto>(
                "El nombre no puede superar los 150 caracteres.",
                TipoErrorDieta.Validacion
            );
        }

        if (dto.Descripcion?.Trim().Length > 500)
        {
            return Error<PlantillaDietaDetalleDto>(
                "La descripción no puede superar los 500 caracteres.",
                TipoErrorDieta.Validacion
            );
        }

        /*
         * Cargamos todo el árbol reutilizable.
         *
         * El ownership exige:
         *
         * dieta
         * -> paciente
         * -> nutricionista autenticado
         */

        var dieta = await _context
            .Dietas.AsNoTracking()
            .Include(d => d.Comidas)
                .ThenInclude(c => c.Secciones)
                    .ThenInclude(s => s.Opciones)
                        .ThenInclude(o => o.Items)
                            .ThenInclude(i => i.Alternativas)
            .Include(d => d.Hidratacion)
            .Include(d => d.Suplementacion)
                .ThenInclude(s => s.Items)
            .AsSplitQuery()
            .FirstOrDefaultAsync(d =>
                d.Id == dietaId
                && d.PacienteId == pacienteId
                && d.Paciente.NutricionistaId == nutricionistaId
            );

        if (dieta is null)
        {
            return Error<PlantillaDietaDetalleDto>(
                "Dieta no encontrada.",
                TipoErrorDieta.NoEncontrado
            );
        }

        var contenido = MapearContenido(dieta);

        var json = JsonSerializer.Serialize(contenido, JsonOptions);

        var plantilla = new PlantillaDieta
        {
            NutricionistaId = nutricionistaId,

            Nombre = dto.Nombre.Trim(),

            Descripcion = Limpiar(dto.Descripcion),

            ContenidoJson = json,

            FechaCreacion = DateTime.UtcNow,
        };

        _context.PlantillasDietas.Add(plantilla);

        await _context.SaveChangesAsync();

        return new ResultadoDieta<PlantillaDietaDetalleDto>
        {
            Exitoso = true,

            Datos = MapearDetalle(plantilla, contenido),

            TipoError = TipoErrorDieta.Ninguno,
        };
    }

    // ==========================================
    // LISTAR
    // ==========================================

    public async Task<ResultadoDieta<List<PlantillaDietaListadoDto>>> ObtenerTodasAsync(
        int nutricionistaId
    )
    {
        var plantillas = await _context
            .PlantillasDietas.AsNoTracking()
            .Where(p => p.NutricionistaId == nutricionistaId)
            .OrderByDescending(p => p.FechaCreacion)
            .ToListAsync();

        var resultado = plantillas.Select(MapearListado).ToList();

        return new ResultadoDieta<List<PlantillaDietaListadoDto>>
        {
            Exitoso = true,

            Datos = resultado,

            TipoError = TipoErrorDieta.Ninguno,
        };
    }

    // ==========================================
    // DETALLE
    // ==========================================

    public async Task<ResultadoDieta<PlantillaDietaDetalleDto>> ObtenerPorIdAsync(
        int nutricionistaId,
        int plantillaId
    )
    {
        var plantilla = await _context
            .PlantillasDietas.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == plantillaId && p.NutricionistaId == nutricionistaId);

        if (plantilla is null)
        {
            return Error<PlantillaDietaDetalleDto>(
                "Plantilla no encontrada.",
                TipoErrorDieta.NoEncontrado
            );
        }

        var contenido = Deserializar(plantilla.ContenidoJson);

        if (contenido is null)
        {
            return Error<PlantillaDietaDetalleDto>(
                "El contenido de la plantilla no es válido.",
                TipoErrorDieta.Validacion
            );
        }

        return new ResultadoDieta<PlantillaDietaDetalleDto>
        {
            Exitoso = true,

            Datos = MapearDetalle(plantilla, contenido),

            TipoError = TipoErrorDieta.Ninguno,
        };
    }

    // ==========================================
    // APLICAR A PACIENTE
    // ==========================================

    public async Task<ResultadoDieta<DietaDetalleDto>> AplicarAPacienteAsync(
        int nutricionistaId,
        int plantillaId,
        int pacienteId
    )
    {
        /*
         * La plantilla tiene que pertenecer
         * al nutricionista autenticado.
         */

        var plantilla = await _context
            .PlantillasDietas.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == plantillaId && p.NutricionistaId == nutricionistaId);

        if (plantilla is null)
        {
            return Error<DietaDetalleDto>("Plantilla no encontrada.", TipoErrorDieta.NoEncontrado);
        }

        /*
         * Y también el paciente destino.
         */

        var pacienteExiste = await _context
            .Pacientes.AsNoTracking()
            .AnyAsync(p => p.Id == pacienteId && p.NutricionistaId == nutricionistaId);

        if (!pacienteExiste)
        {
            return Error<DietaDetalleDto>("Paciente no encontrado.", TipoErrorDieta.NoEncontrado);
        }

        var contenido = Deserializar(plantilla.ContenidoJson);

        if (contenido is null)
        {
            return Error<DietaDetalleDto>(
                "El contenido de la plantilla no es válido.",
                TipoErrorDieta.Validacion
            );
        }

        var errorContenido = await ValidarContenidoAsync(nutricionistaId, contenido);

        if (errorContenido is not null)
        {
            return Error<DietaDetalleDto>(errorContenido, TipoErrorDieta.Validacion);
        }

        /*
         * Tenemos EnableRetryOnFailure en Npgsql.
         *
         * Igual que DuplicarAsync, la transacción
         * manual debe correr dentro de la estrategia
         * de ejecución de EF.
         */

        var estrategia = _context.Database.CreateExecutionStrategy();

        var nuevaDietaId = 0;

        await estrategia.ExecuteAsync(async () =>
        {
            _context.ChangeTracker.Clear();

            var ultimaVersion =
                await _context
                    .Dietas.Where(d => d.PacienteId == pacienteId)
                    .Select(d => (int?)d.Version)
                    .MaxAsync()
                ?? 0;

            await using var transaccion = await _context.Database.BeginTransactionAsync();

            try
            {
                // ======================================
                // DIETA
                // ======================================

                var nuevaDieta = new Dieta
                {
                    PacienteId = pacienteId,

                    /*
                     * Para el MVP usamos el nombre
                     * de la plantilla también como
                     * nombre inicial de la dieta.
                     */
                    Nombre = plantilla.Nombre,

                    Descripcion = plantilla.Descripcion,

                    Version = ultimaVersion + 1,

                    /*
                     * Al aplicar una plantilla
                     * comienza hoy.
                     *
                     * No copiamos fechas del
                     * paciente original.
                     */
                    FechaInicio = DateOnly.FromDateTime(DateTime.UtcNow),

                    FechaFin = null,

                    Estado = EstadoDieta.Borrador,

                    ObservacionesGenerales = contenido.ObservacionesGenerales,

                    FechaCreacion = DateTime.UtcNow,
                };

                _context.Dietas.Add(nuevaDieta);

                await _context.SaveChangesAsync();

                nuevaDietaId = nuevaDieta.Id;

                // ======================================
                // HIDRATACIÓN
                // ======================================

                if (contenido.Hidratacion is not null)
                {
                    var hidratacion = new HidratacionDieta
                    {
                        DietaId = nuevaDieta.Id,

                        MililitrosDiarios = contenido.Hidratacion.MililitrosDiarios,

                        VasosDiarios = contenido.Hidratacion.VasosDiarios,

                        Observaciones = contenido.Hidratacion.Observaciones,
                    };

                    _context.HidratacionesDietas.Add(hidratacion);
                }

                // ======================================
                // SUPLEMENTACIÓN
                // ======================================

                if (contenido.Suplementacion is not null)
                {
                    var suplementacion = new SuplementacionDieta
                    {
                        DietaId = nuevaDieta.Id,

                        ObservacionesGenerales = contenido.Suplementacion.ObservacionesGenerales,
                    };

                    _context.SuplementacionesDietas.Add(suplementacion);

                    await _context.SaveChangesAsync();

                    foreach (var itemDto in contenido.Suplementacion.Items.OrderBy(i => i.Orden))
                    {
                        var item = new ItemSuplementacion
                        {
                            SuplementacionDietaId = suplementacion.Id,

                            Nombre = itemDto.Nombre,

                            Cantidad = itemDto.Cantidad,

                            Unidad = itemDto.Unidad,

                            Momento = itemDto.Momento,

                            Indicaciones = itemDto.Indicaciones,

                            Orden = itemDto.Orden,
                        };

                        _context.ItemsSuplementacion.Add(item);
                    }
                }

                // ======================================
                // COMIDAS
                // ======================================

                var comidasNuevas = new Dictionary<PlantillaComidaDto, Comida>();

                foreach (var comidaDto in contenido.Comidas.OrderBy(c => c.Orden))
                {
                    var comida = new Comida
                    {
                        DietaId = nuevaDieta.Id,

                        Nombre = comidaDto.Nombre,

                        Tipo = (TipoComida)comidaDto.Tipo,

                        Orden = comidaDto.Orden,

                        Observaciones = comidaDto.Observaciones,
                    };

                    _context.Comidas.Add(comida);

                    comidasNuevas[comidaDto] = comida;
                }

                await _context.SaveChangesAsync();

                // ======================================
                // SECCIONES
                // ======================================

                var seccionesNuevas = new Dictionary<PlantillaSeccionDto, SeccionComida>();

                foreach (var comidaDto in contenido.Comidas)
                {
                    var comida = comidasNuevas[comidaDto];

                    foreach (var seccionDto in comidaDto.Secciones.OrderBy(s => s.Orden))
                    {
                        var seccion = new SeccionComida
                        {
                            ComidaId = comida.Id,

                            Nombre = seccionDto.Nombre,

                            Tipo = (TipoSeccionComida)seccionDto.Tipo,

                            Orden = seccionDto.Orden,

                            Observaciones = seccionDto.Observaciones,
                        };

                        _context.SeccionesComidas.Add(seccion);

                        seccionesNuevas[seccionDto] = seccion;
                    }
                }

                await _context.SaveChangesAsync();

                // ======================================
                // OPCIONES
                // ======================================

                var opcionesNuevas = new Dictionary<PlantillaOpcionDto, OpcionSeccionComida>();

                foreach (var comidaDto in contenido.Comidas)
                {
                    foreach (var seccionDto in comidaDto.Secciones)
                    {
                        var seccion = seccionesNuevas[seccionDto];

                        foreach (var opcionDto in seccionDto.Opciones.OrderBy(o => o.Orden))
                        {
                            var opcion = new OpcionSeccionComida
                            {
                                SeccionComidaId = seccion.Id,

                                Nombre = opcionDto.Nombre,

                                Orden = opcionDto.Orden,

                                EsPredeterminada = opcionDto.EsPredeterminada,

                                Observaciones = opcionDto.Observaciones,
                            };

                            _context.OpcionesSeccionesComidas.Add(opcion);

                            opcionesNuevas[opcionDto] = opcion;
                        }
                    }
                }

                await _context.SaveChangesAsync();

                // ======================================
                // ITEMS
                // ======================================

                var itemsNuevos = new Dictionary<PlantillaItemDto, ItemOpcionComida>();

                foreach (var comidaDto in contenido.Comidas)
                {
                    foreach (var seccionDto in comidaDto.Secciones)
                    {
                        foreach (var opcionDto in seccionDto.Opciones)
                        {
                            var opcion = opcionesNuevas[opcionDto];

                            foreach (var itemDto in opcionDto.Items.OrderBy(i => i.Orden))
                            {
                                var item = new ItemOpcionComida
                                {
                                    OpcionSeccionComidaId = opcion.Id,

                                    AlimentoId = itemDto.AlimentoId,

                                    Cantidad = itemDto.Cantidad,

                                    UnidadMedida = (UnidadMedida)itemDto.UnidadMedida,

                                    Indicaciones = itemDto.Indicaciones,

                                    Orden = itemDto.Orden,
                                };

                                _context.ItemsOpcionesComidas.Add(item);

                                itemsNuevos[itemDto] = item;
                            }
                        }
                    }
                }

                await _context.SaveChangesAsync();

                // ======================================
                // ALTERNATIVAS
                // ======================================

                foreach (var comidaDto in contenido.Comidas)
                {
                    foreach (var seccionDto in comidaDto.Secciones)
                    {
                        foreach (var opcionDto in seccionDto.Opciones)
                        {
                            foreach (var itemDto in opcionDto.Items)
                            {
                                var item = itemsNuevos[itemDto];

                                foreach (var alternativaDto in itemDto.Alternativas)
                                {
                                    var alternativa = new AlternativaItemComida
                                    {
                                        ItemOpcionComidaId = item.Id,

                                        AlimentoId = alternativaDto.AlimentoId,

                                        GrupoEquivalenciaId = alternativaDto.GrupoEquivalenciaId,

                                        Activa = alternativaDto.Activa,
                                    };

                                    _context.AlternativasItemsComidas.Add(alternativa);
                                }
                            }
                        }
                    }
                }

                await _context.SaveChangesAsync();

                await transaccion.CommitAsync();
            }
            catch
            {
                await transaccion.RollbackAsync();

                throw;
            }
        });

        return await _dietaService.ObtenerPorIdAsync(nutricionistaId, pacienteId, nuevaDietaId);
    }

    // ==========================================
    // ELIMINAR
    // ==========================================

    public async Task<ResultadoDieta<bool>> EliminarAsync(int nutricionistaId, int plantillaId)
    {
        var plantilla = await _context.PlantillasDietas.FirstOrDefaultAsync(p =>
            p.Id == plantillaId && p.NutricionistaId == nutricionistaId
        );

        if (plantilla is null)
        {
            return Error<bool>("Plantilla no encontrada.", TipoErrorDieta.NoEncontrado);
        }

        _context.PlantillasDietas.Remove(plantilla);

        await _context.SaveChangesAsync();

        return new ResultadoDieta<bool>
        {
            Exitoso = true,

            Datos = true,

            TipoError = TipoErrorDieta.Ninguno,
        };
    }

    // ==========================================
    // VALIDAR CONTENIDO
    // ==========================================

    private async Task<string?> ValidarContenidoAsync(
        int nutricionistaId,
        PlantillaDietaContenidoDto contenido
    )
    {
        foreach (var comida in contenido.Comidas)
        {
            if (!Enum.IsDefined(typeof(TipoComida), comida.Tipo))
            {
                return "La plantilla contiene un tipo de comida inválido.";
            }

            foreach (var seccion in comida.Secciones)
            {
                if (!Enum.IsDefined(typeof(TipoSeccionComida), seccion.Tipo))
                {
                    return "La plantilla contiene un tipo de sección inválido.";
                }

                foreach (var opcion in seccion.Opciones)
                {
                    foreach (var item in opcion.Items)
                    {
                        if (!Enum.IsDefined(typeof(UnidadMedida), item.UnidadMedida))
                        {
                            return "La plantilla contiene una unidad de medida inválida.";
                        }
                    }
                }
            }
        }

        var items = contenido
            .Comidas.SelectMany(c => c.Secciones)
            .SelectMany(s => s.Opciones)
            .SelectMany(o => o.Items)
            .ToList();

        var alimentosIds = items
            .Select(i => i.AlimentoId)
            .Concat(items.SelectMany(i => i.Alternativas).Select(a => a.AlimentoId))
            .Distinct()
            .ToList();

        if (alimentosIds.Count > 0)
        {
            var alimentosValidos = await _context
                .Alimentos.AsNoTracking()
                .Where(a =>
                    alimentosIds.Contains(a.Id) && a.NutricionistaId == nutricionistaId && a.Activo
                )
                .Select(a => a.Id)
                .ToListAsync();

            if (alimentosValidos.Count != alimentosIds.Count)
            {
                return "La plantilla utiliza uno o más alimentos inexistentes, inactivos o pertenecientes a otro nutricionista.";
            }
        }

        var gruposIds = items
            .SelectMany(i => i.Alternativas)
            .Select(a => a.GrupoEquivalenciaId)
            .Distinct()
            .ToList();

        if (gruposIds.Count > 0)
        {
            var gruposValidos = await _context
                .GruposEquivalencias.AsNoTracking()
                .Where(g => gruposIds.Contains(g.Id) && g.NutricionistaId == nutricionistaId)
                .Select(g => g.Id)
                .ToListAsync();

            if (gruposValidos.Count != gruposIds.Count)
            {
                return "La plantilla utiliza grupos de equivalencia que ya no están disponibles.";
            }
        }

        return null;
    }

    // ==========================================
    // MAPEAR CONTENIDO
    // ==========================================

    private static PlantillaDietaContenidoDto MapearContenido(Dieta dieta)
    {
        return new PlantillaDietaContenidoDto
        {
            ObservacionesGenerales = dieta.ObservacionesGenerales,

            Comidas = dieta
                .Comidas.OrderBy(c => c.Orden)
                .Select(c => new PlantillaComidaDto
                {
                    Nombre = c.Nombre,

                    Tipo = (int)c.Tipo,

                    Orden = c.Orden,

                    Observaciones = c.Observaciones,

                    Secciones = c
                        .Secciones.OrderBy(s => s.Orden)
                        .Select(s => new PlantillaSeccionDto
                        {
                            Nombre = s.Nombre,

                            Tipo = (int)s.Tipo,

                            Orden = s.Orden,

                            Observaciones = s.Observaciones,

                            Opciones = s
                                .Opciones.OrderBy(o => o.Orden)
                                .Select(o => new PlantillaOpcionDto
                                {
                                    Nombre = o.Nombre,

                                    Orden = o.Orden,

                                    EsPredeterminada = o.EsPredeterminada,

                                    Observaciones = o.Observaciones,

                                    Items = o
                                        .Items.OrderBy(i => i.Orden)
                                        .Select(i => new PlantillaItemDto
                                        {
                                            AlimentoId = i.AlimentoId,

                                            Cantidad = i.Cantidad,

                                            UnidadMedida = (int)i.UnidadMedida,

                                            Indicaciones = i.Indicaciones,

                                            Orden = i.Orden,

                                            Alternativas = i
                                                .Alternativas.Select(
                                                    a => new PlantillaAlternativaDto
                                                    {
                                                        AlimentoId = a.AlimentoId,

                                                        GrupoEquivalenciaId = a.GrupoEquivalenciaId,

                                                        Activa = a.Activa,
                                                    }
                                                )
                                                .ToList(),
                                        })
                                        .ToList(),
                                })
                                .ToList(),
                        })
                        .ToList(),
                })
                .ToList(),

            Hidratacion = dieta.Hidratacion is null
                ? null
                : new PlantillaHidratacionDto
                {
                    MililitrosDiarios = dieta.Hidratacion.MililitrosDiarios,

                    VasosDiarios = dieta.Hidratacion.VasosDiarios,

                    Observaciones = dieta.Hidratacion.Observaciones,
                },

            Suplementacion = dieta.Suplementacion is null
                ? null
                : new PlantillaSuplementacionDto
                {
                    ObservacionesGenerales = dieta.Suplementacion.ObservacionesGenerales,

                    Items = dieta
                        .Suplementacion.Items.OrderBy(i => i.Orden)
                        .Select(i => new PlantillaItemSuplementacionDto
                        {
                            Nombre = i.Nombre,

                            Cantidad = i.Cantidad,

                            Unidad = i.Unidad,

                            Momento = i.Momento,

                            Indicaciones = i.Indicaciones,

                            Orden = i.Orden,
                        })
                        .ToList(),
                },
        };
    }

    // ==========================================
    // MAPEAR RESPUESTAS
    // ==========================================

    private static PlantillaDietaDetalleDto MapearDetalle(
        PlantillaDieta plantilla,
        PlantillaDietaContenidoDto contenido
    )
    {
        return new PlantillaDietaDetalleDto
        {
            Id = plantilla.Id,

            Nombre = plantilla.Nombre,

            Descripcion = plantilla.Descripcion,

            Contenido = contenido,

            FechaCreacion = plantilla.FechaCreacion,

            FechaActualizacion = plantilla.FechaActualizacion,
        };
    }

    private static PlantillaDietaListadoDto MapearListado(PlantillaDieta plantilla)
    {
        var contenido = Deserializar(plantilla.ContenidoJson);

        return new PlantillaDietaListadoDto
        {
            Id = plantilla.Id,

            Nombre = plantilla.Nombre,

            Descripcion = plantilla.Descripcion,

            CantidadComidas = contenido?.Comidas.Count ?? 0,

            FechaCreacion = plantilla.FechaCreacion,

            FechaActualizacion = plantilla.FechaActualizacion,
        };
    }

    // ==========================================
    // JSON
    // ==========================================

    private static PlantillaDietaContenidoDto? Deserializar(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<PlantillaDietaContenidoDto>(json, JsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    // ==========================================
    // STRING
    // ==========================================

    private static string? Limpiar(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            return null;
        }

        return valor.Trim();
    }

    // ==========================================
    // ERROR
    // ==========================================

    private static ResultadoDieta<T> Error<T>(string mensaje, TipoErrorDieta tipo)
    {
        return new ResultadoDieta<T>
        {
            Exitoso = false,

            Error = mensaje,

            TipoError = tipo,
        };
    }
}
