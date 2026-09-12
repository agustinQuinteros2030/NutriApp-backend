namespace NutriApi.DTOs.ActivacionCuenta;

public class EstadoActivacionCuentaDto
{
    public int PacienteId { get; set; }

    public string Email { get; set; } =
        string.Empty;

    public bool CuentaActivada { get; set; }
}