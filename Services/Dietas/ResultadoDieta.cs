namespace NutriApi.Services.Dietas;

public class ResultadoDieta<T>
{
    public bool Exitoso { get; set; }

    public T? Datos { get; set; }

    public string? Error { get; set; }

    public TipoErrorDieta TipoError { get; set; }
}

public enum TipoErrorDieta
{
    Ninguno = 0,
    Validacion = 1,
    NoEncontrado = 2,
    ErrorInterno = 3
}