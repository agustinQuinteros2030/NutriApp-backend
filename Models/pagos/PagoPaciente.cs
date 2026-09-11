using NutriApp.Models.Usuarios;

namespace NutriApp.Models.Pagos;

public class PagoPaciente
{
    public int Id { get; set; }

    public int PacienteId { get; set; }

    public DateOnly FechaPago { get; set; }

    public DateOnly ProximoVencimiento { get; set; }

    public decimal? Monto { get; set; }

    public string? MetodoPago { get; set; }

    public string? Observaciones { get; set; }

    public DateTime FechaCreacion { get; set; }
        = DateTime.UtcNow;


    // Navegación

    public Paciente Paciente { get; set; } = null!;
}