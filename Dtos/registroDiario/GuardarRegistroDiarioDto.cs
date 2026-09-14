using System.ComponentModel.DataAnnotations;

public class GuardarRegistroDiarioDto
{
    [Required]
    public DateOnly Fecha { get; set; }


    [Range(
        0,
        100,
        ErrorMessage =
            "La adherencia debe estar entre 0 y 100."
    )]
    public int? AdherenciaPorcentaje { get; set; }


    [Range(
        1,
        5,
        ErrorMessage =
            "El hambre debe estar entre 1 y 5."
    )]
    public int? Hambre { get; set; }


    [Range(
        1,
        5,
        ErrorMessage =
            "La energía debe estar entre 1 y 5."
    )]
    public int? Energia { get; set; }


    public bool? Entreno { get; set; }


    [MaxLength(
        1000,
        ErrorMessage =
            "Las observaciones no pueden superar los 1000 caracteres."
    )]
    public string? Observaciones { get; set; }
}