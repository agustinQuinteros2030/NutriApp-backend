namespace NutriApi.DTOs.Dietas;

public class DietaListadoDto
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public int Version { get; set; }

    public string Estado { get; set; } = string.Empty;

    public DateOnly FechaInicio { get; set; }

    public DateOnly? FechaFin { get; set; }

    public int CantidadComidas { get; set; }

    public DateTime FechaCreacion { get; set; }
}