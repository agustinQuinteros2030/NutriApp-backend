using Microsoft.EntityFrameworkCore;

using NutriApi.Calculos;
using NutriApi.DTOs.Equivalencias;

using NutriApp.Data;
using NutriApp.Models.Alimentos;

namespace NutriApi.Services.Equivalencias;

public class EquivalenciaService : IEquivalenciaService
{
    private readonly NutriAppDbContext _context;


    public EquivalenciaService(
        NutriAppDbContext context)
    {
        _context = context;
    }


    // ==========================================
    // CREAR GRUPO
    // ==========================================

    public async Task<
        ResultadoEquivalencia<GrupoEquivalenciaDetalleDto>>
        CrearGrupoAsync(
            int nutricionistaId,
            CrearGrupoEquivalenciaDto dto)
    {
        var nombre =
            dto.Nombre.Trim();


        // ======================================
        // VALIDAR CRITERIO
        // ======================================

        if (!Enum.IsDefined(dto.Criterio))
        {
            return ErrorGrupo(
                "El criterio de equivalencia no es válido.",
                TipoErrorEquivalencia.Validacion
            );
        }


        // ======================================
        // EVITAR NOMBRE DUPLICADO
        // ======================================

        var existe =
            await _context.GruposEquivalencias
                .AnyAsync(g =>
                    g.NutricionistaId ==
                    nutricionistaId
                    &&
                    EF.Functions.ILike(
                        g.Nombre,
                        nombre
                    )
                );


        if (existe)
        {
            return ErrorGrupo(
                "Ya existe un grupo de equivalencias con ese nombre.",
                TipoErrorEquivalencia.Validacion
            );
        }


        // ======================================
        // CREAR
        // ======================================

        var grupo =
            new GrupoEquivalencia
            {
                NutricionistaId =
                    nutricionistaId,

                Nombre =
                    nombre,

                Descripcion =
                    dto.Descripcion?.Trim(),

                Criterio =
                    dto.Criterio,

                Activo =
                    true,

                FechaCreacion =
                    DateTime.UtcNow
            };


        _context.GruposEquivalencias
            .Add(grupo);


        await _context
            .SaveChangesAsync();


        return new ResultadoEquivalencia<
            GrupoEquivalenciaDetalleDto>
        {
            Exitoso =
                true,

            TipoError =
                TipoErrorEquivalencia.Ninguno,

            Datos =
                MapearGrupo(grupo)
        };
    }


    // ==========================================
    // LISTAR GRUPOS
    // ==========================================

    public async Task<
        List<GrupoEquivalenciaListadoDto>>
        ObtenerGruposAsync(
            int nutricionistaId,
            bool incluirInactivos)
    {
        var query =
            _context.GruposEquivalencias
                .AsNoTracking()
                .Where(g =>
                    g.NutricionistaId ==
                    nutricionistaId
                );


        if (!incluirInactivos)
        {
            query =
                query.Where(g =>
                    g.Activo
                );
        }


        return await query
            .OrderBy(g =>
                g.Nombre
            )
            .Select(g =>
                new GrupoEquivalenciaListadoDto
                {
                    Id =
                        g.Id,

                    Nombre =
                        g.Nombre,

                    Descripcion =
                        g.Descripcion,

                    Criterio =
                        g.Criterio,

                    Activo =
                        g.Activo,

                    CantidadAlimentos =
                        g.Equivalencias.Count(e =>
                            e.Activa
                        )
                }
            )
            .ToListAsync();
    }


    // ==========================================
    // DETALLE GRUPO
    // ==========================================

    public async Task<
        ResultadoEquivalencia<GrupoEquivalenciaDetalleDto>>
        ObtenerGrupoPorIdAsync(
            int nutricionistaId,
            int grupoId)
    {
        var grupo =
            await _context
                .GruposEquivalencias
                .AsNoTracking()
                .Include(g =>
                    g.Equivalencias
                )
                    .ThenInclude(e =>
                        e.Alimento
                    )
                .FirstOrDefaultAsync(g =>
                    g.Id ==
                    grupoId
                    &&
                    g.NutricionistaId ==
                    nutricionistaId
                );


        if (grupo is null)
        {
            return GrupoNoEncontrado();
        }


        return new ResultadoEquivalencia<
            GrupoEquivalenciaDetalleDto>
        {
            Exitoso =
                true,

            TipoError =
                TipoErrorEquivalencia.Ninguno,

            Datos =
                MapearGrupo(grupo)
        };
    }


    // ==========================================
    // EDITAR GRUPO
    // ==========================================

    public async Task<
        ResultadoEquivalencia<GrupoEquivalenciaDetalleDto>>
        EditarGrupoAsync(
            int nutricionistaId,
            int grupoId,
            EditarGrupoEquivalenciaDto dto)
    {
        var grupo =
            await _context
                .GruposEquivalencias
                .Include(g =>
                    g.Equivalencias
                )
                    .ThenInclude(e =>
                        e.Alimento
                    )
                .FirstOrDefaultAsync(g =>
                    g.Id ==
                    grupoId
                    &&
                    g.NutricionistaId ==
                    nutricionistaId
                );


        if (grupo is null)
        {
            return GrupoNoEncontrado();
        }


        // ======================================
        // VALIDAR CRITERIO
        // ======================================

        if (!Enum.IsDefined(dto.Criterio))
        {
            return ErrorGrupo(
                "El criterio de equivalencia no es válido.",
                TipoErrorEquivalencia.Validacion
            );
        }


        var nombre =
            dto.Nombre.Trim();


        // ======================================
        // EVITAR NOMBRE DUPLICADO
        // ======================================

        var existe =
            await _context
                .GruposEquivalencias
                .AnyAsync(g =>
                    g.NutricionistaId ==
                    nutricionistaId
                    &&
                    g.Id !=
                    grupoId
                    &&
                    EF.Functions.ILike(
                        g.Nombre,
                        nombre
                    )
                );


        if (existe)
        {
            return ErrorGrupo(
                "Ya existe otro grupo con ese nombre.",
                TipoErrorEquivalencia.Validacion
            );
        }


        // ======================================
        // ACTUALIZAR
        // ======================================

        grupo.Nombre =
            nombre;

        grupo.Descripcion =
            dto.Descripcion?.Trim();

        grupo.Criterio =
            dto.Criterio;


        await _context
            .SaveChangesAsync();


        return new ResultadoEquivalencia<
            GrupoEquivalenciaDetalleDto>
        {
            Exitoso =
                true,

            TipoError =
                TipoErrorEquivalencia.Ninguno,

            Datos =
                MapearGrupo(grupo)
        };
    }


    // ==========================================
    // ACTIVAR / DESACTIVAR GRUPO
    // ==========================================

    public async Task<
        ResultadoEquivalencia<bool>>
        CambiarEstadoGrupoAsync(
            int nutricionistaId,
            int grupoId,
            bool activo)
    {
        var grupo =
            await _context
                .GruposEquivalencias
                .FirstOrDefaultAsync(g =>
                    g.Id ==
                    grupoId
                    &&
                    g.NutricionistaId ==
                    nutricionistaId
                );


        if (grupo is null)
        {
            return new ResultadoEquivalencia<bool>
            {
                Exitoso =
                    false,

                Error =
                    "Grupo de equivalencias no encontrado.",

                TipoError =
                    TipoErrorEquivalencia.NoEncontrado
            };
        }


        grupo.Activo =
            activo;


        await _context
            .SaveChangesAsync();


        return new ResultadoEquivalencia<bool>
        {
            Exitoso =
                true,

            Datos =
                true,

            TipoError =
                TipoErrorEquivalencia.Ninguno
        };
    }


    // ==========================================
    // AGREGAR ALIMENTO AL GRUPO
    // ==========================================

    public async Task<
        ResultadoEquivalencia<EquivalenciaAlimentoDto>>
        AgregarAlimentoAsync(
            int nutricionistaId,
            int grupoId,
            AgregarEquivalenciaAlimentoDto dto)
    {
        var grupo =
            await _context
                .GruposEquivalencias
                .FirstOrDefaultAsync(g =>
                    g.Id ==
                    grupoId
                    &&
                    g.NutricionistaId ==
                    nutricionistaId
                );


        if (grupo is null)
        {
            return ErrorEquivalencia(
                "Grupo de equivalencias no encontrado.",
                TipoErrorEquivalencia.NoEncontrado
            );
        }


        if (!grupo.Activo)
        {
            return ErrorEquivalencia(
                "No se pueden agregar alimentos a un grupo inactivo.",
                TipoErrorEquivalencia.Validacion
            );
        }


        // ======================================
        // ALIMENTO
        // ======================================

        var alimento =
            await _context.Alimentos
                .FirstOrDefaultAsync(a =>
                    a.Id ==
                    dto.AlimentoId
                    &&
                    a.NutricionistaId ==
                    nutricionistaId
                );


        if (alimento is null)
        {
            return ErrorEquivalencia(
                "Alimento no encontrado.",
                TipoErrorEquivalencia.Validacion
            );
        }


        if (!alimento.Activo)
        {
            return ErrorEquivalencia(
                "No se puede agregar un alimento inactivo.",
                TipoErrorEquivalencia.Validacion
            );
        }


        // ======================================
        // BUSCAR EXISTENTE
        // ======================================

        var existente =
            await _context
                .EquivalenciasAlimentos
                .FirstOrDefaultAsync(e =>
                    e.GrupoEquivalenciaId ==
                    grupoId
                    &&
                    e.AlimentoId ==
                    dto.AlimentoId
                );


        /*
         * Por ahora mantenemos:
         *
         * CantidadEquivalente
         * UnidadMedida
         *
         * porque todavía existen partes del
         * sistema que utilizan la lógica anterior.
         *
         * Más adelante se eliminarán una vez que
         * AlternativaItemComidaService utilice
         * exclusivamente el cálculo nutricional.
         */

        if (existente is not null)
        {
            if (existente.Activa)
            {
                return ErrorEquivalencia(
                    "El alimento ya pertenece a este grupo.",
                    TipoErrorEquivalencia.Validacion
                );
            }


           

            existente.Activa =
                true;


            await _context
                .SaveChangesAsync();


            return new ResultadoEquivalencia<
                EquivalenciaAlimentoDto>
            {
                Exitoso =
                    true,

                TipoError =
                    TipoErrorEquivalencia.Ninguno,

                Datos =
                    MapearEquivalencia(
                        existente,
                        alimento.Nombre
                    )
            };
        }


        // ======================================
        // CREAR EQUIVALENCIA
        // ======================================

        var equivalencia =
            new EquivalenciaAlimento
            {
                GrupoEquivalenciaId =
                    grupoId,

                AlimentoId =
                    alimento.Id,

               

                Activa =
                    true
            };


        _context
            .EquivalenciasAlimentos
            .Add(equivalencia);


        await _context
            .SaveChangesAsync();


        return new ResultadoEquivalencia<
            EquivalenciaAlimentoDto>
        {
            Exitoso =
                true,

            TipoError =
                TipoErrorEquivalencia.Ninguno,

            Datos =
                MapearEquivalencia(
                    equivalencia,
                    alimento.Nombre
                )
        };
    }


    // ==========================================
    // ACTIVAR / DESACTIVAR EQUIVALENCIA
    // ==========================================

    public async Task<
        ResultadoEquivalencia<bool>>
        CambiarEstadoEquivalenciaAsync(
            int nutricionistaId,
            int grupoId,
            int equivalenciaId,
            bool activa)
    {
        var equivalencia =
            await _context
                .EquivalenciasAlimentos
                .Include(e =>
                    e.GrupoEquivalencia
                )
                .FirstOrDefaultAsync(e =>
                    e.Id ==
                    equivalenciaId
                    &&
                    e.GrupoEquivalenciaId ==
                    grupoId
                    &&
                    e.GrupoEquivalencia
                        .NutricionistaId ==
                    nutricionistaId
                );


        if (equivalencia is null)
        {
            return new ResultadoEquivalencia<bool>
            {
                Exitoso =
                    false,

                Error =
                    "Equivalencia no encontrada.",

                TipoError =
                    TipoErrorEquivalencia.NoEncontrado
            };
        }


        equivalencia.Activa =
            activa;


        await _context
            .SaveChangesAsync();


        return new ResultadoEquivalencia<bool>
        {
            Exitoso =
                true,

            Datos =
                true,

            TipoError =
                TipoErrorEquivalencia.Ninguno
        };
    }


    // ==========================================
    // CONVERSIÓN
    // ==========================================

    // ==========================================
    // CONVERSIÓN AUTOMÁTICA
    // ==========================================

    public async Task<
        ResultadoEquivalencia<
            List<ConversionEquivalenciaDto>>>
        ConvertirAsync(
            int nutricionistaId,
            int grupoId,
            int alimentoOrigenId,
            decimal cantidad)
    {
        // ======================================
        // VALIDAR CANTIDAD
        // ======================================

        if (cantidad <= 0)
        {
            return new ResultadoEquivalencia<
                List<ConversionEquivalenciaDto>>
            {
                Exitoso = false,

                Error =
                    "La cantidad debe ser mayor a cero.",

                TipoError =
                    TipoErrorEquivalencia.Validacion
            };
        }


        // ======================================
        // OBTENER GRUPO
        // ======================================

        var grupo =
            await _context
                .GruposEquivalencias
                .AsNoTracking()
                .Include(g =>
                    g.Equivalencias
                )
                    .ThenInclude(e =>
                        e.Alimento
                    )
                .FirstOrDefaultAsync(g =>
                    g.Id ==
                    grupoId
                    &&
                    g.NutricionistaId ==
                    nutricionistaId
                );


        if (grupo is null)
        {
            return new ResultadoEquivalencia<
                List<ConversionEquivalenciaDto>>
            {
                Exitoso = false,

                Error =
                    "Grupo de equivalencias no encontrado.",

                TipoError =
                    TipoErrorEquivalencia.NoEncontrado
            };
        }


        // ======================================
        // VALIDAR GRUPO
        // ======================================

        if (!grupo.Activo)
        {
            return new ResultadoEquivalencia<
                List<ConversionEquivalenciaDto>>
            {
                Exitoso = false,

                Error =
                    "El grupo de equivalencias está inactivo.",

                TipoError =
                    TipoErrorEquivalencia.Validacion
            };
        }


        if (!Enum.IsDefined(grupo.Criterio))
        {
            return new ResultadoEquivalencia<
                List<ConversionEquivalenciaDto>>
            {
                Exitoso = false,

                Error =
                    "El criterio del grupo de equivalencias no es válido.",

                TipoError =
                    TipoErrorEquivalencia.Validacion
            };
        }


        // ======================================
        // ALIMENTO ORIGEN
        // ======================================

        var origen =
            grupo.Equivalencias
                .FirstOrDefault(e =>
                    e.AlimentoId ==
                    alimentoOrigenId
                    &&
                    e.Activa
                    &&
                    e.Alimento.Activo
                );


        if (origen is null)
        {
            return new ResultadoEquivalencia<
                List<ConversionEquivalenciaDto>>
            {
                Exitoso = false,

                Error =
                    "El alimento origen no pertenece al grupo.",

                TipoError =
                    TipoErrorEquivalencia.Validacion
            };
        }


        // ======================================
        // VALIDAR CANTIDAD BASE
        // ======================================

        if (origen.Alimento.CantidadBase <= 0)
        {
            return new ResultadoEquivalencia<
                List<ConversionEquivalenciaDto>>
            {
                Exitoso = false,

                Error =
                    $"El alimento '{origen.Alimento.Nombre}' no posee una cantidad base válida.",

                TipoError =
                    TipoErrorEquivalencia.Validacion
            };
        }


        // ======================================
        // VALIDAR NUTRIENTE ORIGEN
        // ======================================

        var valorOrigen =
            CalculadoraEquivalencias
                .ObtenerValorCriterio(
                    origen.Alimento,
                    grupo.Criterio
                );


        if (!valorOrigen.HasValue ||
            valorOrigen.Value <= 0)
        {
            return new ResultadoEquivalencia<
                List<ConversionEquivalenciaDto>>
            {
                Exitoso = false,

                Error =
                    $"El alimento '{origen.Alimento.Nombre}' no posee un valor válido de {grupo.Criterio}.",

                TipoError =
                    TipoErrorEquivalencia.Validacion
            };
        }


        // ======================================
        // CONVERSIONES
        // ======================================

        var conversiones =
            new List<ConversionEquivalenciaDto>();


        foreach (var destino in
                 grupo.Equivalencias
                     .Where(e =>
                         e.Activa
                         &&
                         e.Alimento.Activo
                         &&
                         e.AlimentoId !=
                         alimentoOrigenId
                     ))
        {
            // ==================================
            // VALIDAR CANTIDAD BASE DESTINO
            // ==================================

            if (destino.Alimento.CantidadBase <= 0)
            {
                continue;
            }


            // ==================================
            // VALIDAR NUTRIENTE DESTINO
            // ==================================

            var valorDestino =
                CalculadoraEquivalencias
                    .ObtenerValorCriterio(
                        destino.Alimento,
                        grupo.Criterio
                    );


            /*
             * Si un alimento tiene información
             * nutricional incompleta, no rompemos
             * todas las conversiones.
             *
             * Simplemente no lo ofrecemos.
             */

            if (!valorDestino.HasValue ||
                valorDestino.Value <= 0)
            {
                continue;
            }


            // ==================================
            // CALCULAR AUTOMÁTICAMENTE
            // ==================================

            var cantidadDestino =
                CalculadoraEquivalencias
                    .CalcularCantidadDestino(
                        cantidad,
                        origen.Alimento,
                        destino.Alimento,
                        grupo.Criterio
                    );


            conversiones.Add(
                new ConversionEquivalenciaDto
                {
                    AlimentoId =
                        destino.AlimentoId,

                    Alimento =
                        destino.Alimento.Nombre,

                    Cantidad =
                        cantidadDestino,

                    UnidadMedida =
                        destino.Alimento
                            .UnidadBase
                            .ToString()
                }
            );
        }


        // ======================================
        // RESULTADO
        // ======================================

        return new ResultadoEquivalencia<
            List<ConversionEquivalenciaDto>>
        {
            Exitoso = true,

            Datos =
                conversiones
                    .OrderBy(c =>
                        c.Alimento
                    )
                    .ToList(),

            TipoError =
                TipoErrorEquivalencia.Ninguno
        };
    }

    // ==========================================
    // MAPPERS
    // ==========================================

    private static GrupoEquivalenciaDetalleDto
        MapearGrupo(
            GrupoEquivalencia grupo)
    {
        return new GrupoEquivalenciaDetalleDto
        {
            Id =
                grupo.Id,

            Nombre =
                grupo.Nombre,

            Descripcion =
                grupo.Descripcion,

            Criterio =
                grupo.Criterio.ToString(),

            Activo =
                grupo.Activo,

            FechaCreacion =
                grupo.FechaCreacion,

            Equivalencias =
                grupo.Equivalencias?
                    .Select(e =>
                        MapearEquivalencia(
                            e,
                            e.Alimento?.Nombre
                            ?? string.Empty
                        )
                    )
                    .OrderBy(e =>
                        e.Alimento
                    )
                    .ToList()
                ?? new()
        };
    }


    private static EquivalenciaAlimentoDto
        MapearEquivalencia(
            EquivalenciaAlimento equivalencia,
            string alimento)
    {
        return new EquivalenciaAlimentoDto
        {
            Id =
                equivalencia.Id,

            AlimentoId =
                equivalencia.AlimentoId,

            Alimento =
                alimento,

            

            Activa =
                equivalencia.Activa
        };
    }


    // ==========================================
    // ERRORES
    // ==========================================

    private static ResultadoEquivalencia<
        GrupoEquivalenciaDetalleDto>
        GrupoNoEncontrado()
    {
        return ErrorGrupo(
            "Grupo de equivalencias no encontrado.",
            TipoErrorEquivalencia.NoEncontrado
        );
    }


    private static ResultadoEquivalencia<
        GrupoEquivalenciaDetalleDto>
        ErrorGrupo(
            string error,
            TipoErrorEquivalencia tipo)
    {
        return new ResultadoEquivalencia<
            GrupoEquivalenciaDetalleDto>
        {
            Exitoso =
                false,

            Error =
                error,

            TipoError =
                tipo
        };
    }


    private static ResultadoEquivalencia<
        EquivalenciaAlimentoDto>
        ErrorEquivalencia(
            string error,
            TipoErrorEquivalencia tipo)
    {
        return new ResultadoEquivalencia<
            EquivalenciaAlimentoDto>
        {
            Exitoso =
                false,

            Error =
                error,

            TipoError =
                tipo
        };
    }
}