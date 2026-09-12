namespace NutriApi.DTOs.PlanPaciente;

public class MiPlanDto
{
    public int Id { get; set; }

    public string Nombre { get; set; } =
        string.Empty;

    public string? Descripcion { get; set; }

    public int Version { get; set; }

    public string Estado { get; set; } =
        string.Empty;

    public DateOnly FechaInicio { get; set; }

    public DateOnly? FechaFin { get; set; }

    public string? ObservacionesGenerales { get; set; }

    public PlanTotalesNutricionalesDto Totales { get; set; }
        = new();

    public List<MiPlanComidaDto> Comidas { get; set; }
        = new();

    public MiPlanHidratacionDto? Hidratacion { get; set; }

    public MiPlanSuplementacionDto? Suplementacion { get; set; }
}