using NutriApp.Models.Alimentos;

namespace NutriApp.Models.Dietas;

public class AlternativaItemComida
{
    public int Id { get; set; }

    public int ItemOpcionComidaId { get; set; }

    public int AlimentoId { get; set; }

    public int GrupoEquivalenciaId { get; set; }

    public bool Activa { get; set; } = true;


    public ItemOpcionComida ItemOpcionComida { get; set; } = null!;

    public Alimento Alimento { get; set; } = null!;

    public GrupoEquivalencia GrupoEquivalencia { get; set; } = null!;
}