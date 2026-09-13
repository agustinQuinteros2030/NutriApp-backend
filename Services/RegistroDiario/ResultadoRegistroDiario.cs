namespace NutriApi.Services.RegistroDiario;

public class ResultadoRegistroDiario<T>
{
    public bool Exitoso { get; set; }

    public T? Datos { get; set; }

    public string? Error { get; set; }

    public TipoErrorRegistroDiario TipoError { get; set; }
        = TipoErrorRegistroDiario.Ninguno;
}