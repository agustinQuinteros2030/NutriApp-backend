namespace NutriApp.Models.Dietas;

public class ItemSuplementacion
{
    public int Id { get; set; }

    public int SuplementacionDietaId { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public decimal? Cantidad { get; set; }

    public string? Unidad { get; set; }

    public string? Momento { get; set; }

    public string? Indicaciones { get; set; }

    public int Orden { get; set; }


    // Navegación

    public SuplementacionDieta SuplementacionDieta { get; set; } = null!;
}