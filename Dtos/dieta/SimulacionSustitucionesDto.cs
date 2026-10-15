namespace NutriApi.DTOs.Dietas;

public class SimulacionSustitucionesDto
{
    public int DietaId { get; set; }
    public int ComidaId { get; set; }
    public string NombreComida { get; set; } = string.Empty;

    public bool EsCalculable { get; set; }
    public string? Motivo { get; set; }

    public ObjetivosSimulacionDto Objetivos { get; set; } = new();

    public NutrientesSimulacionDto? TotalesActuales { get; set; }

    public List<PropuestaSustitucionDto> Propuestas { get; set; } = [];
}

public class PropuestaSustitucionDto
{
    public int ItemOrigenId { get; set; }

    public int AlimentoOrigenId { get; set; }
    public string NombreOrigen { get; set; } = string.Empty;
    public decimal CantidadOrigen { get; set; }
    public string UnidadOrigen { get; set; } = string.Empty;

    public int AlimentoDestinoId { get; set; }
    public string NombreDestino { get; set; } = string.Empty;

    public decimal CantidadTeoricaDestino { get; set; }
    public decimal CantidadSugerida { get; set; }
    public string UnidadDestino { get; set; } = string.Empty;

    public int GrupoEquivalenciaId { get; set; }
    public string Criterio { get; set; } = string.Empty;

    public decimal DesviacionCriterioPorcentaje { get; set; }

    public decimal? MejoraPuntaje { get; set; }
    public string EstadoBalance { get; set; } = string.Empty;

    public NutrientesSimulacionDto TotalesSimulados { get; set; } = new();

    public List<ImpactoAjusteNutrienteDto> Impactos { get; set; } = [];

    public List<string> Advertencias { get; set; } = [];
}
