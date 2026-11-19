namespace NutriApi.DTOs.Competencias;

public class CompetenciaDto
{
    public int Id { get; set; }

    public int PacienteId { get; set; }

    public string PacienteNombre { get; set; } = string.Empty;

    public string Nombre { get; set; } = string.Empty;

    public string Disciplina { get; set; } = string.Empty;

    public DateOnly FechaInicio { get; set; }

    public DateOnly? FechaFin { get; set; }

    public string? Lugar { get; set; }

    public string? Observaciones { get; set; }

    public EstadoCompetencia Estado { get; set; }

    public DateTime FechaCreacion { get; set; }

    public DateTime? FechaActualizacion { get; set; }
}
