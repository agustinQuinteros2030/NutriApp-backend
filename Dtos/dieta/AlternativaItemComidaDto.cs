namespace NutriApi.DTOs.Dietas;

public class AlternativaItemComidaDto
{
    public int Id { get; set; }

    public int AlimentoId { get; set; }

    public string Alimento { get; set; } = string.Empty;

    public int GrupoEquivalenciaId { get; set; }

    public string GrupoEquivalencia { get; set; } = string.Empty;

    public decimal CantidadCalculada { get; set; }

    public string UnidadMedida { get; set; } = string.Empty;

    public bool Activa { get; set; }

    public NutricionCalculadaDto? Nutricion { get; set; }
}