using System.ComponentModel.DataAnnotations;

namespace NutriApi.DTOs.Dietas;

public class AgregarAlternativaItemComidaDto
{
    [Range(1, int.MaxValue)]
    public int AlimentoId { get; set; }

    [Range(1, int.MaxValue)]
    public int GrupoEquivalenciaId { get; set; }
}