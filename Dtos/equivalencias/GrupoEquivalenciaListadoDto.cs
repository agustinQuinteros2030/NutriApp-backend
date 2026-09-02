namespace NutriApi.DTOs.Equivalencias;

public class GrupoEquivalenciaListadoDto
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string? Descripcion { get; set; }

    public bool Activo { get; set; }

    public int CantidadAlimentos { get; set; }
}