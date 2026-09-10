namespace NutriApi.Calculos
{


    public static class CalculadoraEquivalencias
    {
        /// <summary>
        /// Calcula la cantidad equivalente de un alimento destino
        /// a partir de una cantidad de un alimento origen.
        ///
        /// Ejemplo:
        ///
        /// Arroz = 100 g
        /// Papa  = 400 g
        ///
        /// Si el paciente consume 300 g de arroz:
        ///
        /// factor = 300 / 100 = 3
        /// papa   = 400 * 3 = 1200 g
        /// </summary>
        public static decimal CalcularCantidadDestino(
            decimal cantidadOrigen,
            decimal cantidadEquivalenteOrigen,
            decimal cantidadEquivalenteDestino)
        {
            if (cantidadOrigen <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(cantidadOrigen),
                    "La cantidad de origen debe ser mayor a cero."
                );
            }

            if (cantidadEquivalenteOrigen <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(cantidadEquivalenteOrigen),
                    "La cantidad equivalente de origen debe ser mayor a cero."
                );
            }

            if (cantidadEquivalenteDestino <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(cantidadEquivalenteDestino),
                    "La cantidad equivalente de destino debe ser mayor a cero."
                );
            }


            var factor =
                cantidadOrigen /
                cantidadEquivalenteOrigen;


            var cantidadDestino =
                cantidadEquivalenteDestino *
                factor;


            return Math.Round(
                cantidadDestino,
                2
            );
        }
    }
}

