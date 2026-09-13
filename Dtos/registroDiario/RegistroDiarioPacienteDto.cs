namespace NutriApi.DTOs.RegistroDiario;

public class RegistroDiarioPacienteDto
{
    public int Id { get; set; }

    public int PacienteId { get; set; }

    public DateOnly Fecha { get; set; }

    public int? AdherenciaPorcentaje { get; set; }

    public int? Hambre { get; set; }

    public int? Energia { get; set; }

    public bool? Entreno { get; set; }

    public string? Observaciones { get; set; }

    public DateTime FechaCreacion { get; set; }

    public DateTime? FechaActualizacion { get; set; }
}