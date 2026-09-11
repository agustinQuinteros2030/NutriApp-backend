using Microsoft.EntityFrameworkCore;

using NutriApi.Calculos;
using NutriApi.DTOs.Dietas;

using NutriApp.Data;
using NutriApp.Enums;
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
                    a.Id == dto.AlimentoId
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


        // ======================================
        // EQUIVALENCIA DEL ALIMENTO ORIGINAL
        // ======================================

        var equivalenciaOrigen =
            await _context.EquivalenciasAlimentos
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
        // EQUIVALENCIA DEL ALTERNATIVO
        // ======================================

        var equivalenciaAlternativa =
            await _context.EquivalenciasAlimentos
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


        /*
         * Para calcular la equivalencia:
         *
         * cantidadActual /
         * cantidadEquivalenteOrigen
         *
         * necesitamos que el item esté expresado
         * en la misma unidad que la equivalencia
         * del alimento original.
         */

        if (item.UnidadMedida !=
            equivalenciaOrigen.UnidadMedida)
        {
            return Error<AlternativaItemComidaDto>(
                "La unidad del ítem no coincide con la unidad utilizada en la equivalencia.",
                TipoErrorDieta.Validacion
            );
        }


        // ======================================
        // VER DUPLICADO
        // ======================================

        var existente =
            await _context.AlternativasItemsComidas
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
         * Por eso, si ya existía pero estaba
         * inactiva, la reactivamos.
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


            await _context.SaveChangesAsync();


            return new ResultadoDieta<
                AlternativaItemComidaDto>
            {
                Exitoso = true,

                Datos =
                    CrearDto(
                        existente.Id,
                        alimentoAlternativo,
                        grupo.Id,
                        grupo.Nombre,
                        item.Cantidad,
                        equivalenciaOrigen
                            .CantidadEquivalente,
                        equivalenciaAlternativa
                            .CantidadEquivalente,
                        equivalenciaAlternativa
                            .UnidadMedida,
                        true
                    ),

                TipoError =
                    TipoErrorDieta.Ninguno
            };
        }


        // ======================================
        // CREAR
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


        _context.AlternativasItemsComidas
            .Add(alternativa);


        await _context.SaveChangesAsync();


        return new ResultadoDieta<
            AlternativaItemComidaDto>
        {
            Exitoso = true,

            Datos =
                CrearDto(
                    alternativa.Id,
                    alimentoAlternativo,
                    grupo.Id,
                    grupo.Nombre,
                    item.Cantidad,
                    equivalenciaOrigen
                        .CantidadEquivalente,
                    equivalenciaAlternativa
                        .CantidadEquivalente,
                    equivalenciaAlternativa
                        .UnidadMedida,
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
            _context.AlternativasItemsComidas
                .AsNoTracking()
                .Include(a => a.Alimento)
                .Include(a => a.GrupoEquivalencia)
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
            await query.ToListAsync();


        if (alternativas.Count == 0)
        {
            return new ResultadoDieta<
                List<AlternativaItemComidaDto>>
            {
                Exitoso = true,

                Datos =
                    new(),

                TipoError =
                    TipoErrorDieta.Ninguno
            };
        }


        /*
         * Traemos todas las equivalencias necesarias
         * en una única consulta.
         *
         * Evitamos hacer consultas dentro
         * del foreach.
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
            await _context.EquivalenciasAlimentos
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


        foreach (var alternativa
                 in alternativas)
        {
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
             * Si alguna equivalencia fue desactivada
             * después de crear la dieta,
             * no la ofrecemos como alternativa válida.
             */

            if (origen is null ||
                destino is null)
            {
                continue;
            }


            if (item.UnidadMedida !=
                origen.UnidadMedida)
            {
                continue;
            }


            resultado.Add(
                CrearDto(
                    alternativa.Id,
                    alternativa.Alimento,
                    alternativa
                        .GrupoEquivalenciaId,
                    alternativa
                        .GrupoEquivalencia
                        .Nombre,
                    item.Cantidad,
                    origen.CantidadEquivalente,
                    destino.CantidadEquivalente,
                    destino.UnidadMedida,
                    alternativa.Activa
                )
            );
        }


        return new ResultadoDieta<
            List<AlternativaItemComidaDto>>
        {
            Exitoso = true,

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
            await _context.AlternativasItemsComidas
                .FirstOrDefaultAsync(a =>
                    a.Id == alternativaId
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


        await _context.SaveChangesAsync();


        return new ResultadoDieta<bool>
        {
            Exitoso = true,

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
        return await _context.ItemsOpcionesComidas
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
                i.Id == itemId
                &&
                i.OpcionSeccionComidaId ==
                opcionId
                &&
                i.OpcionSeccionComida
                    .SeccionComidaId ==
                seccionId
                &&
                i.OpcionSeccionComida
                    .SeccionComida.ComidaId ==
                comidaId
                &&
                i.OpcionSeccionComida
                    .SeccionComida.Comida.DietaId ==
                dietaId
                &&
                i.OpcionSeccionComida
                    .SeccionComida.Comida.Dieta
                    .PacienteId ==
                pacienteId
                &&
                i.OpcionSeccionComida
                    .SeccionComida.Comida.Dieta
                    .Paciente.NutricionistaId ==
                nutricionistaId
            );
    }


    // ==========================================
    // CÁLCULO DE ALTERNATIVA
    // ==========================================

    private static AlternativaItemComidaDto
        CrearDto(
            int id,
            Alimento alimento,
            int grupoId,
            string grupo,
            decimal cantidadOriginal,
            decimal equivalenciaOriginal,
            decimal equivalenciaDestino,
            UnidadMedida unidadDestino,
            bool activa)
    {
        /*
         * Primero calculamos cuánta cantidad
         * del alimento alternativo corresponde.
         */

        var cantidadCalculada =
            CalculadoraEquivalencias
                .CalcularCantidadDestino(
                    cantidadOriginal,
                    equivalenciaOriginal,
                    equivalenciaDestino
                );


        /*
         * Después calculamos los nutrientes
         * de ESA cantidad alternativa.
         */

        NutricionCalculadaDto? nutricion =
            null;


        /*
         * Solo podemos aplicar proporcionalidad
         * nutricional cuando la unidad base
         * del alimento coincide con la unidad
         * utilizada por la equivalencia.
         */

        if (unidadDestino ==
            alimento.UnidadBase)
        {
            var resultadoNutricional =
                CalculadoraNutricional
                    .Calcular(
                        cantidadCalculada,
                        alimento.CantidadBase,
                        alimento.Calorias,
                        alimento.Proteinas,
                        alimento.Carbohidratos,
                        alimento.Grasas
                    );


            nutricion =
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
        }


        return new AlternativaItemComidaDto
        {
            Id =
                id,

            AlimentoId =
                alimento.Id,

            Alimento =
                alimento.Nombre,

            GrupoEquivalenciaId =
                grupoId,

            GrupoEquivalencia =
                grupo,

            CantidadCalculada =
                cantidadCalculada,

            UnidadMedida =
                unidadDestino.ToString(),

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
            Exitoso = false,

            Error =
                mensaje,

            TipoError =
                tipo
        };
    }
}