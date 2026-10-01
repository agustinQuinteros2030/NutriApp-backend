using NutriApp.Models.Usuarios;

namespace NutriApp.Models.Mensajes;

public class MensajePaciente
{
    public int Id { get; set; }

    // ==========================================
    // RELACIONES
    // ==========================================

    public int NutricionistaId { get; set; }

    public Nutricionista Nutricionista { get; set; } = null!;

    public int PacienteId { get; set; }

    public Paciente Paciente { get; set; } = null!;

    // ==========================================
    // CONTENIDO
    // ==========================================

    public string Titulo { get; set; } = string.Empty;

    public string Contenido { get; set; } = string.Empty;

    // ==========================================
    // ESTADO
    // ==========================================

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public DateTime? FechaActualizacion { get; set; }

    public bool Leido { get; set; } = false;

    public DateTime? FechaLectura { get; set; }
}
