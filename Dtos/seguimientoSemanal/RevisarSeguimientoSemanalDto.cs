using System.ComponentModel.DataAnnotations;

namespace NutriApi.DTOs.SeguimientoSemanal;

public class RevisarSeguimientoSemanalDto
{
    [Required(
        ErrorMessage =
            "La revisión es obligatoria."
    )]
    [MaxLength(
        3000,
        ErrorMessage =
            "La revisión no puede superar los 3000 caracteres."
    )]
    public string Revision { get; set; } =
        string.Empty;
}