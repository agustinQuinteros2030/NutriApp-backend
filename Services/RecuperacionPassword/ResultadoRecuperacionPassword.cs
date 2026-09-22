namespace NutriApi.Services.RecuperacionPassword;

public class ResultadoRecuperacionPassword<T>
{
    public bool Exitoso { get; set; }

    public T? Datos { get; set; }

    public string? Error { get; set; }

    public TipoErrorRecuperacionPassword
        TipoError
    { get; set; }
            = TipoErrorRecuperacionPassword.Ninguno;
}