using System.ComponentModel.DataAnnotations;

namespace NutriApi.DTOs.Dietas;

public class ActualizarObjetivosNutricionalesDto
{
    [Range(
        typeof(decimal),
        "0",
        "99999999.99",
        ParseLimitsInInvariantCulture = true,
        ConvertValueInInvariantCulture = true,
        ErrorMessage = "Las calorías deben estar entre 0 y 99999999.99."
    )]
    public decimal? ObjetivoCalorias { get; set; }

    [Range(
        typeof(decimal),
        "0",
        "99999999.99",
        ParseLimitsInInvariantCulture = true,
        ConvertValueInInvariantCulture = true,
        ErrorMessage = "Las proteínas deben estar entre 0 y 99999999.99."
    )]
    public decimal? ObjetivoProteinas { get; set; }

    [Range(
        typeof(decimal),
        "0",
        "99999999.99",
        ParseLimitsInInvariantCulture = true,
        ConvertValueInInvariantCulture = true,
        ErrorMessage = "Los carbohidratos deben estar entre 0 y 99999999.99."
    )]
    public decimal? ObjetivoCarbohidratos { get; set; }

    [Range(
        typeof(decimal),
        "0",
        "99999999.99",
        ParseLimitsInInvariantCulture = true,
        ConvertValueInInvariantCulture = true,
        ErrorMessage = "Las grasas deben estar entre 0 y 99999999.99."
    )]
    public decimal? ObjetivoGrasas { get; set; }
}
