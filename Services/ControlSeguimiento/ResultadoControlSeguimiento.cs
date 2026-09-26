namespace NutriApi.Services.ControlSeguimiento;

public enum TipoErrorControlSeguimiento
{
    Ninguno = 0,

    Validacion = 1,

    NoEncontrado = 2,

    Conflicto = 3,
}

public class ResultadoControlSeguimiento<T>
{
    public bool Exitoso { get; set; }

    public T? Datos { get; set; }

    public string? Error { get; set; }

    public TipoErrorControlSeguimiento TipoError { get; set; }
}
