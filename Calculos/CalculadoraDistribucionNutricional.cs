using NutriApi.DTOs.Dietas;
using NutriApp.Models.Dietas;

namespace NutriApi.Calculos;

public static class CalculadoraDistribucionNutricional
{
    public static DistribucionNutricionalDto Calcular(Dieta dieta)
    {
        ArgumentNullException.ThrowIfNull(dieta);

        var comidas = dieta.Comidas.OrderBy(c => c.Orden).ThenBy(c => c.Id).ToList();

        return new DistribucionNutricionalDto
        {
            DietaId = dieta.Id,

            NombreDieta = dieta.Nombre,

            CantidadComidas = comidas.Count,

            Calorias = CalcularBalance(
                dieta.ObjetivoCalorias,
                comidas.Select(c => c.ObjetivoCalorias)
            ),

            Proteinas = CalcularBalance(
                dieta.ObjetivoProteinas,
                comidas.Select(c => c.ObjetivoProteinas)
            ),

            Carbohidratos = CalcularBalance(
                dieta.ObjetivoCarbohidratos,
                comidas.Select(c => c.ObjetivoCarbohidratos)
            ),

            Grasas = CalcularBalance(dieta.ObjetivoGrasas, comidas.Select(c => c.ObjetivoGrasas)),

            Comidas = comidas
                .Select(c => new ObjetivosComidaDistribucionDto
                {
                    ComidaId = c.Id,

                    Nombre = c.Nombre,

                    Tipo = c.Tipo.ToString(),

                    Orden = c.Orden,

                    ObjetivoCalorias = c.ObjetivoCalorias,

                    ObjetivoProteinas = c.ObjetivoProteinas,

                    ObjetivoCarbohidratos = c.ObjetivoCarbohidratos,

                    ObjetivoGrasas = c.ObjetivoGrasas,
                })
                .ToList(),
        };
    }

    private static BalanceDistribucionNutrienteDto CalcularBalance(
        decimal? objetivoDiario,
        IEnumerable<decimal?> objetivosComidas
    )
    {
        var valores = objetivosComidas.ToList();

        var cantidadConObjetivo = valores.Count(v => v.HasValue);

        var cantidadSinObjetivo = valores.Count - cantidadConObjetivo;

        var totalAsignado = valores.Sum(v => v ?? 0m);

        decimal? diferencia = objetivoDiario.HasValue ? objetivoDiario.Value - totalAsignado : null;

        var excedeObjetivo = diferencia.HasValue && diferencia.Value < 0m;

        var excedente = diferencia.HasValue ? Math.Max(0m, -diferencia.Value) : (decimal?)null;

        var distribucionCompleta =
            objetivoDiario.HasValue
            && valores.Count > 0
            && cantidadSinObjetivo == 0
            && diferencia == 0m;

        return new BalanceDistribucionNutrienteDto
        {
            ObjetivoDiario = objetivoDiario,

            TotalAsignado = totalAsignado,

            Diferencia = diferencia,

            Excedente = excedente,

            ComidasConObjetivo = cantidadConObjetivo,

            ComidasSinObjetivo = cantidadSinObjetivo,

            ExcedeObjetivo = excedeObjetivo,

            DistribucionCompleta = distribucionCompleta,
        };
    }
}
