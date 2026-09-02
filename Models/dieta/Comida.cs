using NutriApi.Models.dieta;

using NutriApp.Enums;
using System.Collections.Generic;


namespace NutriApp.Models.Dietas;

public class Comida
{
    public int Id { get; set; }

    public int DietaId { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public TipoComida Tipo { get; set; }

    public int Orden { get; set; }

    public string? Observaciones { get; set; }


    // Navegación

    public Dieta Dieta { get; set; } = null!;

    public ICollection<SeccionComida> Secciones { get; set; } = [];
}