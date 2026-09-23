using System.ComponentModel.DataAnnotations;

namespace NutriApi.DTOs.RegistroDiario;

public class GuardarRegistroDiarioDto
{
    [Required]
    public DateOnly Fecha { get; set; }


    // ==========================================
    // CHECK-IN
    // ==========================================

    public bool? CumplioPlan { get; set; }


    // ==========================================
    // MEDICIONES OPCIONALES
    // ==========================================

    public decimal? CinturaCm { get; set; }

    public decimal? CaderaCm { get; set; }

    public decimal? GemeloCm { get; set; }

    public decimal? CuelloCm { get; set; }


    // ==========================================
    // OBSERVACIONES
    // ==========================================

    [MaxLength(
        1000,
        ErrorMessage =
            "Las observaciones no pueden superar los 1000 caracteres."
    )]
    public string? Observaciones { get; set; }
}