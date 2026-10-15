
namespace NutriApi.DTOs.Dietas;

public class ComparacionAlternativasDietaDto
{
    public int DietaId { get; set; }

    public string NombreDieta { get; set; } = string.Empty;

    public decimal ToleranciaCalorias { get; set; } = 100m;

    public int SeccionesComparadas { get; set; }

    public int SeccionesFueraDeRango { get; set; }

    // null: no se pudo evaluar toda la comparación.
    public bool? CumpleTolerancia { get; set; }

    public List<ComparacionAlternativasSeccionDto> Secciones
    { get; set; } = new();

    public List<string> Advertencias
    { get; set; } = new();
}

public class ComparacionAlternativasSeccionDto
{
    public int ComidaId { get; set; }

    public string NombreComida { get; set; } = string.Empty;

    public int SeccionId { get; set; }

    public string NombreSeccion { get; set; } = string.Empty;

    public int CantidadOpciones { get; set; }

    public decimal? CaloriasMinimas { get; set; }

    public decimal? CaloriasMaximas { get; set; }

    public decimal? DiferenciaCalorias { get; set; }

    public bool? CumpleTolerancia { get; set; }

    // SinOpciones, SinAlternativas,
    // DatosIncompletos, DentroDeRango, FueraDeRango.
    public string Estado { get; set; } = "SinOpciones";

    public List<ComparacionAlternativaOpcionDto> Opciones
    { get; set; } = new();
}

public class ComparacionAlternativaOpcionDto
{
    public int OpcionId { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public bool EsPredeterminada { get; set; }

    public decimal? Calorias { get; set; }

    public bool CalculoCompleto { get; set; }
}
