using System.ComponentModel.DataAnnotations;
using NutriApp.Enums;

namespace NutriApi.DTOs.Dietas;

public class CrearItemOpcionComidaDto
{
    [Range(1, int.MaxValue)]
    public int AlimentoId { get; set; }

    [Range(typeof(decimal), "0.01", "999999")]
    public decimal Cantidad { get; set; }

    public UnidadMedida UnidadMedida { get; set; }

    [MaxLength(1000)]
    public string? Indicaciones { get; set; }

    [Range(1, 100)]
    public int Orden { get; set; }
}