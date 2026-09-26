using System.ComponentModel.DataAnnotations;
using NutriApp.Enums;

namespace NutriApi.DTOs.Alimentos;

public class EditarAlimentoDto
{
    [Range(1, int.MaxValue)]
    public int CategoriaAlimentoId { get; set; }


    [Required]
    [MaxLength(150)]
    public string Nombre { get; set; } = string.Empty;


    [Range(
        typeof(decimal),
        "0",
        "999999",
        ParseLimitsInInvariantCulture = true,
        ConvertValueInInvariantCulture = true
    )]
    public decimal CantidadBase { get; set; }


    public UnidadMedida UnidadBase { get; set; }


    [Range(
        typeof(decimal),
        "0.01",
        "999999",
        ParseLimitsInInvariantCulture = true,
        ConvertValueInInvariantCulture = true
    )]
    public decimal? Calorias { get; set; }


    [Range(
        typeof(decimal),
        "0.01",
        "999999",
        ParseLimitsInInvariantCulture = true,
        ConvertValueInInvariantCulture = true
    )]
    public decimal? Proteinas { get; set; }


    [Range(
        typeof(decimal),
        "0.01",
        "999999",
        ParseLimitsInInvariantCulture = true,
        ConvertValueInInvariantCulture = true
    )]
    public decimal? Carbohidratos { get; set; }


    [Range(
        typeof(decimal),
        "0",
        "999999",
        ParseLimitsInInvariantCulture = true,
        ConvertValueInInvariantCulture = true
    )]
    public decimal? Grasas { get; set; }
}