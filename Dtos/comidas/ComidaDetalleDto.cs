namespace NutriApi.DTOs.Dietas;

public class ComidaDetalleDto
{
    public int Id { get; set; }

    public int DietaId { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string Tipo { get; set; } = string.Empty;

    public int Orden { get; set; }

    public string? Observaciones { get; set; }

    public List<SeccionComidaDto> Secciones { get; set; }
        = new();
}