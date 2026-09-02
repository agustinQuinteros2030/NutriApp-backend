using System.ComponentModel.DataAnnotations;
using NutriApp.Enums;

namespace NutriApi.DTOs.Equivalencias;

public class AgregarEquivalenciaAlimentoDto
{
    [Range(1, int.MaxValue)]
    public int AlimentoId { get; set; }

    [Range(typeof(decimal), "0.01", "999999")]
    public decimal CantidadEquivalente { get; set; }

    public UnidadMedida UnidadMedida { get; set; }
}