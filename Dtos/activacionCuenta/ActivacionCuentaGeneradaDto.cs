namespace NutriApi.DTOs.ActivacionCuenta;

public class ActivacionCuentaGeneradaDto
{
    public int PacienteId { get; set; }

    public string Email { get; set; } =
        string.Empty;

    public string? Telefono { get; set; }

    public bool CuentaActivada { get; set; }

    public string EnlaceActivacion { get; set; } =
        string.Empty;
}