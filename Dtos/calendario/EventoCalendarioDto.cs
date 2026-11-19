namespace NutriApi.DTOs.Calendario;

public class EventoCalendarioDto
{
    public int Id { get; set; }

    public string TipoEvento { get; set; } = string.Empty;

    public int PacienteId { get; set; }

    public string PacienteNombre { get; set; } = string.Empty;

    public string? PacienteAvatarId { get; set; }

    public string Titulo { get; set; } = string.Empty;

    public string Estado { get; set; } = string.Empty;

    // Turnos: fecha y hora UTC
    public DateTime? FechaHora { get; set; }

    // Competencias: fechas sin hora
    public DateOnly? FechaInicio { get; set; }

    public DateOnly? FechaFin { get; set; }

    public string? Disciplina { get; set; }

    public string? Modalidad { get; set; }

    public string? Lugar { get; set; }

    public string? Motivo { get; set; }
}
