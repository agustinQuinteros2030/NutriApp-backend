using System.ComponentModel.DataAnnotations;

namespace NutriApi.DTOs.Notas;

public class CrearNotaPacienteDto
{
    [Required(
        ErrorMessage = "El contenido de la nota es obligatorio."
    )]
    [MaxLength(
        3000,
        ErrorMessage = "La nota no puede superar los 3000 caracteres."
    )]
    public string Contenido { get; set; } =
        string.Empty;
}