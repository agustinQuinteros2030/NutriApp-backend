using NutriApi.DTOs.Auth;

namespace NutriApi.Services.Auth;

public class ResultadoAuth
{
    public bool Exitoso { get; set; }

    public AuthResponseDto? Datos { get; set; }

    public string? Error { get; set; }

    public TipoErrorAuth TipoError { get; set; }
}

public enum TipoErrorAuth
{
    Ninguno = 0,
    Validacion = 1,
    CredencialesInvalidas = 2,
    UsuarioBloqueado = 3,
    UsuarioDesactivado = 4,
    ErrorInterno = 5
}