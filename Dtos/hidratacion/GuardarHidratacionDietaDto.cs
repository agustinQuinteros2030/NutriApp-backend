using System.ComponentModel.DataAnnotations;

namespace NutriApi.DTOs.Dietas;

public class GuardarHidratacionDietaDto
{
    [Range(
        1,
        20000,
        ErrorMessage =
            "Los mililitros diarios deben estar entre 1 y 20000."
    )]
    public int? MililitrosDiarios { get; set; }


    [Range(
        1,
        100,
        ErrorMessage =
            "Los vasos diarios deben estar entre 1 y 100."
    )]
    public int? VasosDiarios { get; set; }


    [MaxLength(
        1000,
        ErrorMessage =
            "Las observaciones no pueden superar los 1000 caracteres."
    )]
    public string? Observaciones { get; set; }
}