namespace NutriApi.DTOs.Pagos;

public class PagoPacienteDto
{
    public int Id { get; set; }

    public int PacienteId { get; set; }

    public DateOnly FechaPago { get; set; }

    public DateOnly ProximoVencimiento { get; set; }

    public decimal? Monto { get; set; }

    public string? MetodoPago { get; set; }

    public string? Observaciones { get; set; }

    public DateTime FechaCreacion { get; set; }
}