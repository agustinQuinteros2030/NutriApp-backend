using System.ComponentModel.DataAnnotations;
using NutriApp.Enums;

namespace NutriApi.DTOs.Pacientes;

public class CrearPacienteDto
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


    // PERFIL NUTRICIONAL

    public ObjetivoNutricional ObjetivoNutricional { get; set; }

    public TipoActividad TipoActividad { get; set; }

    public decimal? PesoInicial { get; set; }


    // Altura expresada en metros.
    // Ejemplo: 1.80
    [Range(
        typeof(decimal),
        "0.50",
        "2.50",
        ErrorMessage =
            "La altura debe estar entre 0,50 y 2,50 metros."
    )]

    public decimal? Altura { get; set; }

    public DateOnly? FechaInicio { get; set; }

    [MaxLength(250)]
    public string? ActividadDescripcion { get; set; }

    [MaxLength(2000)]
    public string? ObservacionesGenerales { get; set; }
}