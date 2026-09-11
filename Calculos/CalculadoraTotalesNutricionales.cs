using NutriApp.Models.Dietas;

namespace NutriApi.Calculos;

public static class CalculadoraTotalesNutricionales
{
    // ==========================================
    // TOTAL DE UNA OPCIÓN
    // ==========================================

    public static ResultadoTotalesNutricionales
        CalcularOpcion(
            IEnumerable<ItemOpcionComida> items)
    {
        var listaItems =
            items.ToList();


        var resultados =
            listaItems
                .Select(
                    CalcularItem
                )
                .ToList();


        return CrearResultado(
            resultados,
            listaItems.Count,
            0
        );
    }


    // ==========================================
    // TOTAL DE SECCIONES
    // ==========================================

    /*
     * Este método sirve tanto para una comida
     * como para una dieta completa.
     *
     * La regla de negocio es:
     *
     * por cada sección solamente cuenta
     * la opción marcada como predeterminada.
     */

    public static ResultadoTotalesNutricionales
        CalcularSecciones(
            IEnumerable<SeccionComida> secciones)
    {
        var itemsSeleccionados =
            new List<ItemOpcionComida>();


        var seccionesSinPredeterminada =
            0;


        foreach (var seccion in secciones)
        {
            var opcionPredeterminada =
                seccion.Opciones
                    .Where(o =>
                        o.EsPredeterminada
                    )
                    .OrderBy(o =>
                        o.Orden
                    )
                    .ThenBy(o =>
                        o.Id
                    )
                    .FirstOrDefault();


            if (opcionPredeterminada is null)
            {
                seccionesSinPredeterminada++;

                continue;
            }


            itemsSeleccionados.AddRange(
                opcionPredeterminada.Items
            );
        }


        var resultados =
            itemsSeleccionados
                .Select(
                    CalcularItem
                )
                .ToList();


        return CrearResultado(
            resultados,
            itemsSeleccionados.Count,
            seccionesSinPredeterminada
        );
    }


    // ==========================================
    // CÁLCULO DE ITEM
    // ==========================================

    private static ResultadoCalculoNutricional?
        CalcularItem(
            ItemOpcionComida item)
    {
        if (item.Alimento is null)
        {
            return null;
        }


        /*
         * Si por ejemplo:
         *
         * información nutricional = por 100 gramos
         *
         * pero:
         *
         * item = 2 unidades
         *
         * no podemos aplicar proporcionalidad
         * directamente.
         */

        if (item.UnidadMedida !=
            item.Alimento.UnidadBase)
        {
            return null;
        }


        if (item.Alimento.CantidadBase <= 0)
        {
            return null;
        }


        if (item.Cantidad <= 0)
        {
            return null;
        }


        return CalculadoraNutricional.Calcular(
            item.Cantidad,
            item.Alimento.CantidadBase,
            item.Alimento.Calorias,
            item.Alimento.Proteinas,
            item.Alimento.Carbohidratos,
            item.Alimento.Grasas
        );
    }


    // ==========================================
    // CREAR TOTAL
    // ==========================================

    private static ResultadoTotalesNutricionales
        CrearResultado(
            List<ResultadoCalculoNutricional?> resultados,
            int cantidadItems,
            int seccionesSinPredeterminada)
    {
        var itemsSinCalculo =
            resultados.Count(r =>
                r is null
            );


        var calorias =
            SumarCampo(
                resultados,
                r => r.Calorias
            );


        var proteinas =
            SumarCampo(
                resultados,
                r => r.Proteinas
            );


        var carbohidratos =
            SumarCampo(
                resultados,
                r => r.Carbohidratos
            );


        var grasas =
            SumarCampo(
                resultados,
                r => r.Grasas
            );


        var esCompleto =
            itemsSinCalculo == 0
            &&
            seccionesSinPredeterminada == 0
            &&
            calorias.HasValue
            &&
            proteinas.HasValue
            &&
            carbohidratos.HasValue
            &&
            grasas.HasValue;


        return new ResultadoTotalesNutricionales
        {
            Calorias =
                calorias,

            Proteinas =
                proteinas,

            Carbohidratos =
                carbohidratos,

            Grasas =
                grasas,

            CantidadItems =
                cantidadItems,

            ItemsSinCalculo =
                itemsSinCalculo,

            SeccionesSinOpcionPredeterminada =
                seccionesSinPredeterminada,

            EsCompleto =
                esCompleto
        };
    }


    // ==========================================
    // SUMAR UN NUTRIENTE
    // ==========================================

    private static decimal?
        SumarCampo(
            IEnumerable<ResultadoCalculoNutricional?> resultados,
            Func<ResultadoCalculoNutricional, decimal?> selector)
    {
        var lista =
            resultados.ToList();


        /*
         * Si no hay alimentos:
         *
         * total = 0
         */

        if (lista.Count == 0)
        {
            return 0m;
        }


        /*
         * Si existe un item que directamente
         * no pudimos calcular, no devolvemos
         * un total engañoso.
         */

        if (lista.Any(r =>
            r is null))
        {
            return null;
        }


        var valores =
            lista
                .Select(r =>
                    selector(r!)
                )
                .ToList();


        /*
         * Ejemplo:
         *
         * arroz tiene calorías
         * pollo tiene calorías
         * aceite no tiene calorías cargadas
         *
         * No decimos que aceite = 0.
         *
         * El total de calorías pasa a null.
         */

        if (valores.Any(v =>
            !v.HasValue))
        {
            return null;
        }


        return Math.Round(
            valores.Sum(v =>
                v!.Value
            ),
            2
        );
    }
}