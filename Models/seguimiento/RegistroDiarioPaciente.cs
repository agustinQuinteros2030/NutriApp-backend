using NutriApp.Models.Usuarios;

public class RegistroDiarioPaciente
{
    public int Id { get; set; }

    public int PacienteId { get; set; }

    public DateOnly Fecha { get; set; }


    // ==========================================
    // CHECK-IN DIARIO
    // ==========================================

    /*
     * true  = cumplió
     * false = no cumplió
     * null  = no respondió
     */

    public bool? CumplioPlan { get; set; }


    // ==========================================
    // MEDICIONES OPCIONALES
    // ==========================================

    public decimal? CinturaCm { get; set; }

    public decimal? CaderaCm { get; set; }

    public decimal? GemeloCm { get; set; }

    public decimal? CuelloCm { get; set; }


    // ==========================================
    // OBSERVACIONES
    // ==========================================

    public string? Observaciones { get; set; }


    // ==========================================
    // AUDITORÍA
    // ==========================================

    public DateTime FechaCreacion { get; set; }
        = DateTime.UtcNow;

    public DateTime? FechaActualizacion { get; set; }


    public Paciente Paciente { get; set; } = null!;
}