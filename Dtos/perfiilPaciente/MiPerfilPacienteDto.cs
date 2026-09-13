namespace NutriApi.DTOs.PerfilPaciente;

public class MiPerfilPacienteDto
{
    public int Id { get; set; }

    public string Nombre { get; set; } =
        string.Empty;

    public string Apellido { get; set; } =
        string.Empty;

    public string Email { get; set; } =
        string.Empty;

    public string? Telefono { get; set; }

    public DateOnly? FechaNacimiento { get; set; }

    public string? ObjetivoNutricional { get; set; }

    public string? TipoActividad { get; set; }

    public decimal? PesoInicial { get; set; }

    public decimal? Altura { get; set; }

    public DateOnly? FechaInicio { get; set; }

    public string? ActividadDescripcion { get; set; }
}