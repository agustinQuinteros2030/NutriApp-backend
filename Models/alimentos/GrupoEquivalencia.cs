

using System;
using System.Collections.Generic;

namespace NutriApp.Models.Alimentos;

public class GrupoEquivalencia
{
    public int Id { get; set; }

    public int NutricionistaId { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string? Descripcion { get; set; }

    public bool Activo { get; set; } = true;

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;


    // Navegación

    public Nutricionista Nutricionista { get; set; } = null!;

    public ICollection<EquivalenciaAlimento> Equivalencias { get; set; } = [];
}