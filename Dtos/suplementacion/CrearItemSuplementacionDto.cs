using System.ComponentModel.DataAnnotations;

namespace NutriApi.DTOs.Dietas;

public class CrearItemSuplementacionDto
{
    [Required(
        ErrorMessage =
            "El nombre del suplemento es obligatorio."
    )]
    [MaxLength(
        150,
        ErrorMessage =
            "El nombre no puede superar los 150 caracteres."
    )]
    public string Nombre { get; set; } =
        string.Empty;


    [Range(
         typeof(decimal),
         "0.01",
         "999999999",
         ParseLimitsInInvariantCulture = true,
         ConvertValueInInvariantCulture = true,
         ErrorMessage =
             "La cantidad debe ser mayor a cero."
     )]
    public decimal? Cantidad { get; set; }


    [MaxLength(
        50,
        ErrorMessage =
            "La unidad no puede superar los 50 caracteres."
    )]
    public string? Unidad { get; set; }


    [MaxLength(
        150,
        ErrorMessage =
            "El momento no puede superar los 150 caracteres."
    )]
    public string? Momento { get; set; }


    [MaxLength(
        1000,
        ErrorMessage =
            "Las indicaciones no pueden superar los 1000 caracteres."
    )]
    public string? Indicaciones { get; set; }


    [Range(
        1,
        100,
        ErrorMessage =
            "El orden debe estar entre 1 y 100."
    )]
    public int Orden { get; set; }
}