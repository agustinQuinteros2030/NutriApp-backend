using System.ComponentModel.DataAnnotations;
using NutriApp.Enums;

namespace NutriApi.DTOs.Dietas;

public class CrearSeccionComidaDto
{
    [Required]
    [MaxLength(100)]
    public string Nombre { get; set; } = string.Empty;

    public TipoSeccionComida Tipo { get; set; }

    [Range(1, 100)]
    public int Orden { get; set; }

    [MaxLength(1000)]
    public string? Observaciones { get; set; }
}