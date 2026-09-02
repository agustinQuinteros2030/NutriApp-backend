namespace NutriApi.DTOs.Pacientes;

public class PacienteDetalleDto
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string Apellido { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? Telefono { get; set; }

    public DateOnly? FechaNacimiento { get; set; }

    public bool Activo { get; set; }


    public string ObjetivoNutricional { get; set; } = string.Empty;

    public string TipoActividad { get; set; } = string.Empty;

    public decimal? PesoInicial { get; set; }

    public decimal? Altura { get; set; }

    public DateOnly? FechaInicio { get; set; }

    public string? ActividadDescripcion { get; set; }

    public string? ObservacionesGenerales { get; set; }

    public DateTime FechaCreacion { get; set; }
}