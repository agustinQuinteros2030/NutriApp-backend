namespace NutriApi.DTOs.Dietas;

public class ItemOpcionComidaDto
{
    public int Id { get; set; }

    public int AlimentoId { get; set; }

    public string Alimento { get; set; } =
        string.Empty;

    public decimal Cantidad { get; set; }

    public string UnidadMedida { get; set; } =
        string.Empty;

    public string? Indicaciones { get; set; }

    public int Orden { get; set; }

    public int CantidadAlternativas { get; set; }


    // ==========================================
    // NUTRICIÓN CALCULADA
    // ==========================================

    public NutricionCalculadaDto? Nutricion { get; set; }
}