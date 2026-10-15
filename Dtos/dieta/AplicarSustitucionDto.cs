namespace NutriApi.DTOs.Dietas;

public class AplicarSustitucionDto
{
    public int AlimentoDestinoId { get; set; }

    public int GrupoEquivalenciaId { get; set; }

    public int AlimentoOrigenEsperadoId { get; set; }

    public decimal CantidadOrigenEsperada { get; set; }

    public string UnidadOrigenEsperada { get; set; } = string.Empty;

    public decimal CantidadDestinoEsperada { get; set; }
}

public class SustitucionAplicadaDto
{
    public int DietaId { get; set; }

    public int ComidaId { get; set; }

    public int ItemId { get; set; }

    public int AlimentoAnteriorId { get; set; }

    public string NombreAlimentoAnterior { get; set; } = string.Empty;

    public decimal CantidadAnterior { get; set; }

    public string UnidadAnterior { get; set; } = string.Empty;

    public int AlimentoNuevoId { get; set; }

    public string NombreAlimentoNuevo { get; set; } = string.Empty;

    public decimal CantidadNueva { get; set; }

    public string UnidadNueva { get; set; } = string.Empty;

    public int AlternativasDesactivadas { get; set; }

    public bool Aplicada { get; set; }
}
