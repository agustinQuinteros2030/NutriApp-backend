using Microsoft.EntityFrameworkCore;

using NutriApi.DTOs.Turnos;
using NutriApi.Services.Notificaciones;

using NutriApp.Data;
using NutriApp.Enums.Notificaciones;

namespace NutriApi.Services.Turnos;

public class TurnoService
    : ITurnoService
{
    private readonly NutriAppDbContext
        _context;

    private readonly INotificacionService
        _notificacionService;

    private readonly ILogger<TurnoService>
        _logger;


    public TurnoService(
        NutriAppDbContext context,
        INotificacionService notificacionService,
        ILogger<TurnoService> logger)
    {
        _context =
            context;

        _notificacionService =
            notificacionService;

        _logger =
            logger;
    }


    // ==========================================
    // NUTRICIONISTA - CREAR
    // ==========================================

    public async Task<
        ResultadoTurno<TurnoDto>>
        CrearAsync(
            int nutricionistaId,
            int pacienteId,
            CrearTurnoDto dto)
    {
        var paciente =
            await _context.Pacientes
                .AsNoTracking()
                .FirstOrDefaultAsync(p =>
                    p.Id == pacienteId
                    &&
                    p.NutricionistaId ==
                    nutricionistaId
                );


        if (paciente is null)
        {
            return Error<TurnoDto>(
                "Paciente no encontrado.",
                TipoErrorTurno.NoEncontrado
            );
        }


        if (!paciente.Activo)
        {
            return Error<TurnoDto>(
                "No se pueden crear turnos para un paciente inactivo.",
                TipoErrorTurno.Validacion
            );
        }


        var errorValidacion =
            ValidarDatosTurno(
                dto.FechaHora,
                dto.Modalidad
            );


        if (errorValidacion is not null)
        {
            return Error<TurnoDto>(
                errorValidacion,
                TipoErrorTurno.Validacion
            );
        }


        /*
         * DateTimeOffset nos permite recibir:
         *
         * 2026-09-25T18:00:00-03:00
         *
         * y convertirlo inequívocamente a UTC.
         */

        var fechaHoraUtc =
            dto.FechaHora.UtcDateTime;


        var horarioOcupado =
            await ExisteColisionAsync(
                nutricionistaId,
                fechaHoraUtc
            );


        if (horarioOcupado)
        {
            return Error<TurnoDto>(
                "Ya existe otro turno programado en ese horario.",
                TipoErrorTurno.Conflicto
            );
        }


        var turno =
            new Turno
            {
                PacienteId =
                    pacienteId,

                FechaHora =
                    fechaHoraUtc,

                Modalidad =
                    dto.Modalidad,

                Estado =
                    EstadoTurno.Programado,

                Lugar =
                    Limpiar(
                        dto.Lugar
                    ),

                LinkReunion =
                    Limpiar(
                        dto.LinkReunion
                    ),

                Motivo =
                    Limpiar(
                        dto.Motivo
                    ),

                Observaciones =
                    Limpiar(
                        dto.Observaciones
                    ),

                FechaCreacion =
                    DateTime.UtcNow
            };


        _context.Turnos.Add(
            turno
        );


        await _context
            .SaveChangesAsync();


        // ======================================
        // NOTIFICACIÓN
        // ======================================

        await IntentarCrearNotificacionAsync(
            paciente.Id,
            TipoNotificacion.TurnoProgramado,
            "Nueva consulta programada",
            "Tu nutricionista programó una nueva consulta. Revisá tu agenda para ver la fecha y los detalles.",
            "Turno",
            turno.Id
        );


        return Exito(
            Mapear(
                turno,
                paciente.Nombre,
                paciente.Apellido
            )
        );
    }


    // ==========================================
    // NUTRICIONISTA - EDITAR / REPROGRAMAR
    // ==========================================

    public async Task<
        ResultadoTurno<TurnoDto>>
        EditarAsync(
            int nutricionistaId,
            int pacienteId,
            int turnoId,
            EditarTurnoDto dto)
    {
        var turno =
            await _context.Turnos
                .Include(t =>
                    t.Paciente
                )
                .FirstOrDefaultAsync(t =>
                    t.Id == turnoId
                    &&
                    t.PacienteId ==
                    pacienteId
                    &&
                    t.Paciente
                        .NutricionistaId ==
                    nutricionistaId
                );


        if (turno is null)
        {
            return Error<TurnoDto>(
                "Turno no encontrado.",
                TipoErrorTurno.NoEncontrado
            );
        }
        if (!turno.Paciente.Activo)
        {
            return Error<TurnoDto>(
                "No se pueden reprogramar turnos de un paciente inactivo.",
                TipoErrorTurno.Validacion
            );
        }

        if (turno.Estado !=
            EstadoTurno.Programado)
        {
            return Error<TurnoDto>(
                "Solo se pueden editar turnos programados.",
                TipoErrorTurno.Validacion
            );
        }


        var errorValidacion =
            ValidarDatosTurno(
                dto.FechaHora,
                dto.Modalidad
            );


        if (errorValidacion is not null)
        {
            return Error<TurnoDto>(
                errorValidacion,
                TipoErrorTurno.Validacion
            );
        }


        var nuevaFechaHoraUtc =
            dto.FechaHora.UtcDateTime;


        var horarioOcupado =
            await ExisteColisionAsync(
                nutricionistaId,
                nuevaFechaHoraUtc,
                turno.Id
            );


        if (horarioOcupado)
        {
            return Error<TurnoDto>(
                "Ya existe otro turno programado en ese horario.",
                TipoErrorTurno.Conflicto
            );
        }


        /*
         * Lo guardamos ANTES de modificar
         * la entidad para saber después si
         * realmente hubo una reprogramación.
         */

        var fechaAnterior =
            turno.FechaHora;


        var fechaModificada =
            fechaAnterior !=
            nuevaFechaHoraUtc;


        turno.FechaHora =
            nuevaFechaHoraUtc;

        turno.Modalidad =
            dto.Modalidad;

        turno.Lugar =
            Limpiar(
                dto.Lugar
            );

        turno.LinkReunion =
            Limpiar(
                dto.LinkReunion
            );

        turno.Motivo =
            Limpiar(
                dto.Motivo
            );

        turno.Observaciones =
            Limpiar(
                dto.Observaciones
            );

        turno.FechaActualizacion =
            DateTime.UtcNow;


        await _context
            .SaveChangesAsync();


        /*
         * Solamente usamos TurnoReprogramado
         * cuando realmente cambió la fecha/hora.
         *
         * Editar una observación no genera
         * una notificación innecesaria.
         */

        if (fechaModificada)
        {
            await IntentarCrearNotificacionAsync(
                turno.PacienteId,
                TipoNotificacion
                    .TurnoReprogramado,
                "Consulta reprogramada",
                "Tu nutricionista modificó la fecha u hora de una consulta. Revisá tu agenda para ver la nueva programación.",
                "Turno",
                turno.Id
            );
        }


        return Exito(
            Mapear(
                turno,
                turno.Paciente.Nombre,
                turno.Paciente.Apellido
            )
        );
    }


    // ==========================================
    // NUTRICIONISTA - CANCELAR
    // ==========================================

    public async Task<
        ResultadoTurno<TurnoDto>>
        CancelarAsync(
            int nutricionistaId,
            int pacienteId,
            int turnoId)
    {
        var turno =
            await _context.Turnos
                .Include(t =>
                    t.Paciente
                )
                .FirstOrDefaultAsync(t =>
                    t.Id == turnoId
                    &&
                    t.PacienteId ==
                    pacienteId
                    &&
                    t.Paciente
                        .NutricionistaId ==
                    nutricionistaId
                );


        if (turno is null)
        {
            return Error<TurnoDto>(
                "Turno no encontrado.",
                TipoErrorTurno.NoEncontrado
            );
        }


        if (turno.Estado ==
            EstadoTurno.Cancelado)
        {
            return Error<TurnoDto>(
                "El turno ya se encuentra cancelado.",
                TipoErrorTurno.Validacion
            );
        }


        if (turno.Estado ==
            EstadoTurno.Realizado)
        {
            return Error<TurnoDto>(
                "No se puede cancelar un turno que ya fue realizado.",
                TipoErrorTurno.Validacion
            );
        }


        turno.Estado =
            EstadoTurno.Cancelado;

        turno.FechaActualizacion =
            DateTime.UtcNow;


        await _context
            .SaveChangesAsync();


        await IntentarCrearNotificacionAsync(
            turno.PacienteId,
            TipoNotificacion
                .TurnoCancelado,
            "Consulta cancelada",
            "Tu nutricionista canceló una consulta programada. Podés consultar tu agenda para ver los detalles.",
            "Turno",
            turno.Id
        );


        return Exito(
            Mapear(
                turno,
                turno.Paciente.Nombre,
                turno.Paciente.Apellido
            )
        );
    }


    // ==========================================
    // NUTRICIONISTA - MARCAR REALIZADO
    // ==========================================

    public async Task<
        ResultadoTurno<TurnoDto>>
        MarcarRealizadoAsync(
            int nutricionistaId,
            int pacienteId,
            int turnoId)
    {
        var turno =
            await _context.Turnos
                .Include(t =>
                    t.Paciente
                )
                .FirstOrDefaultAsync(t =>
                    t.Id == turnoId
                    &&
                    t.PacienteId ==
                    pacienteId
                    &&
                    t.Paciente
                        .NutricionistaId ==
                    nutricionistaId
                );


        if (turno is null)
        {
            return Error<TurnoDto>(
                "Turno no encontrado.",
                TipoErrorTurno.NoEncontrado
            );
        }


        if (turno.Estado ==
            EstadoTurno.Realizado)
        {
            return Error<TurnoDto>(
                "El turno ya fue marcado como realizado.",
                TipoErrorTurno.Validacion
            );
        }


        if (turno.Estado ==
            EstadoTurno.Cancelado)
        {
            return Error<TurnoDto>(
                "No se puede marcar como realizado un turno cancelado.",
                TipoErrorTurno.Validacion
            );
        }


        /*
         * Evitamos marcar accidentalmente como
         * realizada una consulta futura.
         */

        if (turno.FechaHora >
            DateTime.UtcNow)
        {
            return Error<TurnoDto>(
                "No se puede marcar como realizado un turno que todavía no ocurrió.",
                TipoErrorTurno.Validacion
            );
        }


        turno.Estado =
            EstadoTurno.Realizado;

        turno.FechaActualizacion =
            DateTime.UtcNow;


        await _context
            .SaveChangesAsync();


        /*
         * No generamos notificación:
         *
         * "Tu turno fue realizado"
         *
         * porque no aporta valor al paciente.
         */


        return Exito(
            Mapear(
                turno,
                turno.Paciente.Nombre,
                turno.Paciente.Apellido
            )
        );
    }


    // ==========================================
    // NUTRICIONISTA - PRÓXIMOS TURNOS
    // ==========================================

    public async Task<
        ResultadoTurno<List<TurnoDto>>>
        ObtenerProximosNutricionistaAsync(
            int nutricionistaId)
    {
        var ahora =
            DateTime.UtcNow;


        var turnos =
            await _context.Turnos
                .AsNoTracking()
                .Include(t =>
                    t.Paciente
                )
                .Where(t =>
                    t.Paciente
                        .NutricionistaId ==
                    nutricionistaId
                    &&
                    t.Estado ==
                    EstadoTurno.Programado
                    &&
                    t.FechaHora >=
                    ahora
                )
                .OrderBy(t =>
                    t.FechaHora
                )
                .ThenBy(t =>
                    t.Id
                )
                .ToListAsync();


        return Exito(
            turnos
                .Select(t =>
                    Mapear(
                        t,
                        t.Paciente.Nombre,
                        t.Paciente.Apellido
                    )
                )
                .ToList()
        );
    }


    // ==========================================
    // NUTRICIONISTA - HISTORIAL PACIENTE
    // ==========================================

    public async Task<
        ResultadoTurno<List<TurnoDto>>>
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
                List<TurnoDto>>(
                "Paciente no encontrado.",
                TipoErrorTurno.NoEncontrado
            );
        }


        var turnos =
            await _context.Turnos
                .AsNoTracking()
                .Include(t =>
                    t.Paciente
                )
                .Where(t =>
                    t.PacienteId ==
                    pacienteId
                )
                .OrderByDescending(t =>
                    t.FechaHora
                )
                .ThenByDescending(t =>
                    t.Id
                )
                .ToListAsync();


        return Exito(
            turnos
                .Select(t =>
                    Mapear(
                        t,
                        t.Paciente.Nombre,
                        t.Paciente.Apellido
                    )
                )
                .ToList()
        );
    }


    // ==========================================
    // NUTRICIONISTA - DETALLE
    // ==========================================

    public async Task<
        ResultadoTurno<TurnoDto>>
        ObtenerDetalleAsync(
            int nutricionistaId,
            int pacienteId,
            int turnoId)
    {
        var turno =
            await _context.Turnos
                .AsNoTracking()
                .Include(t =>
                    t.Paciente
                )
                .FirstOrDefaultAsync(t =>
                    t.Id ==
                    turnoId
                    &&
                    t.PacienteId ==
                    pacienteId
                    &&
                    t.Paciente
                        .NutricionistaId ==
                    nutricionistaId
                );


        if (turno is null)
        {
            return Error<TurnoDto>(
                "Turno no encontrado.",
                TipoErrorTurno.NoEncontrado
            );
        }


        return Exito(
            Mapear(
                turno,
                turno.Paciente.Nombre,
                turno.Paciente.Apellido
            )
        );
    }


    // ==========================================
    // PACIENTE - MIS TURNOS
    // ==========================================

    public async Task<
        ResultadoTurno<List<TurnoDto>>>
        ObtenerPropiosAsync(
            int pacienteId)
    {
        var pacienteExiste =
            await _context.Pacientes
                .AsNoTracking()
                .AnyAsync(p =>
                    p.Id ==
                    pacienteId
                );


        if (!pacienteExiste)
        {
            return Error<
                List<TurnoDto>>(
                "Paciente no encontrado.",
                TipoErrorTurno.NoEncontrado
            );
        }


        var turnos =
            await _context.Turnos
                .AsNoTracking()
                .Include(t =>
                    t.Paciente
                )
                .Where(t =>
                    t.PacienteId ==
                    pacienteId
                )
                .OrderByDescending(t =>
                    t.FechaHora
                )
                .ThenByDescending(t =>
                    t.Id
                )
                .ToListAsync();


        return Exito(
            turnos
                .Select(t =>
                    Mapear(
                        t,
                        t.Paciente.Nombre,
                        t.Paciente.Apellido
                    )
                )
                .ToList()
        );
    }


    // ==========================================
    // PACIENTE - PRÓXIMO TURNO
    // ==========================================

    public async Task<
        ResultadoTurno<ProximoTurnoDto?>>
        ObtenerProximoPacienteAsync(
            int pacienteId)
    {
        var pacienteExiste =
            await _context.Pacientes
                .AsNoTracking()
                .AnyAsync(p =>
                    p.Id ==
                    pacienteId
                );


        if (!pacienteExiste)
        {
            return Error<
                ProximoTurnoDto?>(
                "Paciente no encontrado.",
                TipoErrorTurno.NoEncontrado
            );
        }


        var ahora =
            DateTime.UtcNow;


        var turno =
            await _context.Turnos
                .AsNoTracking()
                .Where(t =>
                    t.PacienteId ==
                    pacienteId
                    &&
                    t.Estado ==
                    EstadoTurno.Programado
                    &&
                    t.FechaHora >=
                    ahora
                )
                .OrderBy(t =>
                    t.FechaHora
                )
                .ThenBy(t =>
                    t.Id
                )
                .FirstOrDefaultAsync();


        if (turno is null)
        {
            /*
             * No tener próximos turnos NO es error.
             */

            return Exito<
                ProximoTurnoDto?>(
                null
            );
        }


        var dto =
            new ProximoTurnoDto
            {
                Id =
                    turno.Id,

                FechaHora =
                    turno.FechaHora,

                Modalidad =
                    turno.Modalidad
                        .ToString(),

                Lugar =
                    turno.Lugar,

                LinkReunion =
                    turno.LinkReunion,

                Motivo =
                    turno.Motivo
            };


        return Exito<
            ProximoTurnoDto?>(
                dto
            );
    }


    // ==========================================
    // COLISIÓN DE AGENDA
    // ==========================================

    private async Task<bool>
        ExisteColisionAsync(
            int nutricionistaId,
            DateTime fechaHoraUtc,
            int? turnoExcluirId = null)
    {
        return await _context.Turnos
            .AsNoTracking()
            .AnyAsync(t =>
                t.Paciente
                    .NutricionistaId ==
                nutricionistaId
                &&
                t.Estado ==
                EstadoTurno.Programado
                &&
                t.FechaHora ==
                fechaHoraUtc
                &&
                (
                    !turnoExcluirId.HasValue
                    ||
                    t.Id !=
                    turnoExcluirId.Value
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
    // VALIDACIÓN
    // ==========================================

    private static string?
        ValidarDatosTurno(
            DateTimeOffset fechaHora,
            ModalidadTurno modalidad)
    {
        if (!Enum.IsDefined(
            modalidad))
        {
            return
                "La modalidad del turno no es válida.";
        }


        var fechaUtc =
            fechaHora.UtcDateTime;


        if (fechaUtc <=
            DateTime.UtcNow)
        {
            return
                "La fecha y hora del turno deben ser futuras.";
        }


        return null;
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
            string recursoTipo,
            int recursoId)
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
             * La agenda ya fue modificada.
             *
             * Una falla secundaria al crear una
             * notificación no debe transformar
             * una operación exitosa en un 500.
             */

            _logger.LogError(
                ex,
                "No se pudo crear la notificación {TipoNotificacion} del turno {TurnoId} para el usuario {UsuarioId}.",
                tipo,
                recursoId,
                usuarioId
            );
        }
    }


    // ==========================================
    // MAPPER
    // ==========================================

    private static TurnoDto
        Mapear(
            Turno turno,
            string? nombre,
            string? apellido)
    {
        return new TurnoDto
        {
            Id =
                turno.Id,

            PacienteId =
                turno.PacienteId,

            NombrePaciente =
                ObtenerNombrePaciente(
                    nombre,
                    apellido
                ),

            FechaHora =
                turno.FechaHora,

            Modalidad =
                turno.Modalidad
                    .ToString(),

            Estado =
                turno.Estado
                    .ToString(),

            Lugar =
                turno.Lugar,

            LinkReunion =
                turno.LinkReunion,

            Motivo =
                turno.Motivo,

            Observaciones =
                turno.Observaciones,

            FechaCreacion =
                turno.FechaCreacion,

            FechaActualizacion =
                turno.FechaActualizacion
        };
    }


    // ==========================================
    // NOMBRE PACIENTE
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
            ? "Paciente"
            : nombreCompleto;
    }


    // ==========================================
    // LIMPIEZA
    // ==========================================

    private static string?
        Limpiar(
            string? valor)
    {
        return string.IsNullOrWhiteSpace(
            valor)
            ? null
            : valor.Trim();
    }


    // ==========================================
    // RESULTADOS
    // ==========================================

    private static ResultadoTurno<T>
        Exito<T>(
            T datos)
    {
        return new ResultadoTurno<T>
        {
            Exitoso =
                true,

            Datos =
                datos,

            TipoError =
                TipoErrorTurno.Ninguno
        };
    }


    private static ResultadoTurno<T>
        Error<T>(
            string mensaje,
            TipoErrorTurno tipo)
    {
        return new ResultadoTurno<T>
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