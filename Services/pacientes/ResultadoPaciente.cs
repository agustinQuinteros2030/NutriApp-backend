namespace NutriApi.Services.Pacientes;

public class ResultadoPaciente<T>
{
    public bool Exitoso { get; set; }

    public T? Datos { get; set; }

    public string? Error { get; set; }

    public TipoErrorPaciente TipoError { get; set; }
}

public enum TipoErrorPaciente
{
    Ninguno = 0,
    Validacion = 1,
    NoEncontrado = 2,
    ErrorInterno = 3
}