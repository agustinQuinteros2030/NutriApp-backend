namespace NutriApi.DTOs.Dashboard;

public class CobroProximoDashboardDto
{
    public int PacienteId { get; set; }

    public string NombrePaciente { get; set; } =
        string.Empty;

    public DateOnly UltimoPago { get; set; }

    public DateOnly ProximoVencimiento { get; set; }

    public int DiasParaVencimiento { get; set; }
}