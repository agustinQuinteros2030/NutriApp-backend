namespace NutriApi.Calculos;

public class ResultadoCalculoNutricional
{
    public decimal Cantidad { get; set; }

    public decimal Factor { get; set; }

    public decimal? Calorias { get; set; }

    public decimal? Proteinas { get; set; }

    public decimal? Carbohidratos { get; set; }

    public decimal? Grasas { get; set; }
}