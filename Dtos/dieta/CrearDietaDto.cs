using System.ComponentModel.DataAnnotations;

namespace NutriApi.DTOs.Dietas;

public class CrearDietaDto
{
    [Required]
    [MaxLength(150)]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Descripcion { get; set; }

    public DateOnly FechaInicio { get; set; }

    public DateOnly? FechaFin { get; set; }

    [MaxLength(3000)]
    public string? ObservacionesGenerales { get; set; }
}