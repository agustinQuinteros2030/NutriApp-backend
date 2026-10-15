using NutriApi.DTOs.Dietas;
using NutriApp.Enums;
using NutriApp.Enums.Alimentos;
using NutriApp.Models.Alimentos;
using NutriApp.Models.Dietas;

namespace NutriApi.Calculos;

public sealed record CandidatoSustitucion(
    ItemOpcionComida ItemOrigen,
    Alimento AlimentoDestino,
    int GrupoEquivalenciaId,
    CriterioEquivalencia Criterio
);

public static class SimuladorSustitucionesNutricionales
{
    private const decimal Tolerancia = 0.00000001m;

    public static SimulacionSustitucionesDto Calcular(
        Comida comida,
        IEnumerable<CandidatoSustitucion> candidatos
    )
    {
        ArgumentNullException.ThrowIfNull(comida);
        ArgumentNullException.ThrowIfNull(candidatos);

        var resultado = new SimulacionSustitucionesDto
        {
            DietaId = comida.DietaId,
            ComidaId = comida.Id,
            NombreComida = comida.Nombre,

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

        // Únicamente los ítems de opciones predeterminadas.
        var itemsSeleccionados = new Dictionary<int, ItemOpcionComida>();

        foreach (var seccion in comida.Secciones)
        {
            var opciones = seccion.Opciones.Where(o => o.EsPredeterminada).ToList();

            if (opciones.Count != 1)
            {
                resultado.Motivo =
                    $"La sección '{seccion.Nombre}' debe tener "
                    + "exactamente una opción predeterminada.";

                return resultado;
            }

            if (opciones[0].Items.Count == 0)
            {
                resultado.Motivo =
                    $"La opción predeterminada de '{seccion.Nombre}' " + "no tiene alimentos.";

                return resultado;
            }

            foreach (var item in opciones[0].Items)
            {
                if (!itemsSeleccionados.TryAdd(item.Id, item))
                {
                    resultado.Motivo = "Existen ítems duplicados.";
                    return resultado;
                }
            }
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
            resultado.Motivo = "Los valores nutricionales de la comida " + "están incompletos.";

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
        resultado.EsCalculable = true;

        var puntajeOriginal = CalcularPuntaje(actuales, actuales, resultado.Objetivos);

        foreach (var candidato in candidatos)
        {
            if (
                candidato.ItemOrigen is null
                || candidato.AlimentoDestino is null
                || !itemsSeleccionados.TryGetValue(candidato.ItemOrigen.Id, out var item)
            )
            {
                continue;
            }

            var origen = item.Alimento;
            var destino = candidato.AlimentoDestino;

            if (
                origen is null
                || origen.Id == destino.Id
                || item.Cantidad <= 0m
                || item.UnidadMedida != origen.UnidadBase
                || !DatosValidos(origen)
                || !DatosValidos(destino)
                || !Enum.IsDefined(candidato.Criterio)
            )
            {
                continue;
            }

            var nutrienteOrigen = CalculadoraEquivalencias.ObtenerValorCriterio(
                origen,
                candidato.Criterio
            );

            var nutrienteDestino = CalculadoraEquivalencias.ObtenerValorCriterio(
                destino,
                candidato.Criterio
            );

            if (
                !nutrienteOrigen.HasValue
                || !nutrienteDestino.HasValue
                || nutrienteOrigen.Value <= 0m
                || nutrienteDestino.Value <= 0m
            )
            {
                continue;
            }

            // Reutilizamos el motor de equivalencias existente.
            var cantidadTeorica = CalculadoraEquivalencias.CalcularCantidadDestino(
                item.Cantidad,
                origen,
                destino,
                candidato.Criterio
            );

            // Adaptamos la cantidad a incrementos prácticos.
            var cantidadSugerida = AjustarCantidad(cantidadTeorica, destino.UnidadBase);

            if (cantidadSugerida <= 0m)
                continue;

            var aporteOrigen = CalculadoraNutricional.Calcular(
                item.Cantidad,
                origen.CantidadBase,
                origen.Calorias,
                origen.Proteinas,
                origen.Carbohidratos,
                origen.Grasas
            );

            var aporteDestino = CalculadoraNutricional.Calcular(
                cantidadSugerida,
                destino.CantidadBase,
                destino.Calorias,
                destino.Proteinas,
                destino.Carbohidratos,
                destino.Grasas
            );

            if (
                !aporteOrigen.Calorias.HasValue
                || !aporteOrigen.Proteinas.HasValue
                || !aporteOrigen.Carbohidratos.HasValue
                || !aporteOrigen.Grasas.HasValue
                || !aporteDestino.Calorias.HasValue
                || !aporteDestino.Proteinas.HasValue
                || !aporteDestino.Carbohidratos.HasValue
                || !aporteDestino.Grasas.HasValue
            )
            {
                continue;
            }

            // Total original - alimento retirado + sustituto.
            var simulados = new NutrientesSimulacionDto
            {
                Calorias = Math.Round(
                    actuales.Calorias - aporteOrigen.Calorias.Value + aporteDestino.Calorias.Value,
                    2
                ),

                Proteinas = Math.Round(
                    actuales.Proteinas
                        - aporteOrigen.Proteinas.Value
                        + aporteDestino.Proteinas.Value,
                    2
                ),

                Carbohidratos = Math.Round(
                    actuales.Carbohidratos
                        - aporteOrigen.Carbohidratos.Value
                        + aporteDestino.Carbohidratos.Value,
                    2
                ),

                Grasas = Math.Round(
                    actuales.Grasas - aporteOrigen.Grasas.Value + aporteDestino.Grasas.Value,
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

            var puntajeNuevo = CalcularPuntaje(simulados, actuales, resultado.Objetivos);

            decimal? mejora =
                puntajeOriginal.HasValue && puntajeNuevo.HasValue
                    ? puntajeOriginal.Value - puntajeNuevo.Value
                    : null;

            var estado =
                !mejora.HasValue ? "SinObjetivos"
                : mejora.Value > Tolerancia ? "Mejora"
                : mejora.Value < -Tolerancia ? "Empeora"
                : "SinCambio";

            // Desviación introducida por el redondeo de cantidades.
            var nutrienteAReemplazar = item.Cantidad / origen.CantidadBase * nutrienteOrigen.Value;

            var nutrienteReemplazo =
                cantidadSugerida / destino.CantidadBase * nutrienteDestino.Value;

            var desviacion = Math.Round(
                Math.Abs(nutrienteAReemplazar - nutrienteReemplazo) / nutrienteAReemplazar * 100m,
                2
            );

            var impactos = CrearImpactos(actuales, simulados, resultado.Objetivos);

            var advertencias = new List<string>();

            if (estado == "Empeora")
            {
                advertencias.Add(
                    "La sustitución empeora el balance global "
                        + "respecto de los objetivos configurados."
                );
            }

            if (desviacion > 5m)
            {
                advertencias.Add(
                    "El redondeo de la cantidad genera una "
                        + "diferencia superior al 5% en el nutriente "
                        + "utilizado como criterio de equivalencia."
                );
            }

            resultado.Propuestas.Add(
                new PropuestaSustitucionDto
                {
                    ItemOrigenId = item.Id,

                    AlimentoOrigenId = origen.Id,
                    NombreOrigen = origen.Nombre,
                    CantidadOrigen = item.Cantidad,
                    UnidadOrigen = item.UnidadMedida.ToString(),

                    AlimentoDestinoId = destino.Id,
                    NombreDestino = destino.Nombre,
                    CantidadTeoricaDestino = cantidadTeorica,
                    CantidadSugerida = cantidadSugerida,
                    UnidadDestino = destino.UnidadBase.ToString(),

                    GrupoEquivalenciaId = candidato.GrupoEquivalenciaId,
                    Criterio = candidato.Criterio.ToString(),

                    DesviacionCriterioPorcentaje = desviacion,

                    MejoraPuntaje = mejora.HasValue ? Math.Round(mejora.Value, 8) : null,

                    EstadoBalance = estado,
                    TotalesSimulados = simulados,
                    Impactos = impactos,
                    Advertencias = advertencias,
                }
            );
        }

        resultado.Propuestas = resultado
            .Propuestas.OrderByDescending(p => p.MejoraPuntaje)
            .ThenBy(p => p.DesviacionCriterioPorcentaje)
            .ThenBy(p => p.NombreDestino)
            .ToList();

        if (resultado.Propuestas.Count == 0)
        {
            resultado.Motivo = "No se encontraron sustituciones calculables.";
        }

        return resultado;
    }

    private static bool DatosValidos(Alimento alimento)
    {
        return alimento.CantidadBase > 0m
            && alimento.Calorias >= 0m
            && alimento.Proteinas >= 0m
            && alimento.Carbohidratos >= 0m
            && alimento.Grasas >= 0m
            && alimento.Calorias.HasValue
            && alimento.Proteinas.HasValue
            && alimento.Carbohidratos.HasValue
            && alimento.Grasas.HasValue;
    }

    private static decimal AjustarCantidad(decimal cantidad, UnidadMedida unidad)
    {
        var paso = unidad switch
        {
            UnidadMedida.Gramos => 5m,
            UnidadMedida.Mililitros => 50m,
            UnidadMedida.Unidad => 1m,
            UnidadMedida.Porcion => 0.5m,
            _ => 0m,
        };

        if (paso <= 0m)
            return 0m;

        return Math.Round(cantidad / paso, 0, MidpointRounding.AwayFromZero) * paso;
    }

    private static decimal? CalcularPuntaje(
        NutrientesSimulacionDto evaluados,
        NutrientesSimulacionDto iniciales,
        ObjetivosSimulacionDto objetivos
    )
    {
        if (
            !objetivos.Calorias.HasValue
            && !objetivos.Proteinas.HasValue
            && !objetivos.Carbohidratos.HasValue
            && !objetivos.Grasas.HasValue
        )
        {
            return null;
        }

        return Error(evaluados.Calorias, iniciales.Calorias, objetivos.Calorias)
            + Error(evaluados.Proteinas, iniciales.Proteinas, objetivos.Proteinas)
            + Error(evaluados.Carbohidratos, iniciales.Carbohidratos, objetivos.Carbohidratos)
            + Error(evaluados.Grasas, iniciales.Grasas, objetivos.Grasas);
    }

    private static decimal Error(decimal evaluado, decimal inicial, decimal? objetivo)
    {
        if (!objetivo.HasValue)
            return 0m;

        var escala = Math.Max(1m, Math.Max(Math.Abs(inicial), Math.Abs(objetivo.Value)));

        return Math.Abs(evaluado - objetivo.Value) / escala;
    }

    private static List<ImpactoAjusteNutrienteDto> CrearImpactos(
        NutrientesSimulacionDto actuales,
        NutrientesSimulacionDto simulados,
        ObjetivosSimulacionDto objetivos
    )
    {
        (string Nutriente, decimal Actual, decimal Simulado, decimal? Objetivo)[] datos =
        [
            ("Calorias", actuales.Calorias, simulados.Calorias, objetivos.Calorias),
            ("Proteinas", actuales.Proteinas, simulados.Proteinas, objetivos.Proteinas),
            (
                "Carbohidratos",
                actuales.Carbohidratos,
                simulados.Carbohidratos,
                objetivos.Carbohidratos
            ),
            ("Grasas", actuales.Grasas, simulados.Grasas, objetivos.Grasas),
        ];

        var impactos = new List<ImpactoAjusteNutrienteDto>();

        foreach (var dato in datos)
        {
            var impacto = new ImpactoAjusteNutrienteDto
            {
                Nutriente = dato.Nutriente,
                Actual = dato.Actual,
                Simulado = dato.Simulado,
                Objetivo = dato.Objetivo,
                Estado = "SinObjetivo",
            };

            if (dato.Objetivo.HasValue)
            {
                impacto.DiferenciaAnterior = dato.Actual - dato.Objetivo.Value;

                impacto.DiferenciaSimulada = dato.Simulado - dato.Objetivo.Value;

                var errorAnterior = Math.Abs(impacto.DiferenciaAnterior.Value);

                var errorNuevo = Math.Abs(impacto.DiferenciaSimulada.Value);

                impacto.Estado =
                    errorNuevo < errorAnterior ? "Mejora"
                    : errorNuevo > errorAnterior ? "Empeora"
                    : "SinCambio";
            }

            impactos.Add(impacto);
        }

        return impactos;
    }
}
