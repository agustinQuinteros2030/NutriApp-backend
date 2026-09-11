namespace NutriApi.DTOs.Pagos;

public class EstadoPagoPacienteDto
{
    public bool TienePagos { get; set; }

    public DateOnly? UltimoPago { get; set; }

    public DateOnly? ProximoVencimiento { get; set; }

    public decimal? MontoUltimoPago { get; set; }

    public string Estado { get; set; } =
        "SinRegistro";

    public bool Vencido { get; set; }

    public bool VenceHoy { get; set; }

    public int DiasParaVencer { get; set; }

    public int DiasVencido { get; set; }
}