namespace NutriApi.DTOs.Dietas;

public class DietaDetalleDto
{
    public int Id { get; set; }

    public int PacienteId { get; set; }

    public string Nombre { get; set; } =
        string.Empty;

    public string? Descripcion { get; set; }

    public int Version { get; set; }

    public string Estado { get; set; } =
        string.Empty;

    public DateOnly FechaInicio { get; set; }

    public DateOnly? FechaFin { get; set; }

    public string? ObservacionesGenerales { get; set; }

    public int CantidadComidas { get; set; }

    public DateTime FechaCreacion { get; set; }

    public DateTime? FechaActualizacion { get; set; }


    public TotalesNutricionalesDto Totales { get; set; }
        = new();
}