public class RegistroDiarioPacienteDto
{
    public int Id { get; set; }

    public int PacienteId { get; set; }

    public DateOnly Fecha { get; set; }

    public bool? CumplioPlan { get; set; }

    public decimal? CinturaCm { get; set; }

    public decimal? CaderaCm { get; set; }

    public decimal? GemeloCm { get; set; }

    public decimal? CuelloCm { get; set; }

    public string? Observaciones { get; set; }

    public DateTime FechaCreacion { get; set; }

    public DateTime? FechaActualizacion { get; set; }
}