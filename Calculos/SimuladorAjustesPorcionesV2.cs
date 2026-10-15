using NutriApi.DTOs.Dietas;
using NutriApp.Enums;
using NutriApp.Models.Dietas;

namespace NutriApi.Calculos;

public static class SimuladorAjustesPorcionesV2
{
    private const decimal VariacionMaxima = 0.30m;
    private const decimal PesoPrioritario = 4m;
    private const decimal MejoraMinima = 0.00000001m;

    public static SimulacionAjustesPorcionesV2Dto Calcular(
        Comida comida,
        PrioridadAjusteNutricional prioridad
    )
    {
        ArgumentNullException.ThrowIfNull(comida);

        if (!Enum.IsDefined(prioridad))
        {
            throw new ArgumentOutOfRangeException(nameof(prioridad));
        }

        var resultado = new SimulacionAjustesPorcionesV2Dto
        {
            DietaId = comida.DietaId,
            ComidaId = comida.Id,
            NombreComida = comida.Nombre,
            PrioridadAplicada = prioridad.ToString(),

            Objetivos = new ObjetivosSimulacionDto
            {
                Calorias = comida.ObjetivoCalorias,
                Proteinas = comida.ObjetivoProteinas,
                Carbohidratos = comida.ObjetivoCarbohidratos,
                Grasas = comida.ObjetivoGrasas,
            },
        };

        if (comida.Secciones.Count == 0)
        {
            resultado.Motivo = "La comida no tiene secciones.";
            return resultado;
        }

        var items = new List<ItemOpcionComida>();

        foreach (var seccion in comida.Secciones)
        {
            var predeterminadas = seccion.Opciones.Where(o => o.EsPredeterminada).ToList();

            if (predeterminadas.Count != 1)
            {
                resultado.Motivo =
                    $"La sección '{seccion.Nombre}' debe tener "
                    + "exactamente una opción predeterminada.";

                return resultado;
            }

            var seleccionados = predeterminadas[0].Items;

            if (seleccionados.Count == 0)
            {
                resultado.Motivo =
                    $"La opción predeterminada de " + $"'{seccion.Nombre}' no tiene alimentos.";

                return resultado;
            }

            items.AddRange(seleccionados);
        }

        var totales = CalculadoraTotalesNutricionales.CalcularSecciones(comida.Secciones);

        if (
            !totales.EsCompleto
            || !totales.Calorias.HasValue
            || !totales.Proteinas.HasValue
            || !totales.Carbohidratos.HasValue
            || !totales.Grasas.HasValue
        )
        {
            resultado.Motivo =
                "Los datos nutricionales están incompletos " + "o existen unidades incompatibles.";

            return resultado;
        }

        var actuales = new NutrientesSimulacionDto
        {
            Calorias = totales.Calorias.Value,
            Proteinas = totales.Proteinas.Value,
            Carbohidratos = totales.Carbohidratos.Value,
            Grasas = totales.Grasas.Value,
        };

        resultado.TotalesActuales = actuales;

        var objetivos = resultado.Objetivos;

        if (
            !objetivos.Calorias.HasValue
            && !objetivos.Proteinas.HasValue
            && !objetivos.Carbohidratos.HasValue
            && !objetivos.Grasas.HasValue
        )
        {
            resultado.Motivo = "Configurá al menos un objetivo de la comida.";

            return resultado;
        }

        resultado.EsCalculable = true;

        var puntajeInicial = CalcularPuntaje(actuales, actuales, objetivos, prioridad);

        if (puntajeInicial == 0m)
        {
            resultado.Motivo = "Los aportes coinciden con todos " + "los objetivos configurados.";

            return resultado;
        }

        var candidatas = new List<(PropuestaAjustePorcionV2Dto Propuesta, decimal Puntaje)>();

        foreach (var item in items)
        {
            var alimento = item.Alimento;

            if (
                alimento is null
                || item.Cantidad <= 0m
                || alimento.CantidadBase <= 0m
                || item.UnidadMedida != alimento.UnidadBase
                || !alimento.Calorias.HasValue
                || !alimento.Proteinas.HasValue
                || !alimento.Carbohidratos.HasValue
                || !alimento.Grasas.HasValue
                || alimento.Calorias.Value < 0m
                || alimento.Proteinas.Value < 0m
                || alimento.Carbohidratos.Value < 0m
                || alimento.Grasas.Value < 0m
            )
            {
                continue;
            }

            var paso = ObtenerPaso(item.UnidadMedida);

            if (paso <= 0m)
            {
                continue;
            }

            var aporteAnterior = CalculadoraNutricional.Calcular(
                item.Cantidad,
                alimento.CantidadBase,
                alimento.Calorias,
                alimento.Proteinas,
                alimento.Carbohidratos,
                alimento.Grasas
            );

            // Variación máxima del 30%.
            // Para gramos, ml y porciones admitimos al menos
            // un incremento cuando la cantidad es pequeña.
            // Para unidades enteras mantenemos el 30%.

            var limiteCambio =
                item.UnidadMedida == UnidadMedida.Unidad
                    ? item.Cantidad * VariacionMaxima
                    : Math.Max(paso, item.Cantidad * VariacionMaxima);

            var cantidadRedondeada =
                Math.Round(item.Cantidad / paso, 0, MidpointRounding.AwayFromZero) * paso;

            var mejorPuntaje = puntajeInicial;

            PropuestaAjustePorcionV2Dto? mejorPropuesta = null;

            // Máximo 41 candidatos por alimento.
            // Se evita una búsqueda enorme en porciones grandes.

            for (var indice = -20; indice <= 20; indice++)
            {
                var cantidadNueva = cantidadRedondeada + indice * paso;

                if (cantidadNueva <= 0m || cantidadNueva == item.Cantidad)
                {
                    continue;
                }

                if (Math.Abs(cantidadNueva - item.Cantidad) > limiteCambio)
                {
                    continue;
                }

                var aporteNuevo = CalculadoraNutricional.Calcular(
                    cantidadNueva,
                    alimento.CantidadBase,
                    alimento.Calorias,
                    alimento.Proteinas,
                    alimento.Carbohidratos,
                    alimento.Grasas
                );

                var simulados = new NutrientesSimulacionDto
                {
                    Calorias = Math.Round(
                        actuales.Calorias
                            - aporteAnterior.Calorias!.Value
                            + aporteNuevo.Calorias!.Value,
                        2
                    ),

                    Proteinas = Math.Round(
                        actuales.Proteinas
                            - aporteAnterior.Proteinas!.Value
                            + aporteNuevo.Proteinas!.Value,
                        2
                    ),

                    Carbohidratos = Math.Round(
                        actuales.Carbohidratos
                            - aporteAnterior.Carbohidratos!.Value
                            + aporteNuevo.Carbohidratos!.Value,
                        2
                    ),

                    Grasas = Math.Round(
                        actuales.Grasas - aporteAnterior.Grasas!.Value + aporteNuevo.Grasas!.Value,
                        2
                    ),
                };

                if (
                    simulados.Calorias < 0m
                    || simulados.Proteinas < 0m
                    || simulados.Carbohidratos < 0m
                    || simulados.Grasas < 0m
                )
                {
                    continue;
                }

                var puntajeNuevo = CalcularPuntaje(simulados, actuales, objetivos, prioridad);

                if (puntajeNuevo >= mejorPuntaje - MejoraMinima)
                {
                    continue;
                }

                mejorPuntaje = puntajeNuevo;

                var accion = cantidadNueva < item.Cantidad ? "Reducir" : "Aumentar";

                mejorPropuesta = new PropuestaAjustePorcionV2Dto
                {
                    ItemId = item.Id,
                    AlimentoId = alimento.Id,
                    NombreAlimento = alimento.Nombre,
                    CantidadActual = item.Cantidad,
                    CantidadSugerida = cantidadNueva,
                    UnidadMedida = item.UnidadMedida.ToString(),

                    MejoraPuntaje = Math.Round(puntajeInicial - puntajeNuevo, 6),

                    Explicacion =
                        $"{accion} {alimento.Nombre} de "
                        + $"{item.Cantidad:0.##} a "
                        + $"{cantidadNueva:0.##} "
                        + $"{item.UnidadMedida} mejora la "
                        + "puntuación nutricional ponderada "
                        + $"con prioridad {prioridad}.",

                    TotalesSimulados = simulados,

                    Impactos = CrearImpactos(actuales, simulados, objetivos),
                };
            }

            if (mejorPropuesta is not null)
            {
                candidatas.Add((mejorPropuesta, mejorPuntaje));
            }
        }

        resultado.Propuestas = candidatas
            .OrderBy(c => c.Puntaje)
            .Take(5)
            .Select(c => c.Propuesta)
            .ToList();

        if (resultado.Propuestas.Count == 0)
        {
            resultado.Motivo =
                "No se encontraron cambios individuales "
                + "que mejoren los objetivos dentro "
                + "de las cantidades exploradas.";
        }

        return resultado;
    }

    // ==========================================
    // INCREMENTOS DE CANTIDAD
    // ==========================================

    private static decimal ObtenerPaso(UnidadMedida unidad)
    {
        return unidad switch
        {
            UnidadMedida.Gramos => 5m,
            UnidadMedida.Mililitros => 50m,
            UnidadMedida.Unidad => 1m,
            UnidadMedida.Porcion => 0.5m,
            _ => 0m,
        };
    }

    // ==========================================
    // PUNTAJE CON PRIORIDAD
    // ==========================================

    private static decimal CalcularPuntaje(
        NutrientesSimulacionDto evaluados,
        NutrientesSimulacionDto iniciales,
        ObjetivosSimulacionDto objetivos,
        PrioridadAjusteNutricional prioridad
    )
    {
        return CalcularError(evaluados.Calorias, iniciales.Calorias, objetivos.Calorias)
                * Peso(prioridad, PrioridadAjusteNutricional.Calorias)
            + CalcularError(evaluados.Proteinas, iniciales.Proteinas, objetivos.Proteinas)
                * Peso(prioridad, PrioridadAjusteNutricional.Proteinas)
            + CalcularError(
                evaluados.Carbohidratos,
                iniciales.Carbohidratos,
                objetivos.Carbohidratos
            ) * Peso(prioridad, PrioridadAjusteNutricional.Carbohidratos)
            + CalcularError(evaluados.Grasas, iniciales.Grasas, objetivos.Grasas)
                * Peso(prioridad, PrioridadAjusteNutricional.Grasas);
    }

    private static decimal Peso(
        PrioridadAjusteNutricional prioridad,
        PrioridadAjusteNutricional nutriente
    )
    {
        return prioridad == nutriente ? PesoPrioritario : 1m;
    }

    private static decimal CalcularError(decimal evaluado, decimal inicial, decimal? objetivo)
    {
        if (!objetivo.HasValue)
        {
            return 0m;
        }

        // La escala se mantiene fija durante
        // la simulación de esta comida.
        // Así se comparan candidatos coherentemente.

        var escala = Math.Max(1m, Math.Max(Math.Abs(inicial), Math.Abs(objetivo.Value)));

        return Math.Abs(evaluado - objetivo.Value) / escala;
    }

    // ==========================================
    // IMPACTO POR NUTRIENTE
    // ==========================================

    private static List<ImpactoAjusteNutrienteDto> CrearImpactos(
        NutrientesSimulacionDto actuales,
        NutrientesSimulacionDto simulados,
        ObjetivosSimulacionDto objetivos
    )
    {
        return new List<ImpactoAjusteNutrienteDto>
        {
            CrearImpacto("Calorias", actuales.Calorias, simulados.Calorias, objetivos.Calorias),
            CrearImpacto("Proteinas", actuales.Proteinas, simulados.Proteinas, objetivos.Proteinas),
            CrearImpacto(
                "Carbohidratos",
                actuales.Carbohidratos,
                simulados.Carbohidratos,
                objetivos.Carbohidratos
            ),
            CrearImpacto("Grasas", actuales.Grasas, simulados.Grasas, objetivos.Grasas),
        };
    }

    private static ImpactoAjusteNutrienteDto CrearImpacto(
        string nutriente,
        decimal actual,
        decimal simulado,
        decimal? objetivo
    )
    {
        var impacto = new ImpactoAjusteNutrienteDto
        {
            Nutriente = nutriente,
            Objetivo = objetivo,
            Actual = actual,
            Simulado = simulado,
        };

        if (!objetivo.HasValue)
        {
            impacto.Estado = "SinObjetivo";
            return impacto;
        }

        impacto.DiferenciaAnterior = actual - objetivo.Value;

        impacto.DiferenciaSimulada = simulado - objetivo.Value;

        var errorAnterior = Math.Abs(impacto.DiferenciaAnterior.Value);

        var errorSimulado = Math.Abs(impacto.DiferenciaSimulada.Value);

        impacto.Estado =
            errorSimulado < errorAnterior ? "Mejora"
            : errorSimulado > errorAnterior ? "Empeora"
            : "SinCambio";

        return impacto;
    }
}
