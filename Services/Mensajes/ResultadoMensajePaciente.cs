namespace NutriApi.Services.Mensajes;

public enum TipoErrorMensajePaciente
{
    Ninguno = 0,

    Validacion = 1,

    NoEncontrado = 2,

    Conflicto = 3,
}

public class ResultadoMensajePaciente<T>
{
    public bool Exitoso { get; set; }

    public T? Datos { get; set; }

    public string? Error { get; set; }

    public TipoErrorMensajePaciente TipoError { get; set; }
}
