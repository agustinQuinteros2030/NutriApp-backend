using Microsoft.EntityFrameworkCore;

using NutriApi.Calculos;
using NutriApi.DTOs.Dietas;

using NutriApp.Data;
using NutriApp.Enums;
using NutriApp.Enums.Alimentos;
using NutriApp.Models.Alimentos;
using NutriApp.Models.Dietas;

namespace NutriApi.Services.Dietas;

public class AlternativaItemComidaService
    : IAlternativaItemComidaService
{
    private readonly NutriAppDbContext _context;


    public AlternativaItemComidaService(
        NutriAppDbContext context)
    {
        _context = context;
    }


    // ==========================================
    // AGREGAR ALTERNATIVA
    // ==========================================

    public async Task<
        ResultadoDieta<AlternativaItemComidaDto>>
        AgregarAsync(
            int nutricionistaId,
            int pacienteId,
            int dietaId,
            int comidaId,
            int seccionId,
            int opcionId,
            int itemId,
            AgregarAlternativaItemComidaDto dto)
    {
        var item =
            await ObtenerItemAsync(
                nutricionistaId,
                pacienteId,
                dietaId,
                comidaId,
                seccionId,
                opcionId,
                itemId
            );


        if (item is null)
        {
            return Error<AlternativaItemComidaDto>(
                "Ítem no encontrado.",
                TipoErrorDieta.NoEncontrado
            );
        }


        // ======================================
        // DIETA ARCHIVADA
        // ======================================

        if (item.OpcionSeccionComida
                .SeccionComida
                .Comida
                .Dieta
                .Estado ==
            EstadoDieta.Archivada)
        {
            return Error<AlternativaItemComidaDto>(
                "No se puede modificar una dieta archivada.",
                TipoErrorDieta.Validacion
            );
        }


        // ======================================
        // MISMO ALIMENTO
        // ======================================

        if (item.AlimentoId ==
            dto.AlimentoId)
        {
            return Error<AlternativaItemComidaDto>(
                "El alimento alternativo no puede ser el mismo que el alimento original.",
                TipoErrorDieta.Validacion
            );
        }


        // ======================================
        // ALIMENTO ALTERNATIVO
        // ======================================

        var alimentoAlternativo =
            await _context.Alimentos
                .AsNoTracking()
                .FirstOrDefaultAsync(a =>
                    a.Id ==
                    dto.AlimentoId
                    &&
                    a.NutricionistaId ==
                    nutricionistaId
                );


        if (alimentoAlternativo is null)
        {
            return Error<AlternativaItemComidaDto>(
                "Alimento alternativo no encontrado.",
                TipoErrorDieta.Validacion
            );
        }


        if (!alimentoAlternativo.Activo)
        {
            return Error<AlternativaItemComidaDto>(
                "No se puede utilizar un alimento inactivo.",
                TipoErrorDieta.Validacion
            );
        }


        // ======================================
        // GRUPO DE EQUIVALENCIA
        // ======================================

        var grupo =
            await _context.GruposEquivalencias
                .AsNoTracking()
                .FirstOrDefaultAsync(g =>
                    g.Id ==
                    dto.GrupoEquivalenciaId
                    &&
                    g.NutricionistaId ==
                    nutricionistaId
                );


        if (grupo is null)
        {
            return Error<AlternativaItemComidaDto>(
                "Grupo de equivalencias no encontrado.",
                TipoErrorDieta.Validacion
            );
        }


        if (!grupo.Activo)
        {
            return Error<AlternativaItemComidaDto>(
                "El grupo de equivalencias está inactivo.",
                TipoErrorDieta.Validacion
            );
        }


        if (!Enum.IsDefined(grupo.Criterio))
        {
            return Error<AlternativaItemComidaDto>(
                "El grupo posee un criterio de equivalencia inválido.",
                TipoErrorDieta.Validacion
            );
        }


        // ======================================
        // PERTENENCIA DEL ALIMENTO ORIGINAL
        // ======================================

        /*
         * EquivalenciaAlimento deja de representar
         * una regla matemática manual.
         *
         * Ahora la utilizamos para determinar que:
         *
         * "este alimento pertenece a este grupo".
         *
         * Por ejemplo:
         *
         * Grupo = Carbohidratos
         *
         * Arroz pertenece
         * Papa pertenece
         * Fideos pertenecen
         */

        var equivalenciaOrigen =
            await _context
                .EquivalenciasAlimentos
                .AsNoTracking()
                .FirstOrDefaultAsync(e =>
                    e.GrupoEquivalenciaId ==
                    grupo.Id
                    &&
                    e.AlimentoId ==
                    item.AlimentoId
                    &&
                    e.Activa
                );


        if (equivalenciaOrigen is null)
        {
            return Error<AlternativaItemComidaDto>(
                "El alimento original no pertenece al grupo de equivalencias seleccionado.",
                TipoErrorDieta.Validacion
            );
        }


        // ======================================
        // PERTENENCIA DEL ALIMENTO ALTERNATIVO
        // ======================================

        var equivalenciaAlternativa =
            await _context
                .EquivalenciasAlimentos
                .AsNoTracking()
                .FirstOrDefaultAsync(e =>
                    e.GrupoEquivalenciaId ==
                    grupo.Id
                    &&
                    e.AlimentoId ==
                    alimentoAlternativo.Id
                    &&
                    e.Activa
                );


        if (equivalenciaAlternativa is null)
        {
            return Error<AlternativaItemComidaDto>(
                "El alimento alternativo no pertenece al grupo de equivalencias seleccionado.",
                TipoErrorDieta.Validacion
            );
        }


        // ======================================
        // VALIDAR UNIDAD DEL ITEM
        // ======================================

        /*
         * Para poder calcular automáticamente
         * los nutrientes necesitamos que la cantidad
         * indicada en la dieta utilice la misma
         * unidad base del alimento.
         *
         * Ejemplo:
         *
         * Arroz:
         * UnidadBase = Gramos
         *
         * 100 Gramos -> OK
         * 2 Porciones -> no podemos inferir gramos.
         */

        if (item.UnidadMedida !=
            item.Alimento.UnidadBase)
        {
            return Error<AlternativaItemComidaDto>(
                $"El alimento '{item.Alimento.Nombre}' debe utilizar la unidad base '{item.Alimento.UnidadBase}' para calcular equivalencias automáticamente.",
                TipoErrorDieta.Validacion
            );
        }


        // ======================================
        // VALIDAR CÁLCULO NUTRICIONAL
        // ======================================

        var errorCalculo =
            ValidarCalculoAutomatico(
                item.Alimento,
                alimentoAlternativo,
                grupo.Criterio
            );


        if (errorCalculo is not null)
        {
            return Error<AlternativaItemComidaDto>(
                errorCalculo,
                TipoErrorDieta.Validacion
            );
        }


        // ======================================
        // VER DUPLICADO
        // ======================================

        var existente =
            await _context
                .AlternativasItemsComidas
                .FirstOrDefaultAsync(a =>
                    a.ItemOpcionComidaId ==
                    itemId
                    &&
                    a.AlimentoId ==
                    alimentoAlternativo.Id
                );


        /*
         * Existe un índice único:
         *
         * ItemOpcionComidaId + AlimentoId
         *
         * Si existía pero estaba inactiva,
         * la reactivamos.
         */

        if (existente is not null)
        {
            if (existente.Activa)
            {
                return Error<AlternativaItemComidaDto>(
                    "El alimento ya está agregado como alternativa.",
                    TipoErrorDieta.Validacion
                );
            }


            existente.Activa =
                true;

            existente.GrupoEquivalenciaId =
                grupo.Id;


            await _context
                .SaveChangesAsync();


            return new ResultadoDieta<
                AlternativaItemComidaDto>
            {
                Exitoso =
                    true,

                Datos =
                    CrearDto(
                        existente.Id,
                        item.Alimento,
                        alimentoAlternativo,
                        grupo.Id,
                        grupo.Nombre,
                        item.Cantidad,
                        grupo.Criterio,
                        true
                    ),

                TipoError =
                    TipoErrorDieta.Ninguno
            };
        }


        // ======================================
        // CREAR ALTERNATIVA
        // ======================================

        var alternativa =
            new AlternativaItemComida
            {
                ItemOpcionComidaId =
                    itemId,

                AlimentoId =
                    alimentoAlternativo.Id,

                GrupoEquivalenciaId =
                    grupo.Id,

                Activa =
                    true
            };


        _context
            .AlternativasItemsComidas
            .Add(alternativa);


        await _context
            .SaveChangesAsync();


        return new ResultadoDieta<
            AlternativaItemComidaDto>
        {
            Exitoso =
                true,

            Datos =
                CrearDto(
                    alternativa.Id,
                    item.Alimento,
                    alimentoAlternativo,
                    grupo.Id,
                    grupo.Nombre,
                    item.Cantidad,
                    grupo.Criterio,
                    true
                ),

            TipoError =
                TipoErrorDieta.Ninguno
        };
    }


    // ==========================================
    // LISTAR ALTERNATIVAS
    // ==========================================

    public async Task<
        ResultadoDieta<List<AlternativaItemComidaDto>>>
        ObtenerTodasAsync(
            int nutricionistaId,
            int pacienteId,
            int dietaId,
            int comidaId,
            int seccionId,
            int opcionId,
            int itemId,
            bool incluirInactivas)
    {
        var item =
            await ObtenerItemAsync(
                nutricionistaId,
                pacienteId,
                dietaId,
                comidaId,
                seccionId,
                opcionId,
                itemId
            );


        if (item is null)
        {
            return Error<
                List<AlternativaItemComidaDto>>(
                "Ítem no encontrado.",
                TipoErrorDieta.NoEncontrado
            );
        }


        var query =
            _context
                .AlternativasItemsComidas
                .AsNoTracking()
                .Include(a =>
                    a.Alimento
                )
                .Include(a =>
                    a.GrupoEquivalencia
                )
                .Where(a =>
                    a.ItemOpcionComidaId ==
                    itemId
                );


        if (!incluirInactivas)
        {
            query =
                query.Where(a =>
                    a.Activa
                );
        }


        var alternativas =
            await query
                .ToListAsync();


        if (alternativas.Count == 0)
        {
            return new ResultadoDieta<
                List<AlternativaItemComidaDto>>
            {
                Exitoso =
                    true,

                Datos =
                    new(),

                TipoError =
                    TipoErrorDieta.Ninguno
            };
        }


        // ======================================
        // PERTENENCIAS A GRUPOS
        // ======================================

        /*
         * Seguimos comprobando que tanto el alimento
         * original como el alternativo continúen
         * perteneciendo al grupo.
         *
         * Si después se desactiva una membresía,
         * esa alternativa deja de ofrecerse.
         */

        var gruposIds =
            alternativas
                .Select(a =>
                    a.GrupoEquivalenciaId
                )
                .Distinct()
                .ToList();


        var alimentosIds =
            alternativas
                .Select(a =>
                    a.AlimentoId
                )
                .Append(
                    item.AlimentoId
                )
                .Distinct()
                .ToList();


        var equivalencias =
            await _context
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


        var resultado =
            new List<
                AlternativaItemComidaDto>();


        // ======================================
        // UNIDAD DEL ALIMENTO ORIGINAL
        // ======================================

        /*
         * Sin una relación explícita entre:
         *
         * Porcion -> gramos
         * Unidad -> gramos
         *
         * no podemos transformar cantidades
         * automáticamente.
         */

        if (item.UnidadMedida !=
            item.Alimento.UnidadBase)
        {
            return new ResultadoDieta<
                List<AlternativaItemComidaDto>>
            {
                Exitoso =
                    true,

                Datos =
                    resultado,

                TipoError =
                    TipoErrorDieta.Ninguno
            };
        }


        foreach (var alternativa
                 in alternativas)
        {
            // ==================================
            // VALIDAR MEMBRESÍA ORIGEN
            // ==================================

            var origen =
                equivalencias
                    .FirstOrDefault(e =>
                        e.GrupoEquivalenciaId ==
                        alternativa
                            .GrupoEquivalenciaId
                        &&
                        e.AlimentoId ==
                        item.AlimentoId
                    );


            // ==================================
            // VALIDAR MEMBRESÍA DESTINO
            // ==================================

            var destino =
                equivalencias
                    .FirstOrDefault(e =>
                        e.GrupoEquivalenciaId ==
                        alternativa
                            .GrupoEquivalenciaId
                        &&
                        e.AlimentoId ==
                        alternativa.AlimentoId
                    );


            /*
             * Si alguna membresía fue desactivada
             * después de crear la dieta,
             * no ofrecemos esa alternativa.
             */

            if (origen is null ||
                destino is null)
            {
                continue;
            }


            // ==================================
            // GRUPO VÁLIDO
            // ==================================

            if (!alternativa
                    .GrupoEquivalencia
                    .Activo)
            {
                continue;
            }


            if (!Enum.IsDefined(
                    alternativa
                        .GrupoEquivalencia
                        .Criterio))
            {
                continue;
            }


            // ==================================
            // ALIMENTO ACTIVO
            // ==================================

            if (!alternativa.Alimento.Activo)
            {
                continue;
            }


            // ==================================
            // VALIDAR MACROS
            // ==================================

            var errorCalculo =
                ValidarCalculoAutomatico(
                    item.Alimento,
                    alternativa.Alimento,
                    alternativa
                        .GrupoEquivalencia
                        .Criterio
                );


            if (errorCalculo is not null)
            {
                /*
                 * No queremos romper todo el listado
                 * porque un único alimento tenga
                 * información nutricional incompleta.
                 *
                 * Simplemente no ofrecemos esa
                 * alternativa.
                 */

                continue;
            }


            // ==================================
            // CALCULAR
            // ==================================

            resultado.Add(
                CrearDto(
                    alternativa.Id,
                    item.Alimento,
                    alternativa.Alimento,
                    alternativa
                        .GrupoEquivalenciaId,
                    alternativa
                        .GrupoEquivalencia
                        .Nombre,
                    item.Cantidad,
                    alternativa
                        .GrupoEquivalencia
                        .Criterio,
                    alternativa.Activa
                )
            );
        }


        return new ResultadoDieta<
            List<AlternativaItemComidaDto>>
        {
            Exitoso =
                true,

            Datos =
                resultado
                    .OrderBy(a =>
                        a.Alimento
                    )
                    .ToList(),

            TipoError =
                TipoErrorDieta.Ninguno
        };
    }


    // ==========================================
    // ACTIVAR / DESACTIVAR
    // ==========================================

    public async Task<
        ResultadoDieta<bool>>
        CambiarEstadoAsync(
            int nutricionistaId,
            int pacienteId,
            int dietaId,
            int comidaId,
            int seccionId,
            int opcionId,
            int itemId,
            int alternativaId,
            bool activa)
    {
        var item =
            await ObtenerItemAsync(
                nutricionistaId,
                pacienteId,
                dietaId,
                comidaId,
                seccionId,
                opcionId,
                itemId
            );


        if (item is null)
        {
            return Error<bool>(
                "Ítem no encontrado.",
                TipoErrorDieta.NoEncontrado
            );
        }


        if (item.OpcionSeccionComida
                .SeccionComida
                .Comida
                .Dieta
                .Estado ==
            EstadoDieta.Archivada)
        {
            return Error<bool>(
                "No se puede modificar una dieta archivada.",
                TipoErrorDieta.Validacion
            );
        }


        var alternativa =
            await _context
                .AlternativasItemsComidas
                .FirstOrDefaultAsync(a =>
                    a.Id ==
                    alternativaId
                    &&
                    a.ItemOpcionComidaId ==
                    itemId
                );


        if (alternativa is null)
        {
            return Error<bool>(
                "Alternativa no encontrada.",
                TipoErrorDieta.NoEncontrado
            );
        }


        alternativa.Activa =
            activa;


        await _context
            .SaveChangesAsync();


        return new ResultadoDieta<bool>
        {
            Exitoso =
                true,

            Datos =
                true,

            TipoError =
                TipoErrorDieta.Ninguno
        };
    }


    // ==========================================
    // OBTENER ITEM + OWNERSHIP COMPLETO
    // ==========================================

    private async Task<ItemOpcionComida?>
        ObtenerItemAsync(
            int nutricionistaId,
            int pacienteId,
            int dietaId,
            int comidaId,
            int seccionId,
            int opcionId,
            int itemId)
    {
        return await _context
            .ItemsOpcionesComidas
            .Include(i =>
                i.Alimento
            )
            .Include(i =>
                i.OpcionSeccionComida
            )
                .ThenInclude(o =>
                    o.SeccionComida
                )
                    .ThenInclude(s =>
                        s.Comida
                    )
                        .ThenInclude(c =>
                            c.Dieta
                        )
                            .ThenInclude(d =>
                                d.Paciente
                            )
            .FirstOrDefaultAsync(i =>
                i.Id ==
                itemId
                &&
                i.OpcionSeccionComidaId ==
                opcionId
                &&
                i.OpcionSeccionComida
                    .SeccionComidaId ==
                seccionId
                &&
                i.OpcionSeccionComida
                    .SeccionComida
                    .ComidaId ==
                comidaId
                &&
                i.OpcionSeccionComida
                    .SeccionComida
                    .Comida
                    .DietaId ==
                dietaId
                &&
                i.OpcionSeccionComida
                    .SeccionComida
                    .Comida
                    .Dieta
                    .PacienteId ==
                pacienteId
                &&
                i.OpcionSeccionComida
                    .SeccionComida
                    .Comida
                    .Dieta
                    .Paciente
                    .NutricionistaId ==
                nutricionistaId
            );
    }


    // ==========================================
    // VALIDAR CÁLCULO AUTOMÁTICO
    // ==========================================

    private static string?
        ValidarCalculoAutomatico(
            Alimento alimentoOrigen,
            Alimento alimentoDestino,
            CriterioEquivalencia criterio)
    {
        if (!Enum.IsDefined(criterio))
        {
            return
                "El criterio de equivalencia no es válido.";
        }


        if (alimentoOrigen.CantidadBase <= 0)
        {
            return
                $"El alimento '{alimentoOrigen.Nombre}' no posee una cantidad base válida.";
        }


        if (alimentoDestino.CantidadBase <= 0)
        {
            return
                $"El alimento '{alimentoDestino.Nombre}' no posee una cantidad base válida.";
        }


        var valorOrigen =
            CalculadoraEquivalencias
                .ObtenerValorCriterio(
                    alimentoOrigen,
                    criterio
                );


        var valorDestino =
            CalculadoraEquivalencias
                .ObtenerValorCriterio(
                    alimentoDestino,
                    criterio
                );


        if (!valorOrigen.HasValue ||
            valorOrigen.Value <= 0)
        {
            return
                $"El alimento '{alimentoOrigen.Nombre}' no posee un valor válido de {criterio}.";
        }


        if (!valorDestino.HasValue ||
            valorDestino.Value <= 0)
        {
            return
                $"El alimento '{alimentoDestino.Nombre}' no posee un valor válido de {criterio}.";
        }


        return null;
    }


    // ==========================================
    // CÁLCULO DE ALTERNATIVA
    // ==========================================

    private static AlternativaItemComidaDto
        CrearDto(
            int id,
            Alimento alimentoOrigen,
            Alimento alimentoDestino,
            int grupoId,
            string grupo,
            decimal cantidadOriginal,
            CriterioEquivalencia criterio,
            bool activa)
    {
        /*
         * Ya NO utilizamos:
         *
         * CantidadEquivalenteOrigen
         * CantidadEquivalenteDestino
         *
         * El cálculo sale directamente de:
         *
         * cantidad original
         * +
         * información nutricional del origen
         * +
         * información nutricional del destino
         * +
         * criterio del grupo.
         */

        var cantidadCalculada =
            CalculadoraEquivalencias
                .CalcularCantidadDestino(
                    cantidadOriginal,
                    alimentoOrigen,
                    alimentoDestino,
                    criterio
                );


        /*
         * Como la cantidad calculada queda expresada
         * en la UnidadBase del alimento destino,
         * podemos calcular automáticamente también
         * sus calorías y macronutrientes.
         */

        var resultadoNutricional =
            CalculadoraNutricional
                .Calcular(
                    cantidadCalculada,
                    alimentoDestino.CantidadBase,
                    alimentoDestino.Calorias,
                    alimentoDestino.Proteinas,
                    alimentoDestino.Carbohidratos,
                    alimentoDestino.Grasas
                );


        var nutricion =
            new NutricionCalculadaDto
            {
                Calorias =
                    resultadoNutricional
                        .Calorias,

                Proteinas =
                    resultadoNutricional
                        .Proteinas,

                Carbohidratos =
                    resultadoNutricional
                        .Carbohidratos,

                Grasas =
                    resultadoNutricional
                        .Grasas
            };


        return new AlternativaItemComidaDto
        {
            Id =
                id,

            AlimentoId =
                alimentoDestino.Id,

            Alimento =
                alimentoDestino.Nombre,

            GrupoEquivalenciaId =
                grupoId,

            GrupoEquivalencia =
                grupo,

            CantidadCalculada =
                cantidadCalculada,

            UnidadMedida =
                alimentoDestino
                    .UnidadBase
                    .ToString(),

            Activa =
                activa,

            Nutricion =
                nutricion
        };
    }


    // ==========================================
    // ERROR
    // ==========================================

    private static ResultadoDieta<T>
        Error<T>(
            string mensaje,
            TipoErrorDieta tipo)
    {
        return new ResultadoDieta<T>
        {
            Exitoso =
                false,

            Error =
                mensaje,

            TipoError =
                tipo
        };
    }
}