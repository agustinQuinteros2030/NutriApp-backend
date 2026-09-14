namespace NutriApi.DTOs.SeguimientoSemanal;

public class SeguimientoSemanalDto
{
    public int Id { get; set; }

    public int PacienteId { get; set; }


    public DateOnly FechaInicioSemana { get; set; }

    public DateOnly FechaFinSemana { get; set; }

    public DateTime FechaRespuesta { get; set; }


    public decimal PesoActual { get; set; }

    public int Adherencia { get; set; }

    public string Descanso { get; set; } =
        string.Empty;

    public string Digestiones { get; set; } =
        string.Empty;

    public string? DetalleDigestiones { get; set; }

    public string RendimientoEntrenamientos { get; set; } =
        string.Empty;

    public string CumplimientoHidratacion { get; set; } =
        string.Empty;

    public string RegularidadIntestinal { get; set; } =
        string.Empty;

    public bool TuvoMolestiaFisica { get; set; }

    public string? DetalleMolestiaFisica { get; set; }

    public int SatisfaccionComunicacion { get; set; }


    public string? RevisionNutricionista { get; set; }

    public DateTime? FechaRevisionNutricionista { get; set; }


    public bool Revisado =>
        FechaRevisionNutricionista.HasValue;
}