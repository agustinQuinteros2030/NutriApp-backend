using NutriApp.Models.Usuarios;

namespace NutriApp.Models.Seguimiento;

public class RegistroDiarioPaciente
{
    public int Id { get; set; }

    public int PacienteId { get; set; }

    public DateOnly Fecha { get; set; }

    public int? AdherenciaPorcentaje { get; set; }

    public int? Hambre { get; set; }

    public int? Energia { get; set; }

    public bool? Entreno { get; set; }

    public string? Observaciones { get; set; }

    public DateTime FechaCreacion { get; set; }
        = DateTime.UtcNow;

    public DateTime? FechaActualizacion { get; set; }

    public Paciente Paciente { get; set; } = null!;
}