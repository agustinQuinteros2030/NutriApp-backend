using NutriApi.Models.dieta;
using NutriApp.Enums;
using System.Collections.Generic;


namespace NutriApp.Models.Dietas;

public class SeccionComida
{
    public int Id { get; set; }

    public int ComidaId { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public TipoSeccionComida Tipo { get; set; }

    public int Orden { get; set; }

    public string? Observaciones { get; set; }


    // Navegación

    public Comida Comida { get; set; } = null!;

    public ICollection<OpcionSeccionComida> Opciones { get; set; } = [];
}