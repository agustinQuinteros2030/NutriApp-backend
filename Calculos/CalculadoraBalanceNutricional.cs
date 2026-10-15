using NutriApi.DTOs.Dietas;

namespace NutriApi.Calculos;

public static class CalculadoraBalanceNutricional
{
    private const decimal ToleranciaCalorias = 200m;
    private const decimal ToleranciaMacros = 10m;

    public static BalanceNutricionalDiarioDto Calcular(DietaDetalleDto dieta)
    {
        ArgumentNullException.ThrowIfNull(dieta);

        var totales = dieta.Totales;

        // Validamos que la dieta tenga comidas,
        // alimentos y datos nutricionales completos.

        var calculoCompleto =
            dieta.CantidadComidas > 0
            && totales.CantidadItems > 0
            && totales.EsCompleto
            && totales.Calorias.HasValue
            && totales.Proteinas.HasValue
            && totales.Carbohidratos.HasValue
            && totales.Grasas.HasValue;

        var resultado = new BalanceNutricionalDiarioDto
        {
            DietaId = dieta.Id,

            NombreDieta = dieta.Nombre,

            CalculoCompleto = calculoCompleto,

            Calorias = Evaluar(
                dieta.ObjetivoCalorias,
                totales.Calorias,
                ToleranciaCalorias,
                calculoCompleto
            ),

            Proteinas = Evaluar(
                dieta.ObjetivoProteinas,
                totales.Proteinas,
                ToleranciaMacros,
                calculoCompleto
            ),

            Carbohidratos = Evaluar(
                dieta.ObjetivoCarbohidratos,
                totales.Carbohidratos,
                ToleranciaMacros,
                calculoCompleto
            ),

            Grasas = Evaluar(
                dieta.ObjetivoGrasas,
                totales.Grasas,
                ToleranciaMacros,
                calculoCompleto
            ),
        };

        // ==========================================
        // VALIDACIÓN GLOBAL
        // ==========================================

        var balances = new[]
        {
            resultado.Calorias,
            resultado.Proteinas,
            resultado.Carbohidratos,
            resultado.Grasas,
        };

        resultado.ObjetivosCompletos = balances.All(b => b.Objetivo.HasValue);

        // Solo consideramos evaluable una dieta
        // cuando podemos comparar los cuatro nutrientes.

        var todosEvaluables = balances.All(b =>
            b.Estado == "DentroDeRango" || b.Estado == "FueraDeRango"
        );

        resultado.CumpleTolerancias = todosEvaluables
            ? balances.All(b => b.Estado == "DentroDeRango")
            : null;

        // ==========================================
        // ADVERTENCIAS GENERALES
        // ==========================================

        if (!calculoCompleto)
        {
            resultado.Advertencias.Add(
                "El cálculo nutricional está incompleto. "
                    + "Revisá las opciones predeterminadas "
                    + "y los datos de los alimentos."
            );
        }

        if (!resultado.ObjetivosCompletos)
        {
            resultado.Advertencias.Add(
                "Faltan objetivos nutricionales diarios " + "por configurar."
            );
        }

        // ==========================================
        // ADVERTENCIAS POR NUTRIENTE
        // ==========================================

        AgregarAdvertencia(
            resultado,
            "Calorías",
            "kcal",
            resultado.Calorias,
            "Revisar las porciones y los alimentos " + "que más aportan calorías."
        );

        AgregarAdvertencia(
            resultado,
            "Proteínas",
            "g",
            resultado.Proteinas,
            "Revisar las cantidades de las fuentes " + "de proteínas."
        );

        AgregarAdvertencia(
            resultado,
            "Carbohidratos",
            "g",
            resultado.Carbohidratos,
            "Revisar las cantidades de las fuentes " + "de carbohidratos."
        );

        AgregarAdvertencia(
            resultado,
            "Grasas",
            "g",
            resultado.Grasas,
            "Revisar las cantidades de las fuentes " + "de grasas."
        );

        return resultado;
    }

    // ==========================================
    // EVALUAR UN NUTRIENTE
    // ==========================================

    private static BalanceNutrienteDto Evaluar(
        decimal? objetivo,
        decimal? actual,
        decimal tolerancia,
        bool calculoCompleto
    )
    {
        var balance = new BalanceNutrienteDto { Objetivo = objetivo, Tolerancia = tolerancia };

        // No existe un objetivo configurado.

        if (!objetivo.HasValue)
        {
            balance.Estado = "SinObjetivo";
            return balance;
        }

        // No tenemos información suficiente
        // para realizar una comparación confiable.

        if (!calculoCompleto || !actual.HasValue)
        {
            balance.Estado = "DatosIncompletos";
            return balance;
        }

        // ==========================================
        // CALCULAR DIFERENCIA
        // ==========================================

        balance.Actual = actual.Value;

        balance.Diferencia = actual.Value - objetivo.Value;

        // Positivo: superó el objetivo.
        // Negativo: está por debajo.
        // Cero: coincide exactamente.

        balance.Estado =
            Math.Abs(balance.Diferencia.Value) <= tolerancia ? "DentroDeRango" : "FueraDeRango";

        return balance;
    }

    // ==========================================
    // GENERAR ADVERTENCIAS
    // ==========================================

    private static void AgregarAdvertencia(
        BalanceNutricionalDiarioDto resultado,
        string nombre,
        string unidad,
        BalanceNutrienteDto balance,
        string sugerencia
    )
    {
        // Si el nutriente está dentro del rango,
        // o no puede evaluarse, no generamos
        // advertencia de exceso o déficit.

        if (balance.Estado != "FueraDeRango" || !balance.Diferencia.HasValue)
        {
            return;
        }

        var diferencia = balance.Diferencia.Value;

        var direccion = diferencia > 0 ? "por encima" : "por debajo";

        resultado.Advertencias.Add(
            $"{nombre}: {Math.Abs(diferencia):0.##} "
                + $"{unidad} {direccion} del objetivo. "
                + $"Tolerancia: ±{balance.Tolerancia:0.##} {unidad}."
        );

        resultado.Sugerencias.Add(sugerencia);
    }
}
