
using NutriApi.DTOs.Dietas;
using NutriApp.Enums;
using NutriApp.Models.Dietas;

namespace NutriApi.Calculos;

public static class SimuladorAjustesPorciones
{
    public static SimulacionAjustesPorcionesDto Calcular(Comida comida)
    {
        ArgumentNullException.ThrowIfNull(comida);

        var resultado = new SimulacionAjustesPorcionesDto
        {
            DietaId = comida.DietaId,
            ComidaId = comida.Id,
            NombreComida = comida.Nombre,

            Objetivos = new ObjetivosSimulacionDto
            {
                Calorias = comida.ObjetivoCalorias,
                Proteinas = comida.ObjetivoProteinas,
                Carbohidratos = comida.ObjetivoCarbohidratos,
                Grasas = comida.ObjetivoGrasas
            }
        };

        if (comida.Secciones.Count == 0)
        {
            resultado.Motivo = "La comida no tiene secciones.";
            return resultado;
        }

        // Solamente se simulan los alimentos de
        // las opciones predeterminadas.

        var itemsSeleccionados = new List<ItemOpcionComida>();

        foreach (var seccion in comida.Secciones)
        {
            var predeterminada = seccion.Opciones
                .Where(o => o.EsPredeterminada)
                .OrderBy(o => o.Orden)
                .ThenBy(o => o.Id)
                .FirstOrDefault();

            if (predeterminada is null)
            {
                resultado.Motivo =
                    $"La sección '{seccion.Nombre}' no tiene " +
                    "una opción predeterminada.";

                return resultado;
            }

            if (!predeterminada.Items.Any())
            {
                resultado.Motivo =
                    $"La opción predeterminada de " +
                    $"'{seccion.Nombre}' no tiene alimentos.";

                return resultado;
            }

            itemsSeleccionados.AddRange(predeterminada.Items);
        }

        // Reutilizamos el cálculo nutricional existente.

        var totales =
            CalculadoraTotalesNutricionales.CalcularSecciones(
                comida.Secciones
            );

        if (!totales.EsCompleto
            || !totales.Calorias.HasValue
            || !totales.Proteinas.HasValue
            || !totales.Carbohidratos.HasValue
            || !totales.Grasas.HasValue)
        {
            resultado.Motivo =
                "No se pueden simular ajustes porque " +
                "faltan datos nutricionales o hay " +
                "unidades incompatibles.";

            return resultado;
        }

        var actuales = new NutrientesSimulacionDto
        {
            Calorias = totales.Calorias.Value,
            Proteinas = totales.Proteinas.Value,
            Carbohidratos = totales.Carbohidratos.Value,
            Grasas = totales.Grasas.Value
        };

        resultado.TotalesActuales = actuales;

        var objetivos = resultado.Objetivos;

        if (!objetivos.Calorias.HasValue
            && !objetivos.Proteinas.HasValue
            && !objetivos.Carbohidratos.HasValue
            && !objetivos.Grasas.HasValue)
        {
            resultado.Motivo =
                "Configurá al menos un objetivo nutricional " +
                "para esta comida.";

            return resultado;
        }

        resultado.EsCalculable = true;

        var puntajeInicial = CalcularPuntaje(actuales, objetivos);

        if (puntajeInicial == 0m)
        {
            resultado.Motivo =
                "Los nutrientes actuales coinciden con " +
                "todos los objetivos configurados.";

            return resultado;
        }

        var propuestas = new List<(
            PropuestaAjustePorcionDto Propuesta,
            decimal Mejora
        )>();

        foreach (var item in itemsSeleccionados)
        {
            var alimento = item.Alimento;

            // Evitamos conversiones que todavía
            // no están implementadas.

            if (alimento is null
                || alimento.CantidadBase <= 0m
                || item.Cantidad <= 0m
                || item.UnidadMedida != alimento.UnidadBase
                || !alimento.Calorias.HasValue
                || !alimento.Proteinas.HasValue
                || !alimento.Carbohidratos.HasValue
                || !alimento.Grasas.HasValue)
            {
                continue;
            }

            var paso = ObtenerPaso(item.UnidadMedida);

            if (!paso.HasValue)
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

            decimal mejorPuntaje = puntajeInicial;

            PropuestaAjustePorcionDto? mejorPropuesta = null;

            // Exploramos variaciones desde -30% hasta +30%.
            // Luego ajustamos al incremento de la unidad.

            for (var indice = -10; indice <= 10; indice++)
            {
                if (indice == 0)
                {
                    continue;
                }

                var factor = 1m + indice * 0.03m;

                var cantidadNueva = Math.Round(
                    item.Cantidad * factor / paso.Value,
                    0,
                    MidpointRounding.AwayFromZero
                ) * paso.Value;

                if (cantidadNueva <= 0m
                    || cantidadNueva == item.Cantidad)
                {
                    continue;
                }

                // El redondeo a incrementos puede superar
                // ligeramente el porcentaje explorado.
                // Limitamos el cambio a 30%, como máximo,
                // permitiendo al menos un incremento.

                var limiteCambio = Math.Max(
                    paso.Value,
                    item.Cantidad * 0.30m
                );

                if (Math.Abs(cantidadNueva - item.Cantidad)
                    > limiteCambio)
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

                // Reemplazamos matemáticamente solamente
                // el aporte de este item.
                // Los demás alimentos permanecen iguales.

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
                        actuales.Grasas
                        - aporteAnterior.Grasas!.Value
                        + aporteNuevo.Grasas!.Value,
                        2
                    )
                };

                var puntajeNuevo =
                    CalcularPuntaje(simulados, objetivos);

                // Solamente guardamos cambios que mejoren
                // el balance global de objetivos disponibles.

                if (puntajeNuevo >= mejorPuntaje)
                {
                    continue;
                }

                mejorPuntaje = puntajeNuevo;

                mejorPropuesta = new PropuestaAjustePorcionDto
                {
                    ItemId = item.Id,
                    AlimentoId = alimento.Id,
                    NombreAlimento = alimento.Nombre,

                    CantidadActual = item.Cantidad,
                    CantidadSugerida = cantidadNueva,

                    UnidadMedida = item.UnidadMedida.ToString(),

                    TotalesSimulados = simulados
                };
            }

            if (mejorPropuesta is not null)
            {
                propuestas.Add((
                    mejorPropuesta,
                    puntajeInicial - mejorPuntaje
                ));
            }
        }

        resultado.Propuestas = propuestas
            .OrderByDescending(p => p.Mejora)
            .Take(5)
            .Select(p => p.Propuesta)
            .ToList();

        if (resultado.Propuestas.Count == 0)
        {
            resultado.Motivo =
                "No se encontraron ajustes individuales " +
                "que mejoren el balance dentro del " +
                "rango de cantidades explorado.";
        }

        return resultado;
    }

    // ==========================================
    // PASOS DE CANTIDAD
    // ==========================================

    private static decimal? ObtenerPaso(UnidadMedida unidad)
    {
        return unidad switch
        {
            UnidadMedida.Gramos => 5m,
            UnidadMedida.Mililitros => 50m,
            UnidadMedida.Unidad => 0.5m,
            UnidadMedida.Porcion => 0.5m,
            _ => null
        };
    }

    // ==========================================
    // EVALUACIÓN DEL BALANCE
    // ==========================================

    private static decimal CalcularPuntaje(
        NutrientesSimulacionDto actual,
        ObjetivosSimulacionDto objetivo)
    {
        return ErrorRelativo(
                actual.Calorias,
                objetivo.Calorias)
            + ErrorRelativo(
                actual.Proteinas,
                objetivo.Proteinas)
            + ErrorRelativo(
                actual.Carbohidratos,
                objetivo.Carbohidratos)
            + ErrorRelativo(
                actual.Grasas,
                objetivo.Grasas);
    }

    private static decimal ErrorRelativo(
        decimal actual,
        decimal? objetivo)
    {
        if (!objetivo.HasValue)
        {
            return 0m;
        }

        var escala = Math.Max(1m, objetivo.Value);

        return Math.Abs(actual - objetivo.Value) / escala;
    }
}

