namespace NutriApi.DTOs.Dietas;

public class SimulacionAjustesPorcionesDto
{
    public int DietaId { get; set; }

    public int ComidaId { get; set; }

    public string NombreComida { get; set; } = string.Empty;

    public bool EsCalculable { get; set; }

    public string? Motivo { get; set; }

    public ObjetivosSimulacionDto Objetivos { get; set; } = new();

    public NutrientesSimulacionDto? TotalesActuales { get; set; }

    public List<PropuestaAjustePorcionDto> Propuestas { get; set; } = new();
}

public class ObjetivosSimulacionDto
{
    public decimal? Calorias { get; set; }

    public decimal? Proteinas { get; set; }

    public decimal? Carbohidratos { get; set; }

    public decimal? Grasas { get; set; }
}

public class NutrientesSimulacionDto
{
    public decimal Calorias { get; set; }

    public decimal Proteinas { get; set; }

    public decimal Carbohidratos { get; set; }

    public decimal Grasas { get; set; }
}

public class PropuestaAjustePorcionDto
{
    public int ItemId { get; set; }

    public int AlimentoId { get; set; }

    public string NombreAlimento { get; set; } = string.Empty;

    public decimal CantidadActual { get; set; }

    public decimal CantidadSugerida { get; set; }

    public string UnidadMedida { get; set; } = string.Empty;

    public NutrientesSimulacionDto TotalesSimulados { get; set; } = new();
}
