namespace NutriApp.Models.ControlSeguimiento;

public class ControlSeguimientoPaciente
{
    public int Id { get; set; }

    public int PacienteId { get; set; }

    public bool Activo { get; set; } = true;

    public int FrecuenciaDias { get; set; } = 7;

    public DateOnly? UltimoSeguimiento { get; set; }

    public DateOnly ProximoSeguimiento { get; set; }

    public DateTime FechaCreacion { get; set; }

    public DateTime? FechaActualizacion { get; set; }
}