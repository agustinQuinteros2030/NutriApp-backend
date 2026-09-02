namespace NutriApi.Services.Equivalencias;

public class ResultadoEquivalencia<T>
{
    public bool Exitoso { get; set; }

    public T? Datos { get; set; }

    public string? Error { get; set; }

    public TipoErrorEquivalencia TipoError { get; set; }
}

public enum TipoErrorEquivalencia
{
    Ninguno = 0,
    Validacion = 1,
    NoEncontrado = 2,
    ErrorInterno = 3
}