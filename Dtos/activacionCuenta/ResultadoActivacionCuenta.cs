namespace NutriApi.Services.ActivacionCuenta;

public class ResultadoActivacionCuenta<T>
{
    public bool Exitoso { get; set; }

    public T? Datos { get; set; }

    public string? Error { get; set; }

    public TipoErrorActivacionCuenta TipoError { get; set; }
        = TipoErrorActivacionCuenta.Ninguno;
}