using Microsoft.EntityFrameworkCore;
using NutriApi.Configuracion;
using NutriApi.DTOs.ControlSeguimiento;
using NutriApp.Data;
using NutriApp.Models.ControlSeguimiento;

namespace NutriApi.Services.ControlSeguimiento;

public class ControlSeguimientoService : IControlSeguimientoService
{
    private const int DiasEstadoProximo = 3;

    private readonly NutriAppDbContext _context;

    public ControlSeguimientoService(NutriAppDbContext context)
    {
        _context = context;
    }

    // ==========================================
    // CONFIGURAR
    // ==========================================

    public async Task<ResultadoControlSeguimiento<ControlSeguimientoDto>> ConfigurarAsync(
        int nutricionistaId,
        int pacienteId,
        ConfigurarControlSeguimientoDto dto
    )
    {
        var paciente = await _context
            .Pacientes.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == pacienteId && p.NutricionistaId == nutricionistaId);

        if (paciente is null)
        {
            return Error<ControlSeguimientoDto>(
                "Paciente no encontrado.",
                TipoErrorControlSeguimiento.NoEncontrado
            );
        }

        if (dto.FrecuenciaDias < 1 || dto.FrecuenciaDias > 365)
        {
            return Error<ControlSeguimientoDto>(
                "La frecuencia debe estar entre 1 y 365 días.",
                TipoErrorControlSeguimiento.Validacion
            );
        }

        var hoy = ObtenerFechaLocalActual();

        var control = await _context.ControlesSeguimientoPacientes.FirstOrDefaultAsync(c =>
            c.PacienteId == pacienteId
        );

        /*
         * Si ya existía un último seguimiento
         * y el DTO no manda uno nuevo,
         * lo conservamos.
         */

        var ultimoSeguimiento = dto.UltimoSeguimiento ?? control?.UltimoSeguimiento;

        /*
         * Prioridad:
         *
         * 1. Fecha explícita enviada.
         *
         * 2. Último seguimiento + frecuencia.
         *
         * 3. Hoy + frecuencia.
         */

        var proximoSeguimiento =
            dto.ProximoSeguimiento
            ?? (
                ultimoSeguimiento.HasValue
                    ? ultimoSeguimiento.Value.AddDays(dto.FrecuenciaDias)
                    : hoy.AddDays(dto.FrecuenciaDias)
            );

        // ======================================
        // CREAR
        // ======================================

        if (control is null)
        {
            control = new ControlSeguimientoPaciente
            {
                PacienteId = pacienteId,

                Activo = true,

                FrecuenciaDias = dto.FrecuenciaDias,

                UltimoSeguimiento = ultimoSeguimiento,

                ProximoSeguimiento = proximoSeguimiento,

                FechaCreacion = DateTime.UtcNow,
            };

            _context.ControlesSeguimientoPacientes.Add(control);
        }
        // ======================================
        // ACTUALIZAR
        // ======================================

        else
        {
            control.FrecuenciaDias = dto.FrecuenciaDias;

            /*
             * No borramos accidentalmente
             * el último seguimiento si el
             * frontend no lo envía.
             */

            if (dto.UltimoSeguimiento.HasValue)
            {
                control.UltimoSeguimiento = dto.UltimoSeguimiento;
            }

            control.ProximoSeguimiento = proximoSeguimiento;

            control.FechaActualizacion = DateTime.UtcNow;
        }

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            /*
             * PacienteId tiene índice UNIQUE.
             *
             * Esto cubre dos requests
             * simultáneos intentando crear
             * el control por primera vez.
             */

            return Error<ControlSeguimientoDto>(
                "El control de seguimiento del paciente fue modificado simultáneamente. Volvé a intentarlo.",
                TipoErrorControlSeguimiento.Conflicto
            );
        }

        return Exito(Mapear(control, paciente.Nombre, paciente.Apellido, hoy));
    }

    // ==========================================
    // OBTENER PACIENTE
    // ==========================================

    public async Task<ResultadoControlSeguimiento<ControlSeguimientoDto>> ObtenerPacienteAsync(
        int nutricionistaId,
        int pacienteId
    )
    {
        var paciente = await _context
            .Pacientes.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == pacienteId && p.NutricionistaId == nutricionistaId);

        if (paciente is null)
        {
            return Error<ControlSeguimientoDto>(
                "Paciente no encontrado.",
                TipoErrorControlSeguimiento.NoEncontrado
            );
        }

        var control = await _context
            .ControlesSeguimientoPacientes.AsNoTracking()
            .FirstOrDefaultAsync(c => c.PacienteId == pacienteId);

        if (control is null)
        {
            return Error<ControlSeguimientoDto>(
                "El paciente todavía no tiene configurado el control de seguimiento.",
                TipoErrorControlSeguimiento.NoEncontrado
            );
        }

        return Exito(
            Mapear(control, paciente.Nombre, paciente.Apellido, ObtenerFechaLocalActual())
        );
    }

    // ==========================================
    // MARCAR REALIZADO
    // ==========================================

    public async Task<ResultadoControlSeguimiento<ControlSeguimientoDto>> MarcarRealizadoAsync(
        int nutricionistaId,
        int pacienteId
    )
    {
        var paciente = await _context
            .Pacientes.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == pacienteId && p.NutricionistaId == nutricionistaId);

        if (paciente is null)
        {
            return Error<ControlSeguimientoDto>(
                "Paciente no encontrado.",
                TipoErrorControlSeguimiento.NoEncontrado
            );
        }

        var control = await _context.ControlesSeguimientoPacientes.FirstOrDefaultAsync(c =>
            c.PacienteId == pacienteId
        );

        if (control is null)
        {
            return Error<ControlSeguimientoDto>(
                "El paciente todavía no tiene configurado el control de seguimiento.",
                TipoErrorControlSeguimiento.NoEncontrado
            );
        }

        if (!control.Activo)
        {
            return Error<ControlSeguimientoDto>(
                "El control de seguimiento se encuentra desactivado.",
                TipoErrorControlSeguimiento.Validacion
            );
        }

        var hoy = ObtenerFechaLocalActual();

        /*
         * Acá ocurre la magia principal.
         *
         * El nutricionista confirma:
         *
         * "Ya revisé a Fulano".
         */

        control.UltimoSeguimiento = hoy;

        control.ProximoSeguimiento = hoy.AddDays(control.FrecuenciaDias);

        control.FechaActualizacion = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Exito(Mapear(control, paciente.Nombre, paciente.Apellido, hoy));
    }

    // ==========================================
    // REPROGRAMAR
    // ==========================================

    public async Task<ResultadoControlSeguimiento<ControlSeguimientoDto>> ReprogramarAsync(
        int nutricionistaId,
        int pacienteId,
        ReprogramarControlSeguimientoDto dto
    )
    {
        var paciente = await _context
            .Pacientes.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == pacienteId && p.NutricionistaId == nutricionistaId);

        if (paciente is null)
        {
            return Error<ControlSeguimientoDto>(
                "Paciente no encontrado.",
                TipoErrorControlSeguimiento.NoEncontrado
            );
        }

        var control = await _context.ControlesSeguimientoPacientes.FirstOrDefaultAsync(c =>
            c.PacienteId == pacienteId
        );

        if (control is null)
        {
            return Error<ControlSeguimientoDto>(
                "El paciente todavía no tiene configurado el control de seguimiento.",
                TipoErrorControlSeguimiento.NoEncontrado
            );
        }

        if (!control.Activo)
        {
            return Error<ControlSeguimientoDto>(
                "El control de seguimiento se encuentra desactivado.",
                TipoErrorControlSeguimiento.Validacion
            );
        }

        var hoy = ObtenerFechaLocalActual();

        if (dto.ProximoSeguimiento < hoy)
        {
            return Error<ControlSeguimientoDto>(
                "No se puede reprogramar un seguimiento para una fecha pasada.",
                TipoErrorControlSeguimiento.Validacion
            );
        }

        /*
         * Reprogramar NO significa
         * que el seguimiento fue realizado.
         *
         * Por eso UltimoSeguimiento
         * permanece intacto.
         */

        control.ProximoSeguimiento = dto.ProximoSeguimiento;

        control.FechaActualizacion = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Exito(Mapear(control, paciente.Nombre, paciente.Apellido, hoy));
    }

    // ==========================================
    // ACTIVAR
    // ==========================================

    public async Task<ResultadoControlSeguimiento<ControlSeguimientoDto>> ActivarAsync(
        int nutricionistaId,
        int pacienteId
    )
    {
        return await CambiarEstadoAsync(nutricionistaId, pacienteId, true);
    }

    // ==========================================
    // DESACTIVAR
    // ==========================================

    public async Task<ResultadoControlSeguimiento<ControlSeguimientoDto>> DesactivarAsync(
        int nutricionistaId,
        int pacienteId
    )
    {
        return await CambiarEstadoAsync(nutricionistaId, pacienteId, false);
    }

    // ==========================================
    // LISTADO DEL NUTRICIONISTA
    // ==========================================

    public async Task<ResultadoControlSeguimiento<List<ControlSeguimientoDto>>> ObtenerTodosAsync(
        int nutricionistaId,
        bool incluirInactivos = false
    )
    {
        var hoy = ObtenerFechaLocalActual();

        var query =
            from control in _context.ControlesSeguimientoPacientes.AsNoTracking()

            join paciente in _context.Pacientes.AsNoTracking()
                on control.PacienteId equals paciente.Id

            where
                paciente.NutricionistaId == nutricionistaId && (incluirInactivos || control.Activo)

            orderby control.Activo descending, control.ProximoSeguimiento, paciente.Apellido, paciente.Nombre

            select new
            {
                Control = control,

                paciente.Nombre,

                paciente.Apellido,
            };

        var entidades = await query.ToListAsync();

        var resultado = entidades
            .Select(x => Mapear(x.Control, x.Nombre, x.Apellido, hoy))
            .ToList();

        return Exito(resultado);
    }

    // ==========================================
    // RESUMEN DASHBOARD
    // ==========================================

    public async Task<
        ResultadoControlSeguimiento<ResumenControlSeguimientoDto>
    > ObtenerResumenAsync(int nutricionistaId)
    {
        var hoy = ObtenerFechaLocalActual();

        var fechas = await (
            from control in _context.ControlesSeguimientoPacientes.AsNoTracking()

            join paciente in _context.Pacientes.AsNoTracking()
                on control.PacienteId equals paciente.Id

            where paciente.NutricionistaId == nutricionistaId && control.Activo

            select control.ProximoSeguimiento
        ).ToListAsync();

        var resumen = new ResumenControlSeguimientoDto();

        foreach (var fecha in fechas)
        {
            var dias = fecha.DayNumber - hoy.DayNumber;

            if (dias < 0)
            {
                resumen.Vencidos++;
            }
            else if (dias == 0)
            {
                resumen.ParaHoy++;
            }
            else if (dias <= DiasEstadoProximo)
            {
                resumen.Proximos++;
            }
            else
            {
                resumen.AlDia++;
            }
        }

        return Exito(resumen);
    }

    // ==========================================
    // CAMBIAR ESTADO
    // ==========================================

    private async Task<ResultadoControlSeguimiento<ControlSeguimientoDto>> CambiarEstadoAsync(
        int nutricionistaId,
        int pacienteId,
        bool activo
    )
    {
        var paciente = await _context
            .Pacientes.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == pacienteId && p.NutricionistaId == nutricionistaId);

        if (paciente is null)
        {
            return Error<ControlSeguimientoDto>(
                "Paciente no encontrado.",
                TipoErrorControlSeguimiento.NoEncontrado
            );
        }

        var control = await _context.ControlesSeguimientoPacientes.FirstOrDefaultAsync(c =>
            c.PacienteId == pacienteId
        );

        if (control is null)
        {
            return Error<ControlSeguimientoDto>(
                "El paciente todavía no tiene configurado el control de seguimiento.",
                TipoErrorControlSeguimiento.NoEncontrado
            );
        }

        control.Activo = activo;

        control.FechaActualizacion = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Exito(
            Mapear(control, paciente.Nombre, paciente.Apellido, ObtenerFechaLocalActual())
        );
    }

    // ==========================================
    // MAPPER
    // ==========================================

    private static ControlSeguimientoDto Mapear(
        ControlSeguimientoPaciente control,
        string nombrePaciente,
        string apellidoPaciente,
        DateOnly hoy
    )
    {
        var dias = control.ProximoSeguimiento.DayNumber - hoy.DayNumber;

        return new ControlSeguimientoDto
        {
            Id = control.Id,

            PacienteId = control.PacienteId,

            NombrePaciente = nombrePaciente,

            ApellidoPaciente = apellidoPaciente,

            Activo = control.Activo,

            FrecuenciaDias = control.FrecuenciaDias,

            UltimoSeguimiento = control.UltimoSeguimiento,

            ProximoSeguimiento = control.ProximoSeguimiento,

            Estado = ObtenerEstado(control.Activo, dias),

            DiasHastaSeguimiento = dias,
        };
    }

    // ==========================================
    // ESTADO CALCULADO
    // ==========================================

    private static string ObtenerEstado(bool activo, int dias)
    {
        if (!activo)
        {
            return "Desactivado";
        }

        if (dias < 0)
        {
            return "Vencido";
        }

        if (dias == 0)
        {
            return "Hoy";
        }

        if (dias <= DiasEstadoProximo)
        {
            return "Proximo";
        }

        return "AlDia";
    }

    // ==========================================
    // FECHA LOCAL
    // ==========================================

    private static DateOnly ObtenerFechaLocalActual()
    {
        var zonaHoraria = TimeZoneInfo.FindSystemTimeZoneById(
            ConfiguracionSeguimientoSemanal.ZonaHorariaId
        );

        var fechaLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, zonaHoraria);

        return DateOnly.FromDateTime(fechaLocal);
    }

    // ==========================================
    // RESULTADOS
    // ==========================================

    private static ResultadoControlSeguimiento<T> Exito<T>(T datos)
    {
        return new ResultadoControlSeguimiento<T>
        {
            Exitoso = true,

            Datos = datos,

            TipoError = TipoErrorControlSeguimiento.Ninguno,
        };
    }

    private static ResultadoControlSeguimiento<T> Error<T>(
        string mensaje,
        TipoErrorControlSeguimiento tipo
    )
    {
        return new ResultadoControlSeguimiento<T>
        {
            Exitoso = false,

            Error = mensaje,

            TipoError = tipo,
        };
    }
}
