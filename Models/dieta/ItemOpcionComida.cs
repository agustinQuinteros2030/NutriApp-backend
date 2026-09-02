using NutriApi.Models.dieta;
using NutriApp.Enums;
using NutriApp.Models.Alimentos;
using System.Collections.Generic;

namespace NutriApp.Models.Dietas;

public class ItemOpcionComida
{
    public int Id { get; set; }

    public int OpcionSeccionComidaId { get; set; }

    public int AlimentoId { get; set; }

    public decimal Cantidad { get; set; }

    public UnidadMedida UnidadMedida { get; set; }

    public string? Indicaciones { get; set; }

    public int Orden { get; set; }


    // Navegación

    public OpcionSeccionComida OpcionSeccionComida { get; set; } = null!;

    public Alimento Alimento { get; set; } = null!;

    public ICollection<AlternativaItemComida> Alternativas { get; set; } = [];
}