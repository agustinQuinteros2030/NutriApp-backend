namespace NutriApi.DTOs.PlantillasDietas;

public class PlantillaDietaContenidoDto
{
    public string? ObservacionesGenerales { get; set; }

    public List<PlantillaComidaDto> Comidas { get; set; } = new();

    public PlantillaHidratacionDto? Hidratacion { get; set; }

    public PlantillaSuplementacionDto? Suplementacion { get; set; }
}

public class PlantillaComidaDto
{
    public string Nombre { get; set; } = string.Empty;

    public int Tipo { get; set; }

    public int Orden { get; set; }

    public string? Observaciones { get; set; }

    public List<PlantillaSeccionDto> Secciones { get; set; } = new();
}

public class PlantillaSeccionDto
{
    public string Nombre { get; set; } = string.Empty;

    public int Tipo { get; set; }

    public int Orden { get; set; }

    public string? Observaciones { get; set; }

    public List<PlantillaOpcionDto> Opciones { get; set; } = new();
}

public class PlantillaOpcionDto
{
    public string Nombre { get; set; } = string.Empty;

    public int Orden { get; set; }

    public bool EsPredeterminada { get; set; }

    public string? Observaciones { get; set; }

    public List<PlantillaItemDto> Items { get; set; } = new();
}

public class PlantillaItemDto
{
    public int AlimentoId { get; set; }

    public decimal Cantidad { get; set; }

    public int UnidadMedida { get; set; }

    public string? Indicaciones { get; set; }

    public int Orden { get; set; }

    public List<PlantillaAlternativaDto> Alternativas { get; set; } = new();
}

public class PlantillaAlternativaDto
{
    public int AlimentoId { get; set; }

    public int GrupoEquivalenciaId { get; set; }

    public bool Activa { get; set; }
}

public class PlantillaHidratacionDto
{
    public int? MililitrosDiarios { get; set; }

    public int? VasosDiarios { get; set; }

    public string? Observaciones { get; set; }
}

public class PlantillaSuplementacionDto
{
    public string? ObservacionesGenerales { get; set; }

    public List<PlantillaItemSuplementacionDto> Items { get; set; } = new();
}

public class PlantillaItemSuplementacionDto
{
    public string Nombre { get; set; } = string.Empty;

    public decimal? Cantidad { get; set; }

    public string? Unidad { get; set; }

    public string? Momento { get; set; }

    public string? Indicaciones { get; set; }

    public int Orden { get; set; }
}
