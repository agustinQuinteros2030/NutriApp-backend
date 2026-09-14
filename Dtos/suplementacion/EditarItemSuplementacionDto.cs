using System.ComponentModel.DataAnnotations;

public class EditarItemSuplementacionDto
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


    [MaxLength(50)]
    public string? Unidad { get; set; }


    [MaxLength(150)]
    public string? Momento { get; set; }


    [MaxLength(1000)]
    public string? Indicaciones { get; set; }


    [Range(1, 100)]
    public int Orden { get; set; }
}