namespace NutriApp.Models.Alimentos;

public class EquivalenciaAlimento
{
    public int Id { get; set; }

    public int GrupoEquivalenciaId { get; set; }

    public int AlimentoId { get; set; }

    public bool Activa { get; set; } = true;


    // Navegación

    public GrupoEquivalencia GrupoEquivalencia { get; set; } = null!;

    public Alimento Alimento { get; set; } = null!;
}