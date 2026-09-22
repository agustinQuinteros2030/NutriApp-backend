using System.ComponentModel.DataAnnotations;

namespace NutriApi.DTOs.RecuperacionPassword;

public class RestablecerPasswordDto
{
    [Required]
    [EmailAddress(
        ErrorMessage =
            "El formato del email no es válido."
    )]
    public string Email { get; set; } =
        string.Empty;


    [Required(
        ErrorMessage =
            "El token es obligatorio."
    )]
    public string Token { get; set; } =
        string.Empty;


    [Required(
        ErrorMessage =
            "La contraseña es obligatoria."
    )]
    [MinLength(
        8,
        ErrorMessage =
            "La contraseña debe tener al menos 8 caracteres."
    )]
    public string Password { get; set; } =
        string.Empty;


    [Required(
        ErrorMessage =
            "Debés confirmar la contraseña."
    )]
    [Compare(
        nameof(Password),
        ErrorMessage =
            "Las contraseñas no coinciden."
    )]
    public string ConfirmarPassword { get; set; } =
        string.Empty;
}