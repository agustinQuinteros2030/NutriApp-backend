namespace NutriApi.DTOs.Alimentos;

public class AlimentoDetalleDto
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public int CategoriaAlimentoId { get; set; }

    public string Categoria { get; set; } = string.Empty;

    public decimal CantidadBase { get; set; }

    public string UnidadBase { get; set; } = string.Empty;

    public decimal? Calorias { get; set; }

    public decimal? Proteinas { get; set; }

    public decimal? Carbohidratos { get; set; }

    public decimal? Grasas { get; set; }

    public bool Activo { get; set; }

    public DateTime FechaCreacion { get; set; }

    public DateTime? FechaActualizacion { get; set; }
}