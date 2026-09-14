using System.ComponentModel.DataAnnotations;

using NutriApp.Enums.Seguimiento;

namespace NutriApi.DTOs.SeguimientoSemanal;

public class CrearSeguimientoSemanalDto
{
    [Range(
        typeof(decimal),
        "0.01",
        "999.99",
        ParseLimitsInInvariantCulture = true,
        ConvertValueInInvariantCulture = true,
        ErrorMessage =
            "El peso debe ser mayor a cero."
    )]
    public decimal PesoActual { get; set; }


    [Range(
        1,
        10,
        ErrorMessage =
            "La adherencia debe estar entre 1 y 10."
    )]
    public int Adherencia { get; set; }


    public DescansoSemanal Descanso { get; set; }


    public DigestionSemanal Digestiones { get; set; }


    [MaxLength(
        1500,
        ErrorMessage =
            "El detalle de las digestiones no puede superar los 1500 caracteres."
    )]
    public string? DetalleDigestiones { get; set; }


    public RendimientoEntrenamientoSemanal
        RendimientoEntrenamientos
    { get; set; }


    public CumplimientoHidratacionSemanal
        CumplimientoHidratacion
    { get; set; }


    public RegularidadIntestinalSemanal
        RegularidadIntestinal
    { get; set; }


    public bool TuvoMolestiaFisica { get; set; }


    [MaxLength(
        1500,
        ErrorMessage =
            "El detalle de la molestia no puede superar los 1500 caracteres."
    )]
    public string? DetalleMolestiaFisica { get; set; }


    [Range(
        1,
        10,
        ErrorMessage =
            "La satisfacción con la comunicación debe estar entre 1 y 10."
    )]
    public int SatisfaccionComunicacion { get; set; }
}