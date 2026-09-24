using System.ComponentModel.DataAnnotations;
using NutriApp.Enums.Alimentos;

public class CrearGrupoEquivalenciaDto
{
    [Required]
    [MaxLength(150)]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Descripcion { get; set; }

    public CriterioEquivalencia Criterio { get; set; }
}