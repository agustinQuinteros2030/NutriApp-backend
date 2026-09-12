namespace NutriApi.DTOs.PlanPaciente;

public class MiPlanComidaDto
{
    public int Id { get; set; }

    public string Nombre { get; set; } =
        string.Empty;

    public string Tipo { get; set; } =
        string.Empty;

    public int Orden { get; set; }

    public string? Observaciones { get; set; }

    public PlanTotalesNutricionalesDto Totales { get; set; }
        = new();

    public List<MiPlanSeccionDto> Secciones { get; set; }
        = new();
}