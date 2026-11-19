using NutriApp.Enums.Competencias;
using NutriApp.Models.Usuarios;

namespace NutriApp.Models.Competencias;

public class SolicitudCompetencia
{
    // ==========================================
    // IDENTIFICADOR
    // ==========================================

    public int Id { get; set; }

    // ==========================================
    // PACIENTE
    // ==========================================

    public int PacienteId { get; set; }

    public Paciente Paciente { get; set; } = null!;

    // ==========================================
    // INFORMACIÓN SOLICITADA
    // ==========================================

    public string Nombre { get; set; } = string.Empty;

    public string Disciplina { get; set; } = string.Empty;

    public DateOnly FechaInicio { get; set; }

    public DateOnly? FechaFin { get; set; }

    public string? Lugar { get; set; }

    public string? ComentarioPaciente { get; set; }

    // ==========================================
    // ESTADO DE LA SOLICITUD
    // ==========================================

    public EstadoSolicitudCompetencia Estado { get; set; } = EstadoSolicitudCompetencia.Pendiente;

    public string? MotivoRechazo { get; set; }

    // ==========================================
    // COMPETENCIA GENERADA AL APROBAR
    // ==========================================

    public int? CompetenciaId { get; set; }

    public Competencia? Competencia { get; set; }

    // ==========================================
    // AUDITORÍA
    // ==========================================

    public DateTime FechaSolicitud { get; set; } = DateTime.UtcNow;

    public DateTime? FechaResolucion { get; set; }
}
