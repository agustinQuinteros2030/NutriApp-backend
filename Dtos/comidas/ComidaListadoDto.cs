namespace NutriApi.DTOs.Dietas;

public class ComidaListadoDto
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string Tipo { get; set; } = string.Empty;

    public int Orden { get; set; }

    public int CantidadSecciones { get; set; }
}