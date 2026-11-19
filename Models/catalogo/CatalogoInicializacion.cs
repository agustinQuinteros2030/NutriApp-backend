namespace NutriApp.Models.Catalogo;

public sealed class CatalogoInicializacion
{
    public int Id { get; set; }

    public int NutricionistaId { get; set; }

    public string VersionCatalogo { get; set; } = string.Empty;

    public DateTime FechaInicializacion { get; set; } = DateTime.UtcNow;
}
