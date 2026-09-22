namespace NutriApi.DTOs.Dashboard;

public class CobroVencidoDashboardDto
{
    public int PacienteId { get; set; }

    public string NombrePaciente { get; set; } =
        string.Empty;

    public DateOnly UltimoPago { get; set; }

    public DateOnly ProximoVencimiento { get; set; }

    public int DiasVencido { get; set; }
}