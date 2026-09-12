namespace NutriApi.DTOs.PlanPaciente;

public class MiPlanAlternativaDto
{
    public int Id { get; set; }

    public int AlimentoId { get; set; }

    public string Alimento { get; set; } =
        string.Empty;

    public int GrupoEquivalenciaId { get; set; }

    public string GrupoEquivalencia { get; set; } =
        string.Empty;

    public decimal CantidadCalculada { get; set; }

    public string UnidadMedida { get; set; } =
        string.Empty;

    public PlanNutricionDto? Nutricion { get; set; }
}