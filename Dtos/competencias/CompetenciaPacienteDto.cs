

namespace NutriApi.DTOs.Competencias;

public class CompetenciaPacienteDto
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string Disciplina { get; set; } = string.Empty;

    public DateOnly FechaInicio { get; set; }

    public DateOnly? FechaFin { get; set; }

    public string? Lugar { get; set; }

    public EstadoCompetencia Estado { get; set; }
}