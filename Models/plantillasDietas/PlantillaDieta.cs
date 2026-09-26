namespace NutriApp.Models.PlantillasDietas;

public class PlantillaDieta
{
    public int Id { get; set; }

    public int NutricionistaId { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string? Descripcion { get; set; }

    /*
     * Snapshot completo de la estructura
     * reutilizable de la dieta.
     *
     * Se almacena como jsonb en PostgreSQL.
     */
    public string ContenidoJson { get; set; } = string.Empty;

    public DateTime FechaCreacion { get; set; }

    public DateTime? FechaActualizacion { get; set; }
}
