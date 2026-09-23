namespace NutriApi.Services.Turnos;

public class ResultadoTurno<T>
{
    public bool Exitoso { get; set; }

    public T? Datos { get; set; }

    public string? Error { get; set; }

    public TipoErrorTurno TipoError { get; set; } =
        TipoErrorTurno.Ninguno;
}