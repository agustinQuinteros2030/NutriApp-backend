namespace NutriApi.DTOs.Dietas;

public class SimulacionAjustesPorcionesV2Dto
{
    public int DietaId { get; set; }

    public int ComidaId { get; set; }

    public string NombreComida { get; set; } = string.Empty;

    public string PrioridadAplicada { get; set; } = string.Empty;

    public bool EsCalculable { get; set; }

    public string? Motivo { get; set; }

    public ObjetivosSimulacionDto Objetivos { get; set; } = new();

    public NutrientesSimulacionDto? TotalesActuales { get; set; }

    public List<PropuestaAjustePorcionV2Dto> Propuestas { get; set; } = new();
}

public class PropuestaAjustePorcionV2Dto
{
    public int ItemId { get; set; }

    public int AlimentoId { get; set; }

    public string NombreAlimento { get; set; } = string.Empty;

    public decimal CantidadActual { get; set; }

    public decimal CantidadSugerida { get; set; }

    public string UnidadMedida { get; set; } = string.Empty;

    public decimal MejoraPuntaje { get; set; }

    public string Explicacion { get; set; } = string.Empty;

    public NutrientesSimulacionDto TotalesSimulados { get; set; } = new();

    public List<ImpactoAjusteNutrienteDto> Impactos { get; set; } = new();
}

public class ImpactoAjusteNutrienteDto
{
    public string Nutriente { get; set; } = string.Empty;

    public decimal? Objetivo { get; set; }

    public decimal Actual { get; set; }

    public decimal Simulado { get; set; }

    public decimal? DiferenciaAnterior { get; set; }

    public decimal? DiferenciaSimulada { get; set; }

    // Mejora, Empeora, SinCambio, SinObjetivo.
    public string Estado { get; set; } = string.Empty;
}
