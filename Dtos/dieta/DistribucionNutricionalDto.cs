
namespace NutriApi.DTOs.Dietas;

public class DistribucionNutricionalDto
{
    public int DietaId { get; set; }

    public string NombreDieta { get; set; } = string.Empty;

    public int CantidadComidas { get; set; }

    public BalanceDistribucionNutrienteDto Calorias { get; set; } = new();

    public BalanceDistribucionNutrienteDto Proteinas { get; set; } = new();

    public BalanceDistribucionNutrienteDto Carbohidratos { get; set; } = new();

    public BalanceDistribucionNutrienteDto Grasas { get; set; } = new();

    public List<ObjetivosComidaDistribucionDto> Comidas { get; set; } = new();

    public bool TieneExcesos =>
        Calorias.ExcedeObjetivo ||
        Proteinas.ExcedeObjetivo ||
        Carbohidratos.ExcedeObjetivo ||
        Grasas.ExcedeObjetivo;

    public bool DistribucionCompleta =>
        Calorias.DistribucionCompleta &&
        Proteinas.DistribucionCompleta &&
        Carbohidratos.DistribucionCompleta &&
        Grasas.DistribucionCompleta;
}

public class BalanceDistribucionNutrienteDto
{
    public decimal? ObjetivoDiario { get; set; }

    public decimal TotalAsignado { get; set; }

    // Positivo: falta asignar.
    // Negativo: se superó el objetivo.
    // Null: no existe objetivo diario.

    public decimal? Diferencia { get; set; }

    public decimal? Excedente { get; set; }

    public int ComidasConObjetivo { get; set; }

    public int ComidasSinObjetivo { get; set; }

    public bool ExcedeObjetivo { get; set; }

    public bool DistribucionCompleta { get; set; }
}

public class ObjetivosComidaDistribucionDto
{
    public int ComidaId { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string Tipo { get; set; } = string.Empty;

    public int Orden { get; set; }

    public decimal? ObjetivoCalorias { get; set; }

    public decimal? ObjetivoProteinas { get; set; }

    public decimal? ObjetivoCarbohidratos { get; set; }

    public decimal? ObjetivoGrasas { get; set; }
}
