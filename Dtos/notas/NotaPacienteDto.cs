namespace NutriApi.DTOs.Notas;

public class NotaPacienteDto
{
    public int Id { get; set; }

    public int PacienteId { get; set; }

    public string Contenido { get; set; } =
        string.Empty;

    public DateTime FechaCreacion { get; set; }

    public DateTime? FechaActualizacion { get; set; }
}