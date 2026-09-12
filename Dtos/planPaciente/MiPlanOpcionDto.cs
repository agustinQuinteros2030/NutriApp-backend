namespace NutriApi.DTOs.PlanPaciente;

public class MiPlanOpcionDto
{
    public int Id { get; set; }

    public string Nombre { get; set; } =
        string.Empty;

    public int Orden { get; set; }

    public bool EsPredeterminada { get; set; }

    public string? Observaciones { get; set; }

    public PlanTotalesNutricionalesDto Totales { get; set; }
        = new();

    public List<MiPlanItemDto> Items { get; set; }
        = new();
}