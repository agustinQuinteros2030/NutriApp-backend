using System.ComponentModel.DataAnnotations;
using NutriApp.Enums;

namespace NutriApi.DTOs.Equivalencias;

public class EditarEquivalenciaAlimentoDto
{
    [Range(typeof(decimal), "0.01", "999999")]
    public decimal CantidadEquivalente { get; set; }

    public UnidadMedida UnidadMedida { get; set; }
}