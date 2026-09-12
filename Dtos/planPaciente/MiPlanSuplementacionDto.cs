namespace NutriApi.DTOs.PlanPaciente;

public class MiPlanSuplementacionDto
{
    public string? ObservacionesGenerales { get; set; }

    public List<MiPlanSuplementoDto> Items { get; set; }
        = new();
}


public class MiPlanSuplementoDto
{
    public int Id { get; set; }

    public string Nombre { get; set; } =
        string.Empty;

    public decimal? Cantidad { get; set; }

    public string? Unidad { get; set; }

    public string? Momento { get; set; }

    public string? Indicaciones { get; set; }

    public int Orden { get; set; }
}