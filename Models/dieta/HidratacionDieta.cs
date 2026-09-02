namespace NutriApp.Models.Dietas;

public class HidratacionDieta
{
    public int Id { get; set; }

    public int DietaId { get; set; }

    public int? MililitrosDiarios { get; set; }

    public int? VasosDiarios { get; set; }

    public string? Observaciones { get; set; }


    // Navegación

    public Dieta Dieta { get; set; } = null!;
}