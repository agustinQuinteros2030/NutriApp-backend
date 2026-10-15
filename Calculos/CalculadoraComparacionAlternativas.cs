using NutriApi.DTOs.Dietas;
using NutriApp.Models.Dietas;

namespace NutriApi.Calculos;

public static class CalculadoraComparacionAlternativas
{
    private const decimal ToleranciaCalorias = 100m;

    public static ComparacionAlternativasDietaDto Calcular(Dieta dieta)
    {
        ArgumentNullException.ThrowIfNull(dieta);

        var resultado = new ComparacionAlternativasDietaDto
        {
            DietaId = dieta.Id,
            NombreDieta = dieta.Nombre,
            ToleranciaCalorias = ToleranciaCalorias,
        };

        foreach (var comida in dieta.Comidas.OrderBy(c => c.Orden).ThenBy(c => c.Id))
        {
            foreach (var seccion in comida.Secciones.OrderBy(s => s.Orden).ThenBy(s => s.Id))
            {
                var comparacion = CompararSeccion(comida, seccion);

                resultado.Secciones.Add(comparacion);

                if (comparacion.Estado == "FueraDeRango")
                {
                    resultado.Advertencias.Add(
                        $"{comida.Nombre} - {seccion.Nombre}: "
                            + $"las opciones presentan una diferencia "
                            + $"de {comparacion.DiferenciaCalorias:0.##} kcal. "
                            + $"El máximo permitido es "
                            + $"{ToleranciaCalorias:0.##} kcal."
                    );
                }
                else if (comparacion.Estado == "DatosIncompletos")
                {
                    resultado.Advertencias.Add(
                        $"{comida.Nombre} - {seccion.Nombre}: "
                            + "no se pueden comparar todas las opciones "
                            + "porque faltan datos nutricionales."
                    );
                }
                else if (comparacion.Estado == "SinOpciones")
                {
                    resultado.Advertencias.Add(
                        $"{comida.Nombre} - {seccion.Nombre}: "
                            + "la sección no tiene opciones configuradas."
                    );
                }
            }
        }

        var comparables = resultado.Secciones.Where(s => s.CantidadOpciones >= 2).ToList();

        resultado.SeccionesComparadas = comparables.Count(s => s.CumpleTolerancia.HasValue);

        resultado.SeccionesFueraDeRango = comparables.Count(s => s.CumpleTolerancia == false);

        var haySeccionesSinOpciones = resultado.Secciones.Any(s => s.Estado == "SinOpciones");

        var todasEvaluables =
            comparables.Count > 0
            && !haySeccionesSinOpciones
            && comparables.All(s => s.CumpleTolerancia.HasValue);

        resultado.CumpleTolerancia = todasEvaluables
            ? comparables.All(s => s.CumpleTolerancia == true)
            : null;

        return resultado;
    }

    private static ComparacionAlternativasSeccionDto CompararSeccion(
        Comida comida,
        SeccionComida seccion
    )
    {
        var resultado = new ComparacionAlternativasSeccionDto
        {
            ComidaId = comida.Id,
            NombreComida = comida.Nombre,
            SeccionId = seccion.Id,
            NombreSeccion = seccion.Nombre,
        };

        foreach (var opcion in seccion.Opciones.OrderBy(o => o.Orden).ThenBy(o => o.Id))
        {
            // Reutilizamos la calculadora existente.
            var totales = CalculadoraTotalesNutricionales.CalcularOpcion(opcion.Items);

            var calculoCompleto =
                totales.EsCompleto && totales.CantidadItems > 0 && totales.Calorias.HasValue;

            resultado.Opciones.Add(
                new ComparacionAlternativaOpcionDto
                {
                    OpcionId = opcion.Id,
                    Nombre = opcion.Nombre,
                    EsPredeterminada = opcion.EsPredeterminada,

                    // No presentar un valor parcial como definitivo.
                    Calorias = calculoCompleto ? totales.Calorias : null,

                    CalculoCompleto = calculoCompleto,
                }
            );
        }

        resultado.CantidadOpciones = resultado.Opciones.Count;

        if (resultado.CantidadOpciones == 0)
        {
            resultado.Estado = "SinOpciones";
            return resultado;
        }

        if (resultado.CantidadOpciones == 1)
        {
            resultado.Estado = "SinAlternativas";
            return resultado;
        }

        if (resultado.Opciones.Any(o => !o.CalculoCompleto))
        {
            resultado.Estado = "DatosIncompletos";
            return resultado;
        }

        var calorias = resultado.Opciones.Select(o => o.Calorias!.Value).ToList();

        resultado.CaloriasMinimas = calorias.Min();

        resultado.CaloriasMaximas = calorias.Max();

        resultado.DiferenciaCalorias =
            resultado.CaloriasMaximas.Value - resultado.CaloriasMinimas.Value;

        resultado.CumpleTolerancia = resultado.DiferenciaCalorias.Value <= ToleranciaCalorias;

        resultado.Estado = resultado.CumpleTolerancia.Value ? "DentroDeRango" : "FueraDeRango";

        return resultado;
    }
}
