namespace NutriApi.DTOs.Dashboard;

public class PacienteRecienteDashboardDto
{
    public int Id { get; set; }

    public string NombreCompleto { get; set; } =
        string.Empty;

    public string Email { get; set; } =
        string.Empty;

    public bool Activo { get; set; }

    public bool CuentaActivada { get; set; }

    public DateTime FechaCreacion { get; set; }
}