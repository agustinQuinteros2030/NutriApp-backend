using System.ComponentModel.DataAnnotations;

namespace NutriApi.DTOs.Dietas;

public class GuardarSuplementacionDietaDto
{
    [MaxLength(
        1000,
        ErrorMessage =
            "Las observaciones generales no pueden superar los 1000 caracteres."
    )]
    public string? ObservacionesGenerales { get; set; }
}