namespace NutriApi.Services.Notas;

public class ResultadoNotaPaciente<T>
{
    public bool Exitoso { get; set; }

    public T? Datos { get; set; }

    public string? Error { get; set; }

    public TipoErrorNotaPaciente TipoError { get; set; }
}


public enum TipoErrorNotaPaciente
{
    Ninguno = 0,

    Validacion = 1,

    NoEncontrado = 2,

    ErrorInterno = 3
}