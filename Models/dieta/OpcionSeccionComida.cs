using NutriApi.Models.dieta;
using System.Collections.Generic;

namespace NutriApp.Models.Dietas;

public class OpcionSeccionComida
{
    public int Id { get; set; }

    public int SeccionComidaId { get; set; }

    public string? Nombre { get; set; }

    public int Orden { get; set; }

    public bool EsPredeterminada { get; set; }

    public string? Observaciones { get; set; }


    // Navegación

    public SeccionComida SeccionComida { get; set; } = null!;

    public ICollection<ItemOpcionComida> Items { get; set; } = [];
}