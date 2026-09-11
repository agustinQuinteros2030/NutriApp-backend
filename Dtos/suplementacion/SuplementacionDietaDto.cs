namespace NutriApi.DTOs.Dietas;

public class SuplementacionDietaDto
{
    public int Id { get; set; }

    public int DietaId { get; set; }

    public string? ObservacionesGenerales { get; set; }

    public List<ItemSuplementacionDto> Items { get; set; }
        = new();
}