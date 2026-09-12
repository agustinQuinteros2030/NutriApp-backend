namespace NutriApi.DTOs.PlanPaciente;

public class MiPlanSeccionDto
{
    public int Id { get; set; }

    public string Nombre { get; set; } =
        string.Empty;

    public string Tipo { get; set; } =
        string.Empty;

    public int Orden { get; set; }

    public string? Observaciones { get; set; }

    public List<MiPlanOpcionDto> Opciones { get; set; }
        = new();
}