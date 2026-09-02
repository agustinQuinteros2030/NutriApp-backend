namespace NutriApi.Services.Alimentos;

public class ResultadoAlimento<T>
{
    public bool Exitoso { get; set; }

    public T? Datos { get; set; }

    public string? Error { get; set; }

    public TipoErrorAlimento TipoError { get; set; }
}

public enum TipoErrorAlimento
{
    Ninguno = 0,

    Validacion = 1,

    NoEncontrado = 2,

    ErrorInterno = 3
}