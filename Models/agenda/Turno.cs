using NutriApp.Models.Usuarios;

public class Turno
{
    public int Id { get; set; }

    public int PacienteId { get; set; }

    public DateTime FechaHora { get; set; }

    public ModalidadTurno Modalidad { get; set; }

    public EstadoTurno Estado { get; set; }

    public string? Lugar { get; set; }

    public string? LinkReunion { get; set; }

    public string? Motivo { get; set; }

    public string? Observaciones { get; set; }

    public DateTime FechaCreacion { get; set; }
        = DateTime.UtcNow;

    public DateTime? FechaActualizacion { get; set; }

    public Paciente Paciente { get; set; } =
        null!;
}