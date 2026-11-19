using System.ComponentModel.DataAnnotations;

namespace NutriApi.DTOs.Competencias;

public class CrearCompetenciaDto
{
    [Required]
    [MaxLength(150)]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Disciplina { get; set; } = string.Empty;

    public DateOnly FechaInicio { get; set; }

    public DateOnly? FechaFin { get; set; }

    [MaxLength(200)]
    public string? Lugar { get; set; }

    [MaxLength(1000)]
    public string? Observaciones { get; set; }
}
