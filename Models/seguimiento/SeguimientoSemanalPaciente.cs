using NutriApp.Enums.Seguimiento;
using NutriApp.Models.Usuarios;

namespace NutriApp.Models.Seguimiento;

public class SeguimientoSemanalPaciente
{
    public int Id { get; set; }


    // =========================
    // RELACIÓN
    // =========================

    public int PacienteId { get; set; }

    public Paciente Paciente { get; set; } =
        null!;


    // =========================
    // SEMANA
    // =========================

    public DateOnly FechaInicioSemana { get; set; }

    public DateOnly FechaFinSemana { get; set; }

    public DateTime FechaRespuesta { get; set; }


    // =========================
    // RESPUESTAS DEL PACIENTE
    // =========================

    public decimal PesoActual { get; set; }

    public int Adherencia { get; set; }

    public DescansoSemanal Descanso { get; set; }

    public DigestionSemanal Digestiones { get; set; }

    public string? DetalleDigestiones { get; set; }

    public RendimientoEntrenamientoSemanal
        RendimientoEntrenamientos
    { get; set; }

    public CumplimientoHidratacionSemanal
        CumplimientoHidratacion
    { get; set; }

    public RegularidadIntestinalSemanal
        RegularidadIntestinal
    { get; set; }

    public bool TuvoMolestiaFisica { get; set; }

    public string? DetalleMolestiaFisica { get; set; }

    public int SatisfaccionComunicacion { get; set; }


    // =========================
    // REVISIÓN NUTRICIONISTA
    // =========================

    public string? RevisionNutricionista { get; set; }

    public DateTime? FechaRevisionNutricionista { get; set; }
}