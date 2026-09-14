using System.ComponentModel.DataAnnotations;

namespace NutriApi.DTOs.Pagos;

public class CrearPagoPacienteDto
{
    public DateOnly FechaPago { get; set; }

    public DateOnly ProximoVencimiento { get; set; }


    [Range(
        typeof(decimal),
        "0.01",
        "999999999999",
        ParseLimitsInInvariantCulture = true,
        ConvertValueInInvariantCulture = true,
        ErrorMessage =
            "El monto debe ser mayor a cero."
    )]
    public decimal? Monto { get; set; }


    [MaxLength(100)]
    public string? MetodoPago { get; set; }


    [MaxLength(1000)]
    public string? Observaciones { get; set; }
}