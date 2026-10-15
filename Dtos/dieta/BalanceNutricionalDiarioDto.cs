namespace NutriApi.DTOs.Dietas;

public class BalanceNutricionalDiarioDto
{
    public int DietaId { get; set; }

    public string NombreDieta { get; set; } = string.Empty;

    public bool CalculoCompleto { get; set; }

    public bool ObjetivosCompletos { get; set; }

    // null: no es posible evaluar todavía.
    // true: cumple las cuatro tolerancias.
    // false: al menos una tolerancia se excede.
    public bool? CumpleTolerancias { get; set; }

    public BalanceNutrienteDto Calorias { get; set; } = new();

    public BalanceNutrienteDto Proteinas { get; set; } = new();

    public BalanceNutrienteDto Carbohidratos { get; set; } = new();

    public BalanceNutrienteDto Grasas { get; set; } = new();

    public List<string> Advertencias { get; set; } = new();

    public List<string> Sugerencias { get; set; } = new();
}

public class BalanceNutrienteDto
{
    public decimal? Objetivo { get; set; }

    public decimal? Actual { get; set; }

    // Actual - Objetivo:
    // positivo = exceso
    // negativo = déficit
    public decimal? Diferencia { get; set; }

    public decimal Tolerancia { get; set; }

    // SinObjetivo, DatosIncompletos,
    // DentroDeRango, FueraDeRango
    public string Estado { get; set; } = "SinObjetivo";
}
