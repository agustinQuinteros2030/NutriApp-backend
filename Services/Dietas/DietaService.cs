using Microsoft.EntityFrameworkCore;
using NutriApi.Calculos;
using NutriApi.DTOs.Dietas;
using NutriApp.Data;
using NutriApp.Enums;
using NutriApp.Models.Dietas;

namespace NutriApi.Services.Dietas;

public class DietaService : IDietaService
{
    private readonly NutriAppDbContext _context;

    public DietaService(NutriAppDbContext context)
    {
        _context = context;
    }

    // ==========================================
    // CREAR
    // ==========================================

    public async Task<ResultadoDieta<DietaDetalleDto>> CrearAsync(
        int nutricionistaId,
        int pacienteId,
        CrearDietaDto dto
    )
    {
        var pacienteExiste = await _context
            .Pacientes.AsNoTracking()
            .AnyAsync(p => p.Id == pacienteId && p.NutricionistaId == nutricionistaId);

        if (!pacienteExiste)
        {
            return DietaNoEncontrada("Paciente no encontrado.");
        }

        if (dto.FechaFin.HasValue && dto.FechaFin.Value < dto.FechaInicio)
        {
            return ErrorValidacion("La fecha de fin no puede ser anterior a la fecha de inicio.");
        }

        /*
         * Cada nueva dieta del paciente
         * incrementa la versión.
         */

        var ultimaVersion =
            await _context
                .Dietas.Where(d => d.PacienteId == pacienteId)
                .Select(d => (int?)d.Version)
                .MaxAsync()
            ?? 0;

        var dieta = new Dieta
        {
            PacienteId = pacienteId,

            Nombre = dto.Nombre.Trim(),

            Descripcion = dto.Descripcion?.Trim(),

            Version = ultimaVersion + 1,

            FechaInicio = dto.FechaInicio,

            FechaFin = dto.FechaFin,

            Estado = EstadoDieta.Borrador,

            ObservacionesGenerales = dto.ObservacionesGenerales?.Trim(),

            FechaCreacion = DateTime.UtcNow,
        };

        _context.Dietas.Add(dieta);

        await _context.SaveChangesAsync();

        /*
         * Una dieta recién creada todavía
         * no tiene comidas.
         *
         * Por eso sus totales comienzan en cero.
         */

        return new ResultadoDieta<DietaDetalleDto>
        {
            Exitoso = true,

            Datos = MapearDetalle(dieta, 0),

            TipoError = TipoErrorDieta.Ninguno,
        };
    }

    // ==========================================
    // LISTAR DIETAS DEL PACIENTE
    // ==========================================

    public async Task<ResultadoDieta<List<DietaListadoDto>>> ObtenerTodasAsync(
        int nutricionistaId,
        int pacienteId
    )
    {
        var pacienteExiste = await _context
            .Pacientes.AsNoTracking()
            .AnyAsync(p => p.Id == pacienteId && p.NutricionistaId == nutricionistaId);

        if (!pacienteExiste)
        {
            return new ResultadoDieta<List<DietaListadoDto>>
            {
                Exitoso = false,

                Error = "Paciente no encontrado.",

                TipoError = TipoErrorDieta.NoEncontrado,
            };
        }

        /*
         * El listado sigue siendo liviano.
         *
         * No cargamos todo el árbol nutricional
         * porque solamente necesitamos
         * información resumida de cada dieta.
         */

        var dietas = await _context
            .Dietas.AsNoTracking()
            .Where(d => d.PacienteId == pacienteId)
            .OrderByDescending(d => d.Version)
            .Select(d => new DietaListadoDto
            {
                Id = d.Id,

                Nombre = d.Nombre,

                Version = d.Version,

                Estado = d.Estado.ToString(),

                FechaInicio = d.FechaInicio,

                FechaFin = d.FechaFin,

                CantidadComidas = d.Comidas.Count,

                FechaCreacion = d.FechaCreacion,
            })
            .ToListAsync();

        return new ResultadoDieta<List<DietaListadoDto>>
        {
            Exitoso = true,

            Datos = dietas,

            TipoError = TipoErrorDieta.Ninguno,
        };
    }

    // ==========================================
    // DETALLE
    // ==========================================

    public async Task<ResultadoDieta<DietaDetalleDto>> ObtenerPorIdAsync(
        int nutricionistaId,
        int pacienteId,
        int dietaId
    )
    {
        /*
         * Para obtener los totales del plan
         * necesitamos llegar hasta el alimento:
         *
         * Dieta
         * -> Comidas
         * -> Secciones
         * -> Opciones
         * -> Items
         * -> Alimento
         */

        var dieta = await _context
            .Dietas.AsNoTracking()
            .Include(d => d.Comidas)
                .ThenInclude(c => c.Secciones)
                    .ThenInclude(s => s.Opciones)
                        .ThenInclude(o => o.Items)
                            .ThenInclude(i => i.Alimento)
            .Where(d =>
                d.Id == dietaId
                && d.PacienteId == pacienteId
                && d.Paciente.NutricionistaId == nutricionistaId
            )
            .AsSplitQuery()
            .FirstOrDefaultAsync();

        if (dieta is null)
        {
            return DietaNoEncontrada();
        }

        return new ResultadoDieta<DietaDetalleDto>
        {
            Exitoso = true,

            Datos = MapearDetalle(dieta, dieta.Comidas.Count),

            TipoError = TipoErrorDieta.Ninguno,
        };
    }

    // ==========================================
    // EDITAR
    // ==========================================

    public async Task<ResultadoDieta<DietaDetalleDto>> EditarAsync(
        int nutricionistaId,
        int pacienteId,
        int dietaId,
        EditarDietaDto dto
    )
    {
        var dieta = await _context
            .Dietas.Include(d => d.Comidas)
                .ThenInclude(c => c.Secciones)
                    .ThenInclude(s => s.Opciones)
                        .ThenInclude(o => o.Items)
                            .ThenInclude(i => i.Alimento)
            .AsSplitQuery()
            .FirstOrDefaultAsync(d =>
                d.Id == dietaId
                && d.PacienteId == pacienteId
                && d.Paciente.NutricionistaId == nutricionistaId
            );

        if (dieta is null)
        {
            return DietaNoEncontrada();
        }

        if (dieta.Estado == EstadoDieta.Archivada)
        {
            return ErrorValidacion("No se puede editar una dieta archivada.");
        }

        if (dto.FechaFin.HasValue && dto.FechaFin.Value < dto.FechaInicio)
        {
            return ErrorValidacion("La fecha de fin no puede ser anterior a la fecha de inicio.");
        }

        dieta.Nombre = dto.Nombre.Trim();

        dieta.Descripcion = dto.Descripcion?.Trim();

        dieta.FechaInicio = dto.FechaInicio;

        dieta.FechaFin = dto.FechaFin;

        dieta.ObservacionesGenerales = dto.ObservacionesGenerales?.Trim();

        dieta.FechaActualizacion = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return new ResultadoDieta<DietaDetalleDto>
        {
            Exitoso = true,

            Datos = MapearDetalle(dieta, dieta.Comidas.Count),

            TipoError = TipoErrorDieta.Ninguno,
        };
    }

    // ==========================================
    // DUPLICAR
    // ==========================================

    public async Task<ResultadoDieta<DietaDetalleDto>> DuplicarAsync(
        int nutricionistaId,
        int pacienteId,
        int dietaId
    )
    {
        /*
         * Cargamos TODO el árbol de la dieta origen.
         *
         * Dieta
         *   -> Comidas
         *      -> Secciones
         *         -> Opciones
         *            -> Items
         *               -> Alternativas
         *
         * También:
         *   -> Hidratación
         *   -> Suplementación
         *      -> Items
         */

        var origen = await _context
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

        if (origen is null)
        {
            return DietaNoEncontrada();
        }

        /*
         * Como tenemos EnableRetryOnFailure configurado
         * para PostgreSQL/Npgsql, una transacción manual
         * debe ejecutarse dentro de la estrategia de
         * ejecución de EF Core.
         *
         * De esta forma, si ocurre un error transitorio,
         * EF puede reintentar toda la operación como una
         * única unidad.
         */

        var estrategia = _context.Database.CreateExecutionStrategy();

        var nuevaDietaId = 0;

        await estrategia.ExecuteAsync(async () =>
        {
            /*
             * Si la estrategia está reintentando la
             * operación, eliminamos del ChangeTracker
             * las entidades creadas en el intento anterior.
             *
             * La dieta origen fue cargada con AsNoTracking,
             * por lo que no se pierde nada necesario.
             */

            _context.ChangeTracker.Clear();

            // ==========================================
            // NUEVA VERSIÓN
            // ==========================================

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

                    Nombre = origen.Nombre,

                    Descripcion = origen.Descripcion,

                    Version = ultimaVersion + 1,

                    FechaInicio = origen.FechaInicio,

                    FechaFin = origen.FechaFin,

                    Estado = EstadoDieta.Borrador,

                    ObservacionesGenerales = origen.ObservacionesGenerales,

                    FechaCreacion = DateTime.UtcNow,
                };

                _context.Dietas.Add(nuevaDieta);

                await _context.SaveChangesAsync();

                nuevaDietaId = nuevaDieta.Id;

                // ======================================
                // HIDRATACIÓN
                // ======================================

                if (origen.Hidratacion is not null)
                {
                    var nuevaHidratacion = new HidratacionDieta
                    {
                        DietaId = nuevaDieta.Id,

                        MililitrosDiarios = origen.Hidratacion.MililitrosDiarios,

                        VasosDiarios = origen.Hidratacion.VasosDiarios,

                        Observaciones = origen.Hidratacion.Observaciones,
                    };

                    _context.HidratacionesDietas.Add(nuevaHidratacion);
                }

                // ======================================
                // SUPLEMENTACIÓN
                // ======================================

                if (origen.Suplementacion is not null)
                {
                    var nuevaSuplementacion = new SuplementacionDieta
                    {
                        DietaId = nuevaDieta.Id,

                        ObservacionesGenerales = origen.Suplementacion.ObservacionesGenerales,
                    };

                    _context.SuplementacionesDietas.Add(nuevaSuplementacion);

                    /*
                     * Necesitamos el nuevo Id para
                     * relacionar sus items.
                     */

                    await _context.SaveChangesAsync();

                    foreach (var item in origen.Suplementacion.Items.OrderBy(i => i.Orden))
                    {
                        var nuevoItem = new ItemSuplementacion
                        {
                            SuplementacionDietaId = nuevaSuplementacion.Id,

                            Nombre = item.Nombre,

                            Cantidad = item.Cantidad,

                            Unidad = item.Unidad,

                            Momento = item.Momento,

                            Indicaciones = item.Indicaciones,

                            Orden = item.Orden,
                        };

                        _context.ItemsSuplementacion.Add(nuevoItem);
                    }
                }

                // ======================================
                // COMIDAS
                // ======================================

                var comidasNuevas = new Dictionary<int, Comida>();

                foreach (var comidaOrigen in origen.Comidas.OrderBy(c => c.Orden))
                {
                    var comidaNueva = new Comida
                    {
                        DietaId = nuevaDieta.Id,

                        Nombre = comidaOrigen.Nombre,

                        Tipo = comidaOrigen.Tipo,

                        Orden = comidaOrigen.Orden,

                        Observaciones = comidaOrigen.Observaciones,
                    };

                    _context.Comidas.Add(comidaNueva);

                    comidasNuevas[comidaOrigen.Id] = comidaNueva;
                }

                await _context.SaveChangesAsync();

                // ======================================
                // SECCIONES
                // ======================================

                var seccionesNuevas = new Dictionary<int, SeccionComida>();

                foreach (var comidaOrigen in origen.Comidas)
                {
                    var comidaNueva = comidasNuevas[comidaOrigen.Id];

                    foreach (var seccionOrigen in comidaOrigen.Secciones.OrderBy(s => s.Orden))
                    {
                        var seccionNueva = new SeccionComida
                        {
                            ComidaId = comidaNueva.Id,

                            Nombre = seccionOrigen.Nombre,

                            Tipo = seccionOrigen.Tipo,

                            Orden = seccionOrigen.Orden,

                            Observaciones = seccionOrigen.Observaciones,
                        };

                        _context.SeccionesComidas.Add(seccionNueva);

                        seccionesNuevas[seccionOrigen.Id] = seccionNueva;
                    }
                }

                await _context.SaveChangesAsync();

                // ======================================
                // OPCIONES
                // ======================================

                var opcionesNuevas = new Dictionary<int, OpcionSeccionComida>();

                foreach (var comidaOrigen in origen.Comidas)
                {
                    foreach (var seccionOrigen in comidaOrigen.Secciones)
                    {
                        var seccionNueva = seccionesNuevas[seccionOrigen.Id];

                        foreach (var opcionOrigen in seccionOrigen.Opciones.OrderBy(o => o.Orden))
                        {
                            var opcionNueva = new OpcionSeccionComida
                            {
                                SeccionComidaId = seccionNueva.Id,

                                Nombre = opcionOrigen.Nombre,

                                Orden = opcionOrigen.Orden,

                                EsPredeterminada = opcionOrigen.EsPredeterminada,

                                Observaciones = opcionOrigen.Observaciones,
                            };

                            _context.OpcionesSeccionesComidas.Add(opcionNueva);

                            opcionesNuevas[opcionOrigen.Id] = opcionNueva;
                        }
                    }
                }

                await _context.SaveChangesAsync();

                // ======================================
                // ITEMS
                // ======================================

                var itemsNuevos = new Dictionary<int, ItemOpcionComida>();

                foreach (var comidaOrigen in origen.Comidas)
                {
                    foreach (var seccionOrigen in comidaOrigen.Secciones)
                    {
                        foreach (var opcionOrigen in seccionOrigen.Opciones)
                        {
                            var opcionNueva = opcionesNuevas[opcionOrigen.Id];

                            foreach (var itemOrigen in opcionOrigen.Items.OrderBy(i => i.Orden))
                            {
                                var itemNuevo = new ItemOpcionComida
                                {
                                    OpcionSeccionComidaId = opcionNueva.Id,

                                    AlimentoId = itemOrigen.AlimentoId,

                                    Cantidad = itemOrigen.Cantidad,

                                    UnidadMedida = itemOrigen.UnidadMedida,

                                    Indicaciones = itemOrigen.Indicaciones,

                                    Orden = itemOrigen.Orden,
                                };

                                _context.ItemsOpcionesComidas.Add(itemNuevo);

                                itemsNuevos[itemOrigen.Id] = itemNuevo;
                            }
                        }
                    }
                }

                await _context.SaveChangesAsync();

                // ======================================
                // ALTERNATIVAS
                // ======================================

                foreach (var comidaOrigen in origen.Comidas)
                {
                    foreach (var seccionOrigen in comidaOrigen.Secciones)
                    {
                        foreach (var opcionOrigen in seccionOrigen.Opciones)
                        {
                            foreach (var itemOrigen in opcionOrigen.Items)
                            {
                                var itemNuevo = itemsNuevos[itemOrigen.Id];

                                foreach (var alternativaOrigen in itemOrigen.Alternativas)
                                {
                                    var alternativaNueva = new AlternativaItemComida
                                    {
                                        ItemOpcionComidaId = itemNuevo.Id,

                                        AlimentoId = alternativaOrigen.AlimentoId,

                                        GrupoEquivalenciaId = alternativaOrigen.GrupoEquivalenciaId,

                                        Activa = alternativaOrigen.Activa,
                                    };

                                    _context.AlternativasItemsComidas.Add(alternativaNueva);
                                }
                            }
                        }
                    }
                }

                await _context.SaveChangesAsync();

                // ======================================
                // COMMIT
                // ======================================

                await transaccion.CommitAsync();
            }
            catch
            {
                await transaccion.RollbackAsync();

                throw;
            }
        });

        /*
         * Volvemos a consultar la nueva dieta
         * usando el mapper normal del servicio.
         *
         * Así no duplicamos la lógica de totales.
         */

        return await ObtenerPorIdAsync(nutricionistaId, pacienteId, nuevaDietaId);
    }

    // ==========================================
    // COPIAR A OTRO PACIENTE
    // ==========================================

    public async Task<ResultadoDieta<DietaDetalleDto>> CopiarAOtroPacienteAsync(
        int nutricionistaId,
        int pacienteOrigenId,
        int dietaId,
        int pacienteDestinoId
    )
    {
        if (pacienteOrigenId == pacienteDestinoId)
        {
            return ErrorValidacion("Para el mismo paciente utilizá la opción de duplicar dieta.");
        }

        /*
         * El paciente destino también debe pertenecer
         * al nutricionista autenticado.
         */

        var pacienteDestinoExiste = await _context
            .Pacientes.AsNoTracking()
            .AnyAsync(p => p.Id == pacienteDestinoId && p.NutricionistaId == nutricionistaId);

        if (!pacienteDestinoExiste)
        {
            return DietaNoEncontrada("Paciente destino no encontrado.");
        }

        /*
         * Cargamos TODO el árbol de la dieta origen.
         *
         * Dieta
         *   -> Comidas
         *      -> Secciones
         *         -> Opciones
         *            -> Items
         *               -> Alternativas
         *
         * También:
         *   -> Hidratación
         *   -> Suplementación
         *      -> Items
         */

        var origen = await _context
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
                && d.PacienteId == pacienteOrigenId
                && d.Paciente.NutricionistaId == nutricionistaId
            );

        if (origen is null)
        {
            return DietaNoEncontrada();
        }

        /*
         * Como tenemos EnableRetryOnFailure configurado
         * para PostgreSQL/Npgsql, una transacción manual
         * debe ejecutarse dentro de la estrategia de
         * ejecución de EF Core.
         *
         * De esta forma, si ocurre un error transitorio,
         * EF puede reintentar toda la operación como una
         * única unidad.
         */

        var estrategia = _context.Database.CreateExecutionStrategy();

        var nuevaDietaId = 0;

        await estrategia.ExecuteAsync(async () =>
        {
            /*
             * Si la estrategia está reintentando la
             * operación, eliminamos del ChangeTracker
             * las entidades creadas en el intento anterior.
             *
             * La dieta origen fue cargada con AsNoTracking,
             * por lo que no se pierde nada necesario.
             */

            _context.ChangeTracker.Clear();

            // ==========================================
            // NUEVA VERSIÓN
            // ==========================================

            var ultimaVersion =
                await _context
                    .Dietas.Where(d => d.PacienteId == pacienteDestinoId)
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
                    PacienteId = pacienteDestinoId,

                    Nombre = origen.Nombre,

                    Descripcion = origen.Descripcion,

                    Version = ultimaVersion + 1,

                    FechaInicio = origen.FechaInicio,

                    FechaFin = origen.FechaFin,

                    Estado = EstadoDieta.Borrador,

                    ObservacionesGenerales = origen.ObservacionesGenerales,

                    FechaCreacion = DateTime.UtcNow,
                };

                _context.Dietas.Add(nuevaDieta);

                await _context.SaveChangesAsync();

                nuevaDietaId = nuevaDieta.Id;

                // ======================================
                // HIDRATACIÓN
                // ======================================

                if (origen.Hidratacion is not null)
                {
                    var nuevaHidratacion = new HidratacionDieta
                    {
                        DietaId = nuevaDieta.Id,

                        MililitrosDiarios = origen.Hidratacion.MililitrosDiarios,

                        VasosDiarios = origen.Hidratacion.VasosDiarios,

                        Observaciones = origen.Hidratacion.Observaciones,
                    };

                    _context.HidratacionesDietas.Add(nuevaHidratacion);
                }

                // ======================================
                // SUPLEMENTACIÓN
                // ======================================

                if (origen.Suplementacion is not null)
                {
                    var nuevaSuplementacion = new SuplementacionDieta
                    {
                        DietaId = nuevaDieta.Id,

                        ObservacionesGenerales = origen.Suplementacion.ObservacionesGenerales,
                    };

                    _context.SuplementacionesDietas.Add(nuevaSuplementacion);

                    /*
                     * Necesitamos el nuevo Id para
                     * relacionar sus items.
                     */

                    await _context.SaveChangesAsync();

                    foreach (var item in origen.Suplementacion.Items.OrderBy(i => i.Orden))
                    {
                        var nuevoItem = new ItemSuplementacion
                        {
                            SuplementacionDietaId = nuevaSuplementacion.Id,

                            Nombre = item.Nombre,

                            Cantidad = item.Cantidad,

                            Unidad = item.Unidad,

                            Momento = item.Momento,

                            Indicaciones = item.Indicaciones,

                            Orden = item.Orden,
                        };

                        _context.ItemsSuplementacion.Add(nuevoItem);
                    }
                }

                // ======================================
                // COMIDAS
                // ======================================

                var comidasNuevas = new Dictionary<int, Comida>();

                foreach (var comidaOrigen in origen.Comidas.OrderBy(c => c.Orden))
                {
                    var comidaNueva = new Comida
                    {
                        DietaId = nuevaDieta.Id,

                        Nombre = comidaOrigen.Nombre,

                        Tipo = comidaOrigen.Tipo,

                        Orden = comidaOrigen.Orden,

                        Observaciones = comidaOrigen.Observaciones,
                    };

                    _context.Comidas.Add(comidaNueva);

                    comidasNuevas[comidaOrigen.Id] = comidaNueva;
                }

                await _context.SaveChangesAsync();

                // ======================================
                // SECCIONES
                // ======================================

                var seccionesNuevas = new Dictionary<int, SeccionComida>();

                foreach (var comidaOrigen in origen.Comidas)
                {
                    var comidaNueva = comidasNuevas[comidaOrigen.Id];

                    foreach (var seccionOrigen in comidaOrigen.Secciones.OrderBy(s => s.Orden))
                    {
                        var seccionNueva = new SeccionComida
                        {
                            ComidaId = comidaNueva.Id,

                            Nombre = seccionOrigen.Nombre,

                            Tipo = seccionOrigen.Tipo,

                            Orden = seccionOrigen.Orden,

                            Observaciones = seccionOrigen.Observaciones,
                        };

                        _context.SeccionesComidas.Add(seccionNueva);

                        seccionesNuevas[seccionOrigen.Id] = seccionNueva;
                    }
                }

                await _context.SaveChangesAsync();

                // ======================================
                // OPCIONES
                // ======================================

                var opcionesNuevas = new Dictionary<int, OpcionSeccionComida>();

                foreach (var comidaOrigen in origen.Comidas)
                {
                    foreach (var seccionOrigen in comidaOrigen.Secciones)
                    {
                        var seccionNueva = seccionesNuevas[seccionOrigen.Id];

                        foreach (var opcionOrigen in seccionOrigen.Opciones.OrderBy(o => o.Orden))
                        {
                            var opcionNueva = new OpcionSeccionComida
                            {
                                SeccionComidaId = seccionNueva.Id,

                                Nombre = opcionOrigen.Nombre,

                                Orden = opcionOrigen.Orden,

                                EsPredeterminada = opcionOrigen.EsPredeterminada,

                                Observaciones = opcionOrigen.Observaciones,
                            };

                            _context.OpcionesSeccionesComidas.Add(opcionNueva);

                            opcionesNuevas[opcionOrigen.Id] = opcionNueva;
                        }
                    }
                }

                await _context.SaveChangesAsync();

                // ======================================
                // ITEMS
                // ======================================

                var itemsNuevos = new Dictionary<int, ItemOpcionComida>();

                foreach (var comidaOrigen in origen.Comidas)
                {
                    foreach (var seccionOrigen in comidaOrigen.Secciones)
                    {
                        foreach (var opcionOrigen in seccionOrigen.Opciones)
                        {
                            var opcionNueva = opcionesNuevas[opcionOrigen.Id];

                            foreach (var itemOrigen in opcionOrigen.Items.OrderBy(i => i.Orden))
                            {
                                var itemNuevo = new ItemOpcionComida
                                {
                                    OpcionSeccionComidaId = opcionNueva.Id,

                                    AlimentoId = itemOrigen.AlimentoId,

                                    Cantidad = itemOrigen.Cantidad,

                                    UnidadMedida = itemOrigen.UnidadMedida,

                                    Indicaciones = itemOrigen.Indicaciones,

                                    Orden = itemOrigen.Orden,
                                };

                                _context.ItemsOpcionesComidas.Add(itemNuevo);

                                itemsNuevos[itemOrigen.Id] = itemNuevo;
                            }
                        }
                    }
                }

                await _context.SaveChangesAsync();

                // ======================================
                // ALTERNATIVAS
                // ======================================

                foreach (var comidaOrigen in origen.Comidas)
                {
                    foreach (var seccionOrigen in comidaOrigen.Secciones)
                    {
                        foreach (var opcionOrigen in seccionOrigen.Opciones)
                        {
                            foreach (var itemOrigen in opcionOrigen.Items)
                            {
                                var itemNuevo = itemsNuevos[itemOrigen.Id];

                                foreach (var alternativaOrigen in itemOrigen.Alternativas)
                                {
                                    var alternativaNueva = new AlternativaItemComida
                                    {
                                        ItemOpcionComidaId = itemNuevo.Id,

                                        AlimentoId = alternativaOrigen.AlimentoId,

                                        GrupoEquivalenciaId = alternativaOrigen.GrupoEquivalenciaId,

                                        Activa = alternativaOrigen.Activa,
                                    };

                                    _context.AlternativasItemsComidas.Add(alternativaNueva);
                                }
                            }
                        }
                    }
                }

                await _context.SaveChangesAsync();

                // ======================================
                // COMMIT
                // ======================================

                await transaccion.CommitAsync();
            }
            catch
            {
                await transaccion.RollbackAsync();

                throw;
            }
        });

        /*
         * Volvemos a consultar la nueva dieta
         * usando el mapper normal del servicio.
         *
         * Así no duplicamos la lógica de totales.
         */

        return await ObtenerPorIdAsync(nutricionistaId, pacienteDestinoId, nuevaDietaId);
    }

    // ==========================================
    // ACTIVAR
    // ==========================================

    public async Task<ResultadoDieta<DietaDetalleDto>> ActivarAsync(
        int nutricionistaId,
        int pacienteId,
        int dietaId
    )
    {
        var dieta = await _context
            .Dietas.Include(d => d.Comidas)
                .ThenInclude(c => c.Secciones)
                    .ThenInclude(s => s.Opciones)
                        .ThenInclude(o => o.Items)
                            .ThenInclude(i => i.Alimento)
            .AsSplitQuery()
            .FirstOrDefaultAsync(d =>
                d.Id == dietaId
                && d.PacienteId == pacienteId
                && d.Paciente.NutricionistaId == nutricionistaId
            );

        if (dieta is null)
        {
            return DietaNoEncontrada();
        }

        if (dieta.Estado == EstadoDieta.Archivada)
        {
            return ErrorValidacion("Una dieta archivada no puede volver a activarse.");
        }

        if (dieta.Estado == EstadoDieta.Activa)
        {
            return ErrorValidacion("La dieta ya se encuentra activa.");
        }

        /*
         * Solo permitimos una dieta activa
         * por paciente.
         *
         * Si existe una anterior,
         * la archivamos automáticamente.
         */

        var dietasActivasAnteriores = await _context
            .Dietas.Where(d =>
                d.PacienteId == pacienteId && d.Id != dietaId && d.Estado == EstadoDieta.Activa
            )
            .ToListAsync();

        foreach (var anterior in dietasActivasAnteriores)
        {
            anterior.Estado = EstadoDieta.Archivada;

            anterior.FechaFin ??= DateOnly.FromDateTime(DateTime.UtcNow);

            anterior.FechaActualizacion = DateTime.UtcNow;
        }

        dieta.Estado = EstadoDieta.Activa;

        dieta.FechaActualizacion = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return new ResultadoDieta<DietaDetalleDto>
        {
            Exitoso = true,

            Datos = MapearDetalle(dieta, dieta.Comidas.Count),

            TipoError = TipoErrorDieta.Ninguno,
        };
    }

    // ==========================================
    // ARCHIVAR
    // ==========================================

    public async Task<ResultadoDieta<DietaDetalleDto>> ArchivarAsync(
        int nutricionistaId,
        int pacienteId,
        int dietaId
    )
    {
        var dieta = await _context
            .Dietas.Include(d => d.Comidas)
                .ThenInclude(c => c.Secciones)
                    .ThenInclude(s => s.Opciones)
                        .ThenInclude(o => o.Items)
                            .ThenInclude(i => i.Alimento)
            .AsSplitQuery()
            .FirstOrDefaultAsync(d =>
                d.Id == dietaId
                && d.PacienteId == pacienteId
                && d.Paciente.NutricionistaId == nutricionistaId
            );

        if (dieta is null)
        {
            return DietaNoEncontrada();
        }

        if (dieta.Estado == EstadoDieta.Archivada)
        {
            return ErrorValidacion("La dieta ya se encuentra archivada.");
        }

        dieta.Estado = EstadoDieta.Archivada;

        dieta.FechaFin ??= DateOnly.FromDateTime(DateTime.UtcNow);

        dieta.FechaActualizacion = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return new ResultadoDieta<DietaDetalleDto>
        {
            Exitoso = true,

            Datos = MapearDetalle(dieta, dieta.Comidas.Count),

            TipoError = TipoErrorDieta.Ninguno,
        };
    }

    // ==========================================
    // ELIMINAR
    // ==========================================

    public async Task<ResultadoDieta<bool>> EliminarAsync(
        int nutricionistaId,
        int pacienteId,
        int dietaId
    )
    {
        var dieta = await _context.Dietas.FirstOrDefaultAsync(d =>
            d.Id == dietaId
            && d.PacienteId == pacienteId
            && d.Paciente.NutricionistaId == nutricionistaId
        );

        if (dieta is null)
        {
            return new ResultadoDieta<bool>
            {
                Exitoso = false,

                Error = "Dieta no encontrada.",

                TipoError = TipoErrorDieta.NoEncontrado,
            };
        }

        /*
         * Solamente permitimos eliminar
         * dietas que todavía están en borrador.
         *
         * Una dieta activa o archivada forma
         * parte del historial del paciente.
         */

        if (dieta.Estado != EstadoDieta.Borrador)
        {
            return new ResultadoDieta<bool>
            {
                Exitoso = false,

                Error = "Solo se pueden eliminar dietas en estado borrador.",

                TipoError = TipoErrorDieta.Validacion,
            };
        }

        _context.Dietas.Remove(dieta);

        await _context.SaveChangesAsync();

        return new ResultadoDieta<bool>
        {
            Exitoso = true,

            Datos = true,

            TipoError = TipoErrorDieta.Ninguno,
        };
    }

    // ==========================================
    // MAPPER
    // ==========================================

    private static DietaDetalleDto MapearDetalle(Dieta dieta, int cantidadComidas)
    {
        /*
         * Para calcular el total del plan:
         *
         * 1. Recorremos todas las comidas.
         *
         * 2. Juntamos todas sus secciones.
         *
         * 3. CalculadoraTotalesNutricionales
         *    toma únicamente la opción
         *    predeterminada de cada sección.
         *
         * 4. Suma los items de esas opciones.
         */

        var secciones =
            dieta.Comidas?.SelectMany(c => c.Secciones).ToList() ?? new List<SeccionComida>();

        var resultadoTotales = CalculadoraTotalesNutricionales.CalcularSecciones(secciones);

        return new DietaDetalleDto
        {
            Id = dieta.Id,

            PacienteId = dieta.PacienteId,

            Nombre = dieta.Nombre,

            Descripcion = dieta.Descripcion,

            Version = dieta.Version,

            Estado = dieta.Estado.ToString(),

            FechaInicio = dieta.FechaInicio,

            FechaFin = dieta.FechaFin,

            ObservacionesGenerales = dieta.ObservacionesGenerales,

            CantidadComidas = cantidadComidas,

            FechaCreacion = dieta.FechaCreacion,

            FechaActualizacion = dieta.FechaActualizacion,

            Totales = MapearTotales(resultadoTotales),
        };
    }

    // ==========================================
    // MAPPER TOTALES
    // ==========================================

    private static TotalesNutricionalesDto MapearTotales(ResultadoTotalesNutricionales resultado)
    {
        return new TotalesNutricionalesDto
        {
            Calorias = resultado.Calorias,

            Proteinas = resultado.Proteinas,

            Carbohidratos = resultado.Carbohidratos,

            Grasas = resultado.Grasas,

            CantidadItems = resultado.CantidadItems,

            ItemsSinCalculo = resultado.ItemsSinCalculo,

            SeccionesSinOpcionPredeterminada = resultado.SeccionesSinOpcionPredeterminada,

            EsCompleto = resultado.EsCompleto,
        };
    }

    // ==========================================
    // ERRORES
    // ==========================================

    private static ResultadoDieta<DietaDetalleDto> DietaNoEncontrada(
        string mensaje = "Dieta no encontrada."
    )
    {
        return new ResultadoDieta<DietaDetalleDto>
        {
            Exitoso = false,

            Error = mensaje,

            TipoError = TipoErrorDieta.NoEncontrado,
        };
    }

    private static ResultadoDieta<DietaDetalleDto> ErrorValidacion(string mensaje)
    {
        return new ResultadoDieta<DietaDetalleDto>
        {
            Exitoso = false,

            Error = mensaje,

            TipoError = TipoErrorDieta.Validacion,
        };
    }
}
