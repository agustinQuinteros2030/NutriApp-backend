namespace NutriApi.DTOs.Alimentos;

public class AlimentoListadoDto
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public int CategoriaAlimentoId { get; set; }

    public string Categoria { get; set; } = string.Empty;

    public decimal CantidadBase { get; set; }

    public string UnidadBase { get; set; } = string.Empty;

    public bool Activo { get; set; }
}