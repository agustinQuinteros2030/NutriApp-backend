using Microsoft.EntityFrameworkCore;

using NutriApi.Configuracion;
using NutriApi.DTOs.SeguimientoSemanal;
using NutriApi.Services.Notificaciones;

using NutriApp.Data;
using NutriApp.Enums.Notificaciones;
using NutriApp.Enums.Seguimiento;
using NutriApp.Models.Seguimiento;

namespace NutriApi.Services.SeguimientoSemanal;

public class SeguimientoSemanalService
    : ISeguimientoSemanalService
{
    private readonly NutriAppDbContext
        _context;

    private readonly INotificacionService
        _notificacionService;

    private readonly ILogger<SeguimientoSemanalService>
        _logger;


    public SeguimientoSemanalService(
        NutriAppDbContext context,
        INotificacionService notificacionService,
        ILogger<SeguimientoSemanalService> logger)
    {
        _context =
            context;

        _notificacionService =
            notificacionService;

        _logger =
            logger;
    }


    // ==========================================
    // PACIENTE - ESTADO DE LA SEMANA ACTUAL
    // ==========================================

    public async Task<
        ResultadoSeguimientoSemanal<
            EstadoSeguimientoSemanalDto>>
        ObtenerEstadoActualAsync(
            int pacienteId)
    {
        var pacienteExiste =
            await _context.Pacientes
                .AsNoTracking()
                .AnyAsync(p =>
                    p.Id == pacienteId
                );


        if (!pacienteExiste)
        {
            return Error<
                EstadoSeguimientoSemanalDto>(
                "Paciente no encontrado.",
                TipoErrorSeguimientoSemanal
                    .NoEncontrado
            );
        }


        var hoy =
            ObtenerFechaLocalActual();


        var semana =
            ObtenerSemana(
                hoy
            );


        var seguimiento =
            await _context
                .SeguimientosSemanalesPacientes
                .AsNoTracking()
                .Where(s =>
                    s.PacienteId ==
                    pacienteId
                    &&
                    s.FechaInicioSemana ==
                    semana.Inicio
                )
                .Select(s =>
                    new
                    {
                        s.Id
                    }
                )
                .FirstOrDefaultAsync();


        var dto =
            new EstadoSeguimientoSemanalDto
            {
                Disponible =
                    EstaDisponible(
                        hoy
                    ),

                Completado =
                    seguimiento is not null,

                FechaInicioSemana =
                    semana.Inicio,

                FechaFinSemana =
                    semana.Fin,

                SeguimientoId =
                    seguimiento?.Id
            };


        return Exito(
            dto
        );
    }


    // ==========================================
    // PACIENTE - CREAR SEGUIMIENTO ACTUAL
    // ==========================================

    public async Task<
        ResultadoSeguimientoSemanal<
            SeguimientoSemanalDto>>
        CrearActualAsync(
            int pacienteId,
            CrearSeguimientoSemanalDto dto)
    {
        /*
         * Antes solamente comprobábamos existencia.
         *
         * Ahora necesitamos además:
         *
         * - NutricionistaId
         * - Nombre
         * - Apellido
         *
         * para poder crear la notificación.
         */

        var paciente =
            await _context.Pacientes
                .AsNoTracking()
                .FirstOrDefaultAsync(p =>
                    p.Id ==
                    pacienteId
                );


        if (paciente is null)
        {
            return Error<
                SeguimientoSemanalDto>(
                "Paciente no encontrado.",
                TipoErrorSeguimientoSemanal
                    .NoEncontrado
            );
        }


        var hoy =
            ObtenerFechaLocalActual();


        /*
         * El formulario solamente puede
         * completarse entre viernes y domingo.
         */

        if (!EstaDisponible(hoy))
        {
            return Error<
                SeguimientoSemanalDto>(
                "El seguimiento semanal está disponible de viernes a domingo.",
                TipoErrorSeguimientoSemanal
                    .Validacion
            );
        }


        var errorValidacion =
            ValidarRespuestas(
                dto
            );


        if (errorValidacion is not null)
        {
            return Error<
                SeguimientoSemanalDto>(
                errorValidacion,
                TipoErrorSeguimientoSemanal
                    .Validacion
            );
        }


        var semana =
            ObtenerSemana(
                hoy
            );


        /*
         * No permitimos más de una respuesta
         * por paciente y semana.
         */

        var yaExiste =
            await _context
                .SeguimientosSemanalesPacientes
                .AsNoTracking()
                .AnyAsync(s =>
                    s.PacienteId ==
                    pacienteId
                    &&
                    s.FechaInicioSemana ==
                    semana.Inicio
                );


        if (yaExiste)
        {
            return Error<
                SeguimientoSemanalDto>(
                "El seguimiento de esta semana ya fue completado.",
                TipoErrorSeguimientoSemanal
                    .Conflicto
            );
        }


        var seguimiento =
            new SeguimientoSemanalPaciente
            {
                PacienteId =
                    pacienteId,

                FechaInicioSemana =
                    semana.Inicio,

                FechaFinSemana =
                    semana.Fin,

                FechaRespuesta =
                    DateTime.UtcNow,


                PesoActual =
                    dto.PesoActual,

                Adherencia =
                    dto.Adherencia,

                Descanso =
                    dto.Descanso,

                Digestiones =
                    dto.Digestiones,

                DetalleDigestiones =
                    Limpiar(
                        dto.DetalleDigestiones
                    ),

                RendimientoEntrenamientos =
                    dto.RendimientoEntrenamientos,

                CumplimientoHidratacion =
                    dto.CumplimientoHidratacion,

                RegularidadIntestinal =
                    dto.RegularidadIntestinal,

                TuvoMolestiaFisica =
                    dto.TuvoMolestiaFisica,

                DetalleMolestiaFisica =
                    dto.TuvoMolestiaFisica
                        ? Limpiar(
                            dto.DetalleMolestiaFisica
                        )
                        : null,

                SatisfaccionComunicacion =
                    dto.SatisfaccionComunicacion
            };


        _context
            .SeguimientosSemanalesPacientes
            .Add(
                seguimiento
            );


        try
        {
            await _context
                .SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            /*
             * Protección adicional ante dos requests
             * prácticamente simultáneos.
             *
             * La BD también tiene índice UNIQUE:
             * PacienteId + FechaInicioSemana.
             */

            var existeAhora =
                await _context
                    .SeguimientosSemanalesPacientes
                    .AsNoTracking()
                    .AnyAsync(s =>
                        s.PacienteId ==
                        pacienteId
                        &&
                        s.FechaInicioSemana ==
                        semana.Inicio
                    );


            if (existeAhora)
            {
                return Error<
                    SeguimientoSemanalDto>(
                    "El seguimiento de esta semana ya fue completado.",
                    TipoErrorSeguimientoSemanal
                        .Conflicto
                );
            }


            throw;
        }


        // ======================================
        // NOTIFICACIÓN AL NUTRICIONISTA
        // ======================================

        /*
         * El seguimiento ya quedó persistido.
         *
         * Si la notificación falla por algún
         * problema aislado, NO hacemos fallar
         * la operación principal.
         */

        var nombrePaciente =
            ObtenerNombrePaciente(
                paciente.Nombre,
                paciente.Apellido
            );


        await IntentarCrearNotificacionAsync(
            paciente.NutricionistaId,
            TipoNotificacion
                .SeguimientoRespondido,
            "Nuevo seguimiento semanal",
            $"{nombrePaciente} completó su seguimiento semanal.",
            "SeguimientoSemanal",
            seguimiento.Id
        );


        return Exito(
            Mapear(
                seguimiento
            )
        );
    }


    // ==========================================
    // PACIENTE - HISTORIAL PROPIO
    // ==========================================

    public async Task<
        ResultadoSeguimientoSemanal<
            List<SeguimientoSemanalDto>>>
        ObtenerPropiosAsync(
            int pacienteId)
    {
        var pacienteExiste =
            await _context.Pacientes
                .AsNoTracking()
                .AnyAsync(p =>
                    p.Id == pacienteId
                );


        if (!pacienteExiste)
        {
            return Error<
                List<SeguimientoSemanalDto>>(
                "Paciente no encontrado.",
                TipoErrorSeguimientoSemanal
                    .NoEncontrado
            );
        }


        var seguimientos =
            await _context
                .SeguimientosSemanalesPacientes
                .AsNoTracking()
                .Where(s =>
                    s.PacienteId ==
                    pacienteId
                )
                .OrderByDescending(s =>
                    s.FechaInicioSemana
                )
                .ThenByDescending(s =>
                    s.Id
                )
                .ToListAsync();


        return Exito(
            seguimientos
                .Select(
                    Mapear
                )
                .ToList()
        );
    }


    // ==========================================
    // NUTRICIONISTA - HISTORIAL DEL PACIENTE
    // ==========================================

    public async Task<
        ResultadoSeguimientoSemanal<
            List<SeguimientoSemanalDto>>>
        ObtenerPacienteAsync(
            int nutricionistaId,
            int pacienteId)
    {
        var pertenece =
            await PacientePerteneceAsync(
                nutricionistaId,
                pacienteId
            );


        if (!pertenece)
        {
            return Error<
                List<SeguimientoSemanalDto>>(
                "Paciente no encontrado.",
                TipoErrorSeguimientoSemanal
                    .NoEncontrado
            );
        }


        var seguimientos =
            await _context
                .SeguimientosSemanalesPacientes
                .AsNoTracking()
                .Where(s =>
                    s.PacienteId ==
                    pacienteId
                )
                .OrderByDescending(s =>
                    s.FechaInicioSemana
                )
                .ThenByDescending(s =>
                    s.Id
                )
                .ToListAsync();


        return Exito(
            seguimientos
                .Select(
                    Mapear
                )
                .ToList()
        );
    }


    // ==========================================
    // NUTRICIONISTA - DETALLE
    // ==========================================

    public async Task<
        ResultadoSeguimientoSemanal<
            SeguimientoSemanalDto>>
        ObtenerDetallePacienteAsync(
            int nutricionistaId,
            int pacienteId,
            int seguimientoId)
    {
        var seguimiento =
            await _context
                .SeguimientosSemanalesPacientes
                .AsNoTracking()
                .FirstOrDefaultAsync(s =>
                    s.Id ==
                    seguimientoId
                    &&
                    s.PacienteId ==
                    pacienteId
                    &&
                    s.Paciente
                        .NutricionistaId ==
                    nutricionistaId
                );


        if (seguimiento is null)
        {
            return Error<
                SeguimientoSemanalDto>(
                "Seguimiento semanal no encontrado.",
                TipoErrorSeguimientoSemanal
                    .NoEncontrado
            );
        }


        return Exito(
            Mapear(
                seguimiento
            )
        );
    }


    // ==========================================
    // NUTRICIONISTA - REVISAR
    // ==========================================

    public async Task<
        ResultadoSeguimientoSemanal<
            SeguimientoSemanalDto>>
        RevisarAsync(
            int nutricionistaId,
            int pacienteId,
            int seguimientoId,
            RevisarSeguimientoSemanalDto dto)
    {
        var seguimiento =
            await _context
                .SeguimientosSemanalesPacientes
                .FirstOrDefaultAsync(s =>
                    s.Id ==
                    seguimientoId
                    &&
                    s.PacienteId ==
                    pacienteId
                    &&
                    s.Paciente
                        .NutricionistaId ==
                    nutricionistaId
                );


        if (seguimiento is null)
        {
            return Error<
                SeguimientoSemanalDto>(
                "Seguimiento semanal no encontrado.",
                TipoErrorSeguimientoSemanal
                    .NoEncontrado
            );
        }


        if (string.IsNullOrWhiteSpace(
            dto.Revision))
        {
            return Error<
                SeguimientoSemanalDto>(
                "La revisión no puede estar vacía.",
                TipoErrorSeguimientoSemanal
                    .Validacion
            );
        }


        /*
         * Guardamos el estado ANTES de editar.
         *
         * Si FechaRevisionNutricionista ya tenía
         * valor significa que el nutricionista
         * está modificando una revisión existente.
         *
         * En ese caso NO volvemos a notificar.
         */

        var esPrimeraRevision =
            !seguimiento
                .FechaRevisionNutricionista
                .HasValue;


        seguimiento.RevisionNutricionista =
            dto.Revision.Trim();


        seguimiento.FechaRevisionNutricionista =
            DateTime.UtcNow;


        await _context
            .SaveChangesAsync();


        // ======================================
        // NOTIFICACIÓN AL PACIENTE
        // ======================================

        if (esPrimeraRevision)
        {
            await IntentarCrearNotificacionAsync(
                seguimiento.PacienteId,
                TipoNotificacion
                    .SeguimientoRevisado,
                "Seguimiento revisado",
                "Tu nutricionista revisó tu seguimiento semanal.",
                "SeguimientoSemanal",
                seguimiento.Id
            );
        }


        return Exito(
            Mapear(
                seguimiento
            )
        );
    }


    // ==========================================
    // OWNERSHIP
    // ==========================================

    private async Task<bool>
        PacientePerteneceAsync(
            int nutricionistaId,
            int pacienteId)
    {
        return await _context.Pacientes
            .AsNoTracking()
            .AnyAsync(p =>
                p.Id ==
                pacienteId
                &&
                p.NutricionistaId ==
                nutricionistaId
            );
    }


    // ==========================================
    // NOTIFICACIONES
    // ==========================================

    private async Task
        IntentarCrearNotificacionAsync(
            int usuarioId,
            TipoNotificacion tipo,
            string titulo,
            string mensaje,
            string? recursoTipo,
            int? recursoId)
    {
        try
        {
            await _notificacionService
                .CrearAsync(
                    usuarioId,
                    tipo,
                    titulo,
                    mensaje,
                    recursoTipo,
                    recursoId
                );
        }
        catch (Exception ex)
        {
            /*
             * La notificación es secundaria.
             *
             * No queremos convertir una operación
             * exitosa de seguimiento/revisión en
             * un 500 porque falló el subsistema
             * de notificaciones.
             */

            _logger.LogError(
                ex,
                "No se pudo crear la notificación {TipoNotificacion} para el usuario {UsuarioId}.",
                tipo,
                usuarioId
            );
        }
    }


    // ==========================================
    // NOMBRE DEL PACIENTE
    // ==========================================

    private static string
        ObtenerNombrePaciente(
            string? nombre,
            string? apellido)
    {
        var nombreCompleto =
            $"{nombre} {apellido}"
                .Trim();


        return string.IsNullOrWhiteSpace(
            nombreCompleto)
            ? "Un paciente"
            : nombreCompleto;
    }


    // ==========================================
    // VALIDACIÓN DE RESPUESTAS
    // ==========================================

    private static string?
        ValidarRespuestas(
            CrearSeguimientoSemanalDto dto)
    {
        if (dto.PesoActual <= 0)
        {
            return
                "El peso debe ser mayor a cero.";
        }


        if (dto.Adherencia < 1 ||
            dto.Adherencia > 10)
        {
            return
                "La adherencia debe estar entre 1 y 10.";
        }


        if (!Enum.IsDefined(
            dto.Descanso))
        {
            return
                "La opción de descanso no es válida.";
        }


        if (!Enum.IsDefined(
            dto.Digestiones))
        {
            return
                "La opción de digestiones no es válida.";
        }


        if (!Enum.IsDefined(
            dto.RendimientoEntrenamientos))
        {
            return
                "La opción de rendimiento no es válida.";
        }


        if (!Enum.IsDefined(
            dto.CumplimientoHidratacion))
        {
            return
                "La opción de hidratación no es válida.";
        }


        if (!Enum.IsDefined(
            dto.RegularidadIntestinal))
        {
            return
                "La opción de regularidad intestinal no es válida.";
        }


        if (dto.TuvoMolestiaFisica &&
            string.IsNullOrWhiteSpace(
                dto.DetalleMolestiaFisica
            ))
        {
            return
                "Indicá qué molestia física tuviste.";
        }


        if (dto.SatisfaccionComunicacion < 1 ||
            dto.SatisfaccionComunicacion > 10)
        {
            return
                "La satisfacción con la comunicación debe estar entre 1 y 10.";
        }


        return null;
    }


    // ==========================================
    // SEMANA
    // ==========================================

    private static (
        DateOnly Inicio,
        DateOnly Fin)
        ObtenerSemana(
            DateOnly fecha)
    {
        /*
         * Semana lógica:
         *
         * lunes -> domingo
         */

        var diasDesdeLunes =
            (
                7
                +
                (int)fecha.DayOfWeek
                -
                (int)DayOfWeek.Monday
            )
            % 7;


        var inicio =
            fecha.AddDays(
                -diasDesdeLunes
            );


        var fin =
            inicio.AddDays(
                6
            );


        return (
            inicio,
            fin
        );
    }


    // ==========================================
    // DISPONIBILIDAD
    // ==========================================

    private static bool
        EstaDisponible(
            DateOnly fecha)
    {
        var actual =
            (int)fecha.DayOfWeek;

        var inicio =
            (int)ConfiguracionSeguimientoSemanal
                .DiaSeguimiento;

        var fin =
            (int)ConfiguracionSeguimientoSemanal
                .UltimoDiaDisponible;


        /*
         * Caso normal:
         * martes -> jueves, por ejemplo.
         */

        if (inicio <= fin)
        {
            return actual >= inicio &&
                   actual <= fin;
        }


        /*
         * Caso que cruza el final de la semana:
         *
         * viernes (5) -> domingo (0)
         *
         * viernes = true
         * sábado  = true
         * domingo = true
         */

        return actual >= inicio ||
               actual <= fin;
    }


    // ==========================================
    // FECHA LOCAL
    // ==========================================

    private static DateOnly
        ObtenerFechaLocalActual()
    {
        var zonaHoraria =
            TimeZoneInfo
                .FindSystemTimeZoneById(
                    ConfiguracionSeguimientoSemanal
                        .ZonaHorariaId
                );


        var fechaLocal =
            TimeZoneInfo
                .ConvertTimeFromUtc(
                    DateTime.UtcNow,
                    zonaHoraria
                );


        return DateOnly
            .FromDateTime(
                fechaLocal
            );
    }


    // ==========================================
    // MAPPER
    // ==========================================

    private static SeguimientoSemanalDto
        Mapear(
            SeguimientoSemanalPaciente seguimiento)
    {
        return new SeguimientoSemanalDto
        {
            Id =
                seguimiento.Id,

            PacienteId =
                seguimiento.PacienteId,

            FechaInicioSemana =
                seguimiento.FechaInicioSemana,

            FechaFinSemana =
                seguimiento.FechaFinSemana,

            FechaRespuesta =
                seguimiento.FechaRespuesta,


            PesoActual =
                seguimiento.PesoActual,

            Adherencia =
                seguimiento.Adherencia,

            Descanso =
                seguimiento.Descanso
                    .ToString(),

            Digestiones =
                seguimiento.Digestiones
                    .ToString(),

            DetalleDigestiones =
                seguimiento.DetalleDigestiones,

            RendimientoEntrenamientos =
                seguimiento
                    .RendimientoEntrenamientos
                    .ToString(),

            CumplimientoHidratacion =
                seguimiento
                    .CumplimientoHidratacion
                    .ToString(),

            RegularidadIntestinal =
                seguimiento
                    .RegularidadIntestinal
                    .ToString(),

            TuvoMolestiaFisica =
                seguimiento
                    .TuvoMolestiaFisica,

            DetalleMolestiaFisica =
                seguimiento
                    .DetalleMolestiaFisica,

            SatisfaccionComunicacion =
                seguimiento
                    .SatisfaccionComunicacion,


            RevisionNutricionista =
                seguimiento
                    .RevisionNutricionista,

            FechaRevisionNutricionista =
                seguimiento
                    .FechaRevisionNutricionista
        };
    }


    // ==========================================
    // HELPERS
    // ==========================================

    private static string?
        Limpiar(
            string? valor)
    {
        return string.IsNullOrWhiteSpace(
            valor
        )
            ? null
            : valor.Trim();
    }


    private static ResultadoSeguimientoSemanal<T>
        Exito<T>(
            T datos)
    {
        return new ResultadoSeguimientoSemanal<T>
        {
            Exitoso =
                true,

            Datos =
                datos,

            TipoError =
                TipoErrorSeguimientoSemanal
                    .Ninguno
        };
    }


    private static ResultadoSeguimientoSemanal<T>
        Error<T>(
            string mensaje,
            TipoErrorSeguimientoSemanal tipo)
    {
        return new ResultadoSeguimientoSemanal<T>
        {
            Exitoso =
                false,

            Error =
                mensaje,

            TipoError =
                tipo
        };
    }
}