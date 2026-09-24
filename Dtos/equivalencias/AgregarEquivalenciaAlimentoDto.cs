using System.ComponentModel.DataAnnotations;

namespace NutriApi.DTOs.Equivalencias;

public class AgregarEquivalenciaAlimentoDto
{
    [Range(1, int.MaxValue)]
    public int AlimentoId { get; set; }
}