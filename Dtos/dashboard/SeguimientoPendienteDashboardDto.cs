namespace NutriApi.DTOs.Dashboard;

public class SeguimientoPendienteDashboardDto
{
    public int SeguimientoId { get; set; }

    public int PacienteId { get; set; }

    public string NombrePaciente { get; set; } =
        string.Empty;

    public DateOnly FechaInicioSemana { get; set; }

    public DateOnly FechaFinSemana { get; set; }

    public DateTime FechaRespuesta { get; set; }
}