using NutriApi.Models.dieta;
using System.Collections.Generic;

namespace NutriApp.Models.Dietas;

public class SuplementacionDieta
{
    public int Id { get; set; }

    public int DietaId { get; set; }

    public string? ObservacionesGenerales { get; set; }


    // Navegación

    public Dieta Dieta { get; set; } = null!;

    public ICollection<ItemSuplementacion> Items { get; set; } = [];
}