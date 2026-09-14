namespace NutriApi.DTOs.SeguimientoSemanal;

public class EstadoSeguimientoSemanalDto
{
    public bool Disponible { get; set; }

    public bool Completado { get; set; }

    public DateOnly FechaInicioSemana { get; set; }

    public DateOnly FechaFinSemana { get; set; }

    public int? SeguimientoId { get; set; }
}