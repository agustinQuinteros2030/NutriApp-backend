namespace NutriApi.DTOs.Equivalencias;

public class ConversionEquivalenciaDto
{
    public int AlimentoId { get; set; }

    public string Alimento { get; set; } = string.Empty;

    public decimal Cantidad { get; set; }

    public string UnidadMedida { get; set; } = string.Empty;
}