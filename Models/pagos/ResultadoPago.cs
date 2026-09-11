namespace NutriApi.Services.Pagos;

public class ResultadoPago<T>
{
    public bool Exitoso { get; set; }

    public T? Datos { get; set; }

    public string? Error { get; set; }

    public TipoErrorPago TipoError { get; set; }
        = TipoErrorPago.Ninguno;
}