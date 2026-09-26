using NutriApi.DTOs.PlanPaciente;

namespace NutriApi.DTOs.Pdf;

public class DietaPdfDatos
{
    public string NombrePaciente { get; set; } = string.Empty;

    public string NombreNutricionista { get; set; } = string.Empty;

    public MiPlanDto Plan { get; set; } = null!;
}
