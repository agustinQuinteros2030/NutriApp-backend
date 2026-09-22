namespace NutriApi.Services.RecuperacionPassword;

public enum TipoErrorRecuperacionPassword
{
    Ninguno = 0,

    Validacion = 1,

    TokenInvalido = 2,

    Interno = 3,
    NoEncontrado = 4

}