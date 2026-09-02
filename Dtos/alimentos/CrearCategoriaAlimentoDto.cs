using System.ComponentModel.DataAnnotations;

namespace NutriApi.DTOs.Alimentos;

public class CrearCategoriaAlimentoDto
{
    [Required]
    [MaxLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Descripcion { get; set; }
}