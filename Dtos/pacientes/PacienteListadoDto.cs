namespace NutriApi.DTOs.Pacientes;

public class PacienteListadoDto
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string Apellido { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? Telefono { get; set; }

    public bool Activo { get; set; }

    public string ObjetivoNutricional { get; set; } = string.Empty;
}