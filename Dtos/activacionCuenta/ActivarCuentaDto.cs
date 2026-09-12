using System.ComponentModel.DataAnnotations;

namespace NutriApi.DTOs.ActivacionCuenta;

public class ActivarCuentaDto
{
    [Required(
        ErrorMessage =
            "El email es obligatorio."
    )]
    [EmailAddress(
        ErrorMessage =
            "El email no es válido."
    )]
    public string Email { get; set; } =
        string.Empty;


    [Required(
        ErrorMessage =
            "El token de activación es obligatorio."
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