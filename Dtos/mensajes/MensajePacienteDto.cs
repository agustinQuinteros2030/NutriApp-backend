namespace NutriApi.DTOs.Mensajes;

public class MensajePacienteDto
{
    public int Id { get; set; }

    public int PacienteId { get; set; }

    public string Titulo { get; set; } = string.Empty;

    public string Contenido { get; set; } = string.Empty;

    public DateTime FechaCreacion { get; set; }

    public DateTime? FechaActualizacion { get; set; }

    public bool Leido { get; set; }

    public DateTime? FechaLectura { get; set; }
}
