namespace NutriApi.Calculos;

public static class CalculadoraNutricional
{
    public static ResultadoCalculoNutricional Calcular(
        decimal cantidadConsumida,
        decimal cantidadBase,
        decimal? caloriasBase,
        decimal? proteinasBase,
        decimal? carbohidratosBase,
        decimal? grasasBase)
    {
        if (cantidadConsumida <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(cantidadConsumida),
                "La cantidad consumida debe ser mayor a cero."
            );
        }

        if (cantidadBase <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(cantidadBase),
                "La cantidad base del alimento debe ser mayor a cero."
            );
        }


        var factor =
            cantidadConsumida /
            cantidadBase;


        return new ResultadoCalculoNutricional
        {
            Cantidad =
                cantidadConsumida,

            Factor =
                Math.Round(
                    factor,
                    4
                ),

            Calorias =
                CalcularValor(
                    caloriasBase,
                    factor
                ),

            Proteinas =
                CalcularValor(
                    proteinasBase,
                    factor
                ),

            Carbohidratos =
                CalcularValor(
                    carbohidratosBase,
                    factor
                ),

            Grasas =
                CalcularValor(
                    grasasBase,
                    factor
                )
        };
    }


    private static decimal? CalcularValor(
        decimal? valorBase,
        decimal factor)
    {
        if (!valorBase.HasValue)
        {
            return null;
        }


        return Math.Round(
            valorBase.Value * factor,
            2
        );
    }
}