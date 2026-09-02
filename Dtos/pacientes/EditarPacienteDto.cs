using System.ComponentModel.DataAnnotations;
using NutriApp.Enums;

namespace NutriApi.DTOs.Pacientes;

public class EditarPacienteDto
{
    [Required]
    [MaxLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Apellido { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(30)]
    public string? Telefono { get; set; }

    public DateOnly? FechaNacimiento { get; set; }


    public ObjetivoNutricional ObjetivoNutricional { get; set; }

    public TipoActividad TipoActividad { get; set; }

    public decimal? PesoInicial { get; set; }

    public decimal? Altura { get; set; }

    public DateOnly? FechaInicio { get; set; }

    [MaxLength(250)]
    public string? ActividadDescripcion { get; set; }

    [MaxLength(2000)]
    public string? ObservacionesGenerales { get; set; }
}