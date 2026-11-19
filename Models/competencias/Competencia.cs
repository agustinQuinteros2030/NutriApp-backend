using NutriApp.Models.Usuarios;

namespace NutriApp.Models.Competencias;

public class Competencia
{
    public int Id { get; set; }

    // ==========================================
    // PACIENTE
    // ==========================================

    public int PacienteId { get; set; }

    public Paciente Paciente { get; set; } = null!;

    // ==========================================
    // INFORMACIÓN DE LA COMPETENCIA
    // ==========================================

    public string Nombre { get; set; } = string.Empty;

    public string Disciplina { get; set; } = string.Empty;

    public DateOnly FechaInicio { get; set; }

    public DateOnly? FechaFin { get; set; }

    public string? Lugar { get; set; }

    public string? Observaciones { get; set; }

    // ==========================================
    // ESTADO
    // ==========================================

    public EstadoCompetencia Estado { get; set; } = EstadoCompetencia.Programada;

    // ==========================================
    // AUDITORÍA
    // ==========================================

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public DateTime? FechaActualizacion { get; set; }
}
