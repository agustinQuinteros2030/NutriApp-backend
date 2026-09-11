namespace NutriApi.DTOs.Dietas;

public class HidratacionDietaDto
{
    public int Id { get; set; }

    public int DietaId { get; set; }

    public int? MililitrosDiarios { get; set; }

    public int? VasosDiarios { get; set; }

    public string? Observaciones { get; set; }
}