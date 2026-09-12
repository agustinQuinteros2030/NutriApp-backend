namespace NutriApi.DTOs.ActivacionCuenta;

public class ActivacionCuentaGeneradaDto
{
    public int PacienteId { get; set; }

    public string Email { get; set; } =
        string.Empty;

    public string Token { get; set; } =
        string.Empty;

    public bool CuentaActivada { get; set; }
}