namespace NutriApi.DTOs.RecuperacionPassword;

public class RecuperacionPasswordPacienteGeneradaDto
{
    public int PacienteId { get; set; }

    public string Email { get; set; } =
        string.Empty;

    public string? Telefono { get; set; }

    public string EnlaceRecuperacion { get; set; } =
        string.Empty;
}