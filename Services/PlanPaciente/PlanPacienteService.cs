using Microsoft.EntityFrameworkCore;

using NutriApi.Calculos;
using NutriApi.DTOs.PlanPaciente;

using NutriApp.Data;
using NutriApp.Enums;
using NutriApp.Models.Alimentos;
using NutriApp.Models.Dietas;

namespace NutriApi.Services.PlanPaciente;

public class PlanPacienteService
    : IPlanPacienteService
{
    private readonly NutriAppDbContext _context;


    public PlanPacienteService(
        NutriAppDbContext context)
    {
        _context = context;
    }


    // ==========================================
    // MI PLAN ACTIVO
    // ==========================================

    public async Task<MiPlanDto?>
        ObtenerMiPlanAsync(
            int pacienteId)
    {
        /*
         * El paciente NO manda un dietaId.
         *
         * Buscamos exclusivamente su dieta activa.
         */

        var dieta =
            await _context.Dietas
                .AsNoTracking()

                .Where(d =>
                    d.PacienteId ==
                    pacienteId
                    &&
                    d.Estado ==
                    EstadoDieta.Activa
                )

                .OrderByDescending(d =>
                    d.Version
                )

                // HIDRATACIÓN
                .Include(d =>
                    d.Hidratacion
                )

                // SUPLEMENTACIÓN
                .Include(d =>
                    d.Suplementacion
                )
                    .ThenInclude(s =>
                        s.Items
                    )

                // ALIMENTOS
                .Include(d =>
                    d.Comidas
                )
                    .ThenInclude(c =>
                        c.Secciones
                    )
                        .ThenInclude(s =>
                            s.Opciones
                        )
                            .ThenInclude(o =>
                                o.Items
                            )
                                .ThenInclude(i =>
                                    i.Alimento
                                )

                // ALTERNATIVA -> ALIMENTO
                .Include(d =>
                    d.Comidas
                )
                    .ThenInclude(c =>
                        c.Secciones
                    )
                        .ThenInclude(s =>
                            s.Opciones
                        )
                            .ThenInclude(o =>
                                o.Items
                            )
                                .ThenInclude(i =>
                                    i.Alternativas
                                )
                                    .ThenInclude(a =>
                                        a.Alimento
                                    )

                // ALTERNATIVA -> GRUPO
                .Include(d =>
                    d.Comidas
                )
                    .ThenInclude(c =>
                        c.Secciones
                    )
                        .ThenInclude(s =>
                            s.Opciones
                        )
                            .ThenInclude(o =>
                                o.Items
                            )
                                .ThenInclude(i =>
                                    i.Alternativas
                                )
                                    .ThenInclude(a =>
                                        a.GrupoEquivalencia
                                    )

                .AsSplitQuery()

                .FirstOrDefaultAsync();


        if (dieta is null)
        {
            return null;
        }


        /*
         * Para calcular las alternativas necesitamos
         * las equivalencias de origen y destino.
         *
         * Las traemos todas juntas para evitar
         * hacer consultas dentro de cada foreach.
         */

        var todosLosItems =
            dieta.Comidas
                .SelectMany(c =>
                    c.Secciones
                )
                .SelectMany(s =>
                    s.Opciones
                )
                .SelectMany(o =>
                    o.Items
                )
                .ToList();


        var alternativasActivas =
            todosLosItems
                .SelectMany(i =>
                    i.Alternativas
                )
                .Where(a =>
                    a.Activa
                )
                .ToList();


        var gruposIds =
            alternativasActivas
                .Select(a =>
                    a.GrupoEquivalenciaId
                )
                .Distinct()
                .ToList();


        var alimentosIds =
            todosLosItems
                .Select(i =>
                    i.AlimentoId
                )
                .Concat(
                    alternativasActivas
                        .Select(a =>
                            a.AlimentoId
                        )
                )
                .Distinct()
                .ToList();


        var equivalencias =
            gruposIds.Count == 0
                ? new List<EquivalenciaAlimento>()
                : await _context
                    .EquivalenciasAlimentos
                    .AsNoTracking()
                    .Where(e =>
                        gruposIds.Contains(
                            e.GrupoEquivalenciaId
                        )
                        &&
                        alimentosIds.Contains(
                            e.AlimentoId
                        )
                        &&
                        e.Activa
                    )
                    .ToListAsync();


        return MapearPlan(
            dieta,
            equivalencias
        );
    }


    // ==========================================
    // MAPEAR PLAN COMPLETO
    // ==========================================

    private static MiPlanDto
        MapearPlan(
            Dieta dieta,
            List<EquivalenciaAlimento> equivalencias)
    {
        var seccionesTotales =
            dieta.Comidas
                .SelectMany(c =>
                    c.Secciones
                )
                .ToList();


        var resultadoTotales =
            CalculadoraTotalesNutricionales
                .CalcularSecciones(
                    seccionesTotales
                );


        return new MiPlanDto
        {
            Id =
                dieta.Id,

            Nombre =
                dieta.Nombre,

            Descripcion =
                dieta.Descripcion,

            Version =
                dieta.Version,

            Estado =
                dieta.Estado.ToString(),

            FechaInicio =
                dieta.FechaInicio,

            FechaFin =
                dieta.FechaFin,

            ObservacionesGenerales =
                dieta.ObservacionesGenerales,

            Totales =
                MapearTotales(
                    resultadoTotales
                ),

            Comidas =
                dieta.Comidas
                    .OrderBy(c =>
                        c.Orden
                    )
                    .ThenBy(c =>
                        c.Id
                    )
                    .Select(c =>
                        MapearComida(
                            c,
                            equivalencias
                        )
                    )
                    .ToList(),

            Hidratacion =
                dieta.Hidratacion is null
                    ? null
                    : new MiPlanHidratacionDto
                    {
                        MililitrosDiarios =
                            dieta
                                .Hidratacion
                                .MililitrosDiarios,

                        VasosDiarios =
                            dieta
                                .Hidratacion
                                .VasosDiarios,

                        Observaciones =
                            dieta
                                .Hidratacion
                                .Observaciones
                    },

            Suplementacion =
                dieta.Suplementacion is null
                    ? null
                    : MapearSuplementacion(
                        dieta.Suplementacion
                    )
        };
    }


    // ==========================================
    // COMIDA
    // ==========================================

    private static MiPlanComidaDto
        MapearComida(
            Comida comida,
            List<EquivalenciaAlimento> equivalencias)
    {
        var resultadoTotales =
            CalculadoraTotalesNutricionales
                .CalcularSecciones(
                    comida.Secciones
                );


        return new MiPlanComidaDto
        {
            Id =
                comida.Id,

            Nombre =
                comida.Nombre,

            Tipo =
                comida.Tipo.ToString(),

            Orden =
                comida.Orden,

            Observaciones =
                comida.Observaciones,

            Totales =
                MapearTotales(
                    resultadoTotales
                ),

            Secciones =
                comida.Secciones
                    .OrderBy(s =>
                        s.Orden
                    )
                    .ThenBy(s =>
                        s.Id
                    )
                    .Select(s =>
                        MapearSeccion(
                            s,
                            equivalencias
                        )
                    )
                    .ToList()
        };
    }


    // ==========================================
    // SECCIÓN
    // ==========================================

    private static MiPlanSeccionDto
        MapearSeccion(
            SeccionComida seccion,
            List<EquivalenciaAlimento> equivalencias)
    {
        return new MiPlanSeccionDto
        {
            Id =
                seccion.Id,

            Nombre =
                seccion.Nombre,

            Tipo =
                seccion.Tipo.ToString(),

            Orden =
                seccion.Orden,

            Observaciones =
                seccion.Observaciones,

            Opciones =
                seccion.Opciones
                    .OrderBy(o =>
                        o.Orden
                    )
                    .ThenBy(o =>
                        o.Id
                    )
                    .Select(o =>
                        MapearOpcion(
                            o,
                            equivalencias
                        )
                    )
                    .ToList()
        };
    }


    // ==========================================
    // OPCIÓN
    // ==========================================

    private static MiPlanOpcionDto
        MapearOpcion(
            OpcionSeccionComida opcion,
            List<EquivalenciaAlimento> equivalencias)
    {
        var resultadoTotales =
            CalculadoraTotalesNutricionales
                .CalcularOpcion(
                    opcion.Items
                );


        return new MiPlanOpcionDto
        {
            Id =
                opcion.Id,

            Nombre =
                opcion.Nombre,

            Orden =
                opcion.Orden,

            EsPredeterminada =
                opcion.EsPredeterminada,

            Observaciones =
                opcion.Observaciones,

            Totales =
                MapearTotales(
                    resultadoTotales
                ),

            Items =
                opcion.Items
                    .OrderBy(i =>
                        i.Orden
                    )
                    .ThenBy(i =>
                        i.Id
                    )
                    .Select(i =>
                        MapearItem(
                            i,
                            equivalencias
                        )
                    )
                    .ToList()
        };
    }


    // ==========================================
    // ITEM
    // ==========================================

    private static MiPlanItemDto
        MapearItem(
            ItemOpcionComida item,
            List<EquivalenciaAlimento> equivalencias)
    {
        return new MiPlanItemDto
        {
            Id =
                item.Id,

            AlimentoId =
                item.AlimentoId,

            Alimento =
                item.Alimento?.Nombre
                ??
                string.Empty,

            Cantidad =
                item.Cantidad,

            UnidadMedida =
                item.UnidadMedida.ToString(),

            Indicaciones =
                item.Indicaciones,

            Orden =
                item.Orden,

            Nutricion =
                item.Alimento is null
                    ? null
                    : CalcularNutricion(
                        item.Cantidad,
                        item.UnidadMedida,
                        item.Alimento
                    ),

            Alternativas =
                MapearAlternativas(
                    item,
                    equivalencias
                )
        };
    }


    // ==========================================
    // ALTERNATIVAS
    // ==========================================

    private static List<MiPlanAlternativaDto>
    MapearAlternativas(
        ItemOpcionComida item,
        List<EquivalenciaAlimento> equivalencias)
    {
        var resultado =
            new List<MiPlanAlternativaDto>();


        // ==========================================
        // ALIMENTO ORIGINAL
        // ==========================================

        if (item.Alimento is null ||
            !item.Alimento.Activo)
        {
            return resultado;
        }


        /*
         * Para hacer el cálculo nutricional automático,
         * la cantidad del item debe estar expresada
         * usando la unidad base del alimento.
         *
         * Ejemplo:
         *
         * Arroz:
         * UnidadBase = Gramos
         *
         * 100 gramos -> OK
         * 2 porciones -> no podemos inferir gramos.
         */

        if (item.UnidadMedida !=
            item.Alimento.UnidadBase)
        {
            return resultado;
        }


        foreach (var alternativa
                 in item.Alternativas
                    .Where(a =>
                        a.Activa
                    ))
        {
            // ======================================
            // ALIMENTO ALTERNATIVO
            // ======================================

            if (alternativa.Alimento is null ||
                !alternativa.Alimento.Activo)
            {
                continue;
            }


            // ======================================
            // GRUPO
            // ======================================

            if (alternativa
                    .GrupoEquivalencia is null ||
                !alternativa
                    .GrupoEquivalencia
                    .Activo)
            {
                continue;
            }


            var grupo =
                alternativa
                    .GrupoEquivalencia;


            if (!Enum.IsDefined(
                    grupo.Criterio))
            {
                continue;
            }


            // ======================================
            // VALIDAR MEMBRESÍA DEL ORIGEN
            // ======================================

            var origen =
                equivalencias
                    .FirstOrDefault(e =>
                        e.GrupoEquivalenciaId ==
                        alternativa
                            .GrupoEquivalenciaId
                        &&
                        e.AlimentoId ==
                        item.AlimentoId
                        &&
                        e.Activa
                    );


            // ======================================
            // VALIDAR MEMBRESÍA DEL DESTINO
            // ======================================

            var destino =
                equivalencias
                    .FirstOrDefault(e =>
                        e.GrupoEquivalenciaId ==
                        alternativa
                            .GrupoEquivalenciaId
                        &&
                        e.AlimentoId ==
                        alternativa.AlimentoId
                        &&
                        e.Activa
                    );


            /*
             * EquivalenciaAlimento ahora solamente
             * representa pertenencia al grupo.
             *
             * Si alguno dejó de pertenecer,
             * no mostramos la alternativa.
             */

            if (origen is null ||
                destino is null)
            {
                continue;
            }


            // ======================================
            // VALIDAR CANTIDADES BASE
            // ======================================

            if (item.Alimento.CantidadBase <= 0 ||
                alternativa
                    .Alimento
                    .CantidadBase <= 0)
            {
                continue;
            }


            // ======================================
            // VALIDAR NUTRIENTE ORIGEN
            // ======================================

            var valorOrigen =
                CalculadoraEquivalencias
                    .ObtenerValorCriterio(
                        item.Alimento,
                        grupo.Criterio
                    );


            if (!valorOrigen.HasValue ||
                valorOrigen.Value <= 0)
            {
                continue;
            }


            // ======================================
            // VALIDAR NUTRIENTE DESTINO
            // ======================================

            var valorDestino =
                CalculadoraEquivalencias
                    .ObtenerValorCriterio(
                        alternativa.Alimento,
                        grupo.Criterio
                    );


            if (!valorDestino.HasValue ||
                valorDestino.Value <= 0)
            {
                continue;
            }


            // ======================================
            // CALCULAR EQUIVALENCIA AUTOMÁTICA
            // ======================================

            var cantidadCalculada =
                CalculadoraEquivalencias
                    .CalcularCantidadDestino(
                        item.Cantidad,
                        item.Alimento,
                        alternativa.Alimento,
                        grupo.Criterio
                    );


            // ======================================
            // CALCULAR NUTRICIÓN
            // ======================================

            /*
             * La cantidad calculada queda expresada
             * en la UnidadBase del alimento destino.
             */

            var nutricion =
                CalcularNutricion(
                    cantidadCalculada,
                    alternativa
                        .Alimento
                        .UnidadBase,
                    alternativa.Alimento
                );


            // ======================================
            // DTO
            // ======================================

            resultado.Add(
                new MiPlanAlternativaDto
                {
                    Id =
                        alternativa.Id,

                    AlimentoId =
                        alternativa.AlimentoId,

                    Alimento =
                        alternativa
                            .Alimento
                            .Nombre,

                    GrupoEquivalenciaId =
                        alternativa
                            .GrupoEquivalenciaId,

                    GrupoEquivalencia =
                        grupo.Nombre,

                    CantidadCalculada =
                        cantidadCalculada,

                    UnidadMedida =
                        alternativa
                            .Alimento
                            .UnidadBase
                            .ToString(),

                    Nutricion =
                        nutricion
                }
            );
        }


        return resultado
            .OrderBy(a =>
                a.Alimento
            )
            .ToList();
    }

    // ==========================================
    // NUTRICIÓN
    // ==========================================

    private static PlanNutricionDto?
        CalcularNutricion(
            decimal cantidad,
            UnidadMedida unidad,
            Alimento alimento)
    {
        if (cantidad <= 0)
        {
            return null;
        }


        if (alimento.CantidadBase <= 0)
        {
            return null;
        }


        if (unidad !=
            alimento.UnidadBase)
        {
            return null;
        }


        var resultado =
            CalculadoraNutricional
                .Calcular(
                    cantidad,
                    alimento.CantidadBase,
                    alimento.Calorias,
                    alimento.Proteinas,
                    alimento.Carbohidratos,
                    alimento.Grasas
                );


        return new PlanNutricionDto
        {
            Calorias =
                resultado.Calorias,

            Proteinas =
                resultado.Proteinas,

            Carbohidratos =
                resultado.Carbohidratos,

            Grasas =
                resultado.Grasas
        };
    }


    // ==========================================
    // TOTALES
    // ==========================================

    private static PlanTotalesNutricionalesDto
        MapearTotales(
            ResultadoTotalesNutricionales resultado)
    {
        return new PlanTotalesNutricionalesDto
        {
            Calorias =
                resultado.Calorias,

            Proteinas =
                resultado.Proteinas,

            Carbohidratos =
                resultado.Carbohidratos,

            Grasas =
                resultado.Grasas,

            CantidadItems =
                resultado.CantidadItems,

            ItemsSinCalculo =
                resultado.ItemsSinCalculo,

            SeccionesSinOpcionPredeterminada =
                resultado
                    .SeccionesSinOpcionPredeterminada,

            EsCompleto =
                resultado.EsCompleto
        };
    }


    // ==========================================
    // SUPLEMENTACIÓN
    // ==========================================

    private static MiPlanSuplementacionDto
        MapearSuplementacion(
            SuplementacionDieta suplementacion)
    {
        return new MiPlanSuplementacionDto
        {
            ObservacionesGenerales =
                suplementacion
                    .ObservacionesGenerales,

            Items =
                suplementacion.Items
                    .OrderBy(i =>
                        i.Orden
                    )
                    .ThenBy(i =>
                        i.Id
                    )
                    .Select(i =>
                        new MiPlanSuplementoDto
                        {
                            Id =
                                i.Id,

                            Nombre =
                                i.Nombre,

                            Cantidad =
                                i.Cantidad,

                            Unidad =
                                i.Unidad,

                            Momento =
                                i.Momento,

                            Indicaciones =
                                i.Indicaciones,

                            Orden =
                                i.Orden
                        }
                    )
                    .ToList()
        };
    }
}