namespace NutriApi.DTOs.Equivalencias;

public class EquivalenciaAlimentoDto
{
    public int Id { get; set; }

    public int AlimentoId { get; set; }

    public string Alimento { get; set; } = string.Empty;

    public decimal CantidadEquivalente { get; set; }

    public string UnidadMedida { get; set; } = string.Empty;

    public bool Activa { get; set; }
}