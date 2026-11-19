using Microsoft.EntityFrameworkCore;
using NutriApi.DTOs.Calendario;
using NutriApp.Data;

namespace NutriApi.Services.Calendario;

public class CalendarioService : ICalendarioService
{
    private readonly NutriAppDbContext _context;

    private static readonly TimeZoneInfo ZonaArgentina = ObtenerZonaArgentina();

    public CalendarioService(NutriAppDbContext context)
    {
        _context = context;
    }

    public async Task<List<EventoCalendarioDto>> ObtenerEventosAsync(
        int nutricionistaId,
        DateOnly desde,
        DateOnly hasta
    )
    {
        // ======================================
        // RANGO LOCAL ARGENTINA A UTC
        // ======================================

        var inicioLocal = desde.ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified);

        var finLocalExclusivo = hasta
            .AddDays(1)
            .ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified);

        var inicioUtc = TimeZoneInfo.ConvertTimeToUtc(inicioLocal, ZonaArgentina);

        var finUtcExclusivo = TimeZoneInfo.ConvertTimeToUtc(finLocalExclusivo, ZonaArgentina);

        // ======================================
        // TURNOS DEL NUTRICIONISTA
        // ======================================

        var turnos = await _context
            .Turnos.AsNoTracking()
            .Where(t =>
                t.Paciente.NutricionistaId == nutricionistaId
                && t.FechaHora >= inicioUtc
                && t.FechaHora < finUtcExclusivo
            )
            .Select(t => new
            {
                t.Id,
                t.PacienteId,
                NombrePaciente = t.Paciente.Nombre,
                ApellidoPaciente = t.Paciente.Apellido,
                AvatarId = t.Paciente.AvatarId,
                t.FechaHora,
                t.Modalidad,
                t.Estado,
                t.Lugar,
                t.Motivo,
            })
            .ToListAsync();

        // ======================================
        // COMPETENCIAS DEL NUTRICIONISTA
        // ======================================

        var competencias = await _context
            .Competencias.AsNoTracking()
            .Where(c =>
                c.Paciente.NutricionistaId == nutricionistaId
                && c.FechaInicio <= hasta
                && (c.FechaFin ?? c.FechaInicio) >= desde
            )
            .Select(c => new
            {
                c.Id,
                c.PacienteId,
                NombrePaciente = c.Paciente.Nombre,
                ApellidoPaciente = c.Paciente.Apellido,
                AvatarId = c.Paciente.AvatarId,
                c.Nombre,
                c.Disciplina,
                c.FechaInicio,
                c.FechaFin,
                c.Lugar,
                c.Estado,
            })
            .ToListAsync();

        // ======================================
        // UNIFICAR EVENTOS
        // ======================================

        var eventos = new List<EventoCalendarioDto>(turnos.Count + competencias.Count);

        foreach (var turno in turnos)
        {
            eventos.Add(
                new EventoCalendarioDto
                {
                    Id = turno.Id,

                    TipoEvento = "Turno",

                    PacienteId = turno.PacienteId,

                    PacienteNombre = ObtenerNombre(turno.NombrePaciente, turno.ApellidoPaciente),

                    PacienteAvatarId = turno.AvatarId,

                    Titulo = string.IsNullOrWhiteSpace(turno.Motivo)
                        ? "Consulta nutricional"
                        : turno.Motivo,

                    FechaHora = turno.FechaHora,

                    Estado = turno.Estado.ToString(),

                    Modalidad = turno.Modalidad.ToString(),

                    Lugar = turno.Lugar,

                    Motivo = turno.Motivo,
                }
            );
        }

        foreach (var competencia in competencias)
        {
            eventos.Add(
                new EventoCalendarioDto
                {
                    Id = competencia.Id,

                    TipoEvento = "Competencia",

                    PacienteId = competencia.PacienteId,

                    PacienteNombre = ObtenerNombre(
                        competencia.NombrePaciente,
                        competencia.ApellidoPaciente
                    ),

                    PacienteAvatarId = competencia.AvatarId,

                    Titulo = competencia.Nombre,

                    FechaInicio = competencia.FechaInicio,

                    FechaFin = competencia.FechaFin,

                    Estado = competencia.Estado.ToString(),

                    Disciplina = competencia.Disciplina,

                    Lugar = competencia.Lugar,
                }
            );
        }

        // ======================================
        // ORDEN CRONOLÓGICO LOCAL
        // ======================================

        return eventos
            .OrderBy(e =>
                e.FechaHora.HasValue
                    ? TimeZoneInfo.ConvertTimeFromUtc(e.FechaHora.Value, ZonaArgentina)
                    : e.FechaInicio!.Value.ToDateTime(TimeOnly.MinValue)
            )
            .ThenBy(e => e.TipoEvento)
            .ThenBy(e => e.Id)
            .ToList();
    }

    private static string ObtenerNombre(string? nombre, string? apellido)
    {
        var nombreCompleto = $"{nombre} {apellido}".Trim();

        return string.IsNullOrWhiteSpace(nombreCompleto) ? "Paciente" : nombreCompleto;
    }

    private static TimeZoneInfo ObtenerZonaArgentina()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("America/Argentina/Buenos_Aires");
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Argentina Standard Time");
        }
    }
}
