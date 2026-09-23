namespace NutriApi.DTOs.Turnos;

public class TurnoDto
{
    public int Id { get; set; }

    public int PacienteId { get; set; }

    public string NombrePaciente { get; set; } =
        string.Empty;


    // UTC
    public DateTime FechaHora { get; set; }


    public string Modalidad { get; set; } =
        string.Empty;

    public string Estado { get; set; } =
        string.Empty;


    public string? Lugar { get; set; }

    public string? LinkReunion { get; set; }

    public string? Motivo { get; set; }

    public string? Observaciones { get; set; }


    public DateTime FechaCreacion { get; set; }

    public DateTime? FechaActualizacion { get; set; }
}