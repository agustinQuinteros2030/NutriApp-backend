namespace NutriApi.DTOs.Competencias;

public class ResultadoCompetencia<T>
{
    public bool Exitoso { get; set; }

    public T? Datos { get; set; }

    public string? Error { get; set; }

    public TipoErrorCompetencia TipoError { get; set; } = TipoErrorCompetencia.Ninguno;
}
