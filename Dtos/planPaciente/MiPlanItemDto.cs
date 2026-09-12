namespace NutriApi.DTOs.PlanPaciente;

public class MiPlanItemDto
{
    public int Id { get; set; }

    public int AlimentoId { get; set; }

    public string Alimento { get; set; } =
        string.Empty;

    public decimal Cantidad { get; set; }

    public string UnidadMedida { get; set; } =
        string.Empty;

    public string? Indicaciones { get; set; }

    public int Orden { get; set; }

    public PlanNutricionDto? Nutricion { get; set; }

    public List<MiPlanAlternativaDto> Alternativas { get; set; }
        = new();
}