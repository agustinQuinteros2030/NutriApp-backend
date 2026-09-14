namespace NutriApi.Services.SeguimientoSemanal;

public class ResultadoSeguimientoSemanal<T>
{
    public bool Exitoso { get; set; }

    public T? Datos { get; set; }

    public string? Error { get; set; }

    public TipoErrorSeguimientoSemanal TipoError { get; set; }
        = TipoErrorSeguimientoSemanal.Ninguno;
}