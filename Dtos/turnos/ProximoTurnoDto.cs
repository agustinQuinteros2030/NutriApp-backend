namespace NutriApi.DTOs.Turnos;

public class ProximoTurnoDto
{
    public int Id { get; set; }

    public DateTime FechaHora { get; set; }

    public string Modalidad { get; set; } =
        string.Empty;

    public string? Lugar { get; set; }

    public string? LinkReunion { get; set; }

    public string? Motivo { get; set; }
}