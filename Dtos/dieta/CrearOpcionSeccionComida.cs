using System.ComponentModel.DataAnnotations;

namespace NutriApi.DTOs.Dietas;

public class CrearOpcionSeccionComidaDto
{
    [Required]
    [MaxLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [Range(1, 100)]
    public int Orden { get; set; }

    public bool EsPredeterminada { get; set; }

    [MaxLength(1000)]
    public string? Observaciones { get; set; }
}