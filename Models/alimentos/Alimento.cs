
using NutriApi.Models.dieta;

using NutriApp.Enums;
using NutriApp.Models.Dietas;
using NutriApp.Models.Usuarios;
using System;
using System.Collections.Generic;

namespace NutriApp.Models.Alimentos;

public class Alimento
{
    public int Id { get; set; }

    public int NutricionistaId { get; set; }

    public int CategoriaAlimentoId { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public decimal CantidadBase { get; set; }

    public UnidadMedida UnidadBase { get; set; }

    public decimal? Calorias { get; set; }

    public decimal? Proteinas { get; set; }

    public decimal? Carbohidratos { get; set; }

    public decimal? Grasas { get; set; }

    public bool Activo { get; set; } = true;

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public DateTime? FechaActualizacion { get; set; }


    // Navegación

    public Nutricionista Nutricionista { get; set; } = null!;

    public CategoriaAlimento Categoria { get; set; } = null!;

    public ICollection<EquivalenciaAlimento> Equivalencias { get; set; } = [];

    public ICollection<ItemOpcionComida> ItemsComida { get; set; } = [];

    public ICollection<AlternativaItemComida> Alternativas { get; set; } = [];
}