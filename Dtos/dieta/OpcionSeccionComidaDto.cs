namespace NutriApi.DTOs.Dietas;

public class OpcionSeccionComidaDto
{
    public int Id { get; set; }

    public int SeccionComidaId { get; set; }

    public string Nombre { get; set; } =
        string.Empty;

    public int Orden { get; set; }

    public bool EsPredeterminada { get; set; }

    public string? Observaciones { get; set; }

    public List<ItemOpcionComidaDto> Items { get; set; }
        = new();


    public TotalesNutricionalesDto Totales { get; set; }
        = new();
}