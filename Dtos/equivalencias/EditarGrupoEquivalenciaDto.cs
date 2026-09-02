using System.ComponentModel.DataAnnotations;

namespace NutriApi.DTOs.Equivalencias;

public class EditarGrupoEquivalenciaDto
{
    [Required]
    [MaxLength(150)]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Descripcion { get; set; }
}