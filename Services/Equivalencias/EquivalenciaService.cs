using Microsoft.EntityFrameworkCore;

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

    public async Task<ResultadoEquivalencia<GrupoEquivalenciaDetalleDto>>
        CrearGrupoAsync(
            int nutricionistaId,
            CrearGrupoEquivalenciaDto dto)
    {
        var nombre = dto.Nombre.Trim();

        var existe =
            await _context.GruposEquivalencias
                .AnyAsync(g =>
                    g.NutricionistaId == nutricionistaId
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


        var grupo = new GrupoEquivalencia
        {
            NutricionistaId = nutricionistaId,

            Nombre = nombre,

            Descripcion =
                dto.Descripcion?.Trim(),

            Activo = true,

            FechaCreacion = DateTime.UtcNow
        };


        _context.GruposEquivalencias.Add(grupo);

        await _context.SaveChangesAsync();


        return new ResultadoEquivalencia<GrupoEquivalenciaDetalleDto>
        {
            Exitoso = true,

            TipoError =
                TipoErrorEquivalencia.Ninguno,

            Datos = MapearGrupo(grupo)
        };
    }


    // ==========================================
    // LISTAR GRUPOS
    // ==========================================

    public async Task<List<GrupoEquivalenciaListadoDto>>
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
                query.Where(g => g.Activo);
        }


        return await query
            .OrderBy(g => g.Nombre)
            .Select(g =>
                new GrupoEquivalenciaListadoDto
                {
                    Id = g.Id,

                    Nombre = g.Nombre,

                    Descripcion = g.Descripcion,

                    Activo = g.Activo,

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

    public async Task<ResultadoEquivalencia<GrupoEquivalenciaDetalleDto>>
        ObtenerGrupoPorIdAsync(
            int nutricionistaId,
            int grupoId)
    {
        var grupo =
            await _context.GruposEquivalencias
                .AsNoTracking()
                .Include(g => g.Equivalencias)
                    .ThenInclude(e => e.Alimento)
                .FirstOrDefaultAsync(g =>
                    g.Id == grupoId
                    &&
                    g.NutricionistaId ==
                    nutricionistaId
                );


        if (grupo is null)
        {
            return GrupoNoEncontrado();
        }


        return new ResultadoEquivalencia<GrupoEquivalenciaDetalleDto>
        {
            Exitoso = true,

            TipoError =
                TipoErrorEquivalencia.Ninguno,

            Datos =
                MapearGrupo(grupo)
        };
    }


    // ==========================================
    // EDITAR GRUPO
    // ==========================================

    public async Task<ResultadoEquivalencia<GrupoEquivalenciaDetalleDto>>
        EditarGrupoAsync(
            int nutricionistaId,
            int grupoId,
            EditarGrupoEquivalenciaDto dto)
    {
        var grupo =
            await _context.GruposEquivalencias
                .Include(g => g.Equivalencias)
                    .ThenInclude(e => e.Alimento)
                .FirstOrDefaultAsync(g =>
                    g.Id == grupoId
                    &&
                    g.NutricionistaId ==
                    nutricionistaId
                );


        if (grupo is null)
        {
            return GrupoNoEncontrado();
        }


        var nombre = dto.Nombre.Trim();


        var existe =
            await _context.GruposEquivalencias
                .AnyAsync(g =>
                    g.NutricionistaId ==
                    nutricionistaId
                    &&
                    g.Id != grupoId
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


        grupo.Nombre = nombre;

        grupo.Descripcion =
            dto.Descripcion?.Trim();


        await _context.SaveChangesAsync();


        return new ResultadoEquivalencia<GrupoEquivalenciaDetalleDto>
        {
            Exitoso = true,

            TipoError =
                TipoErrorEquivalencia.Ninguno,

            Datos =
                MapearGrupo(grupo)
        };
    }


    // ==========================================
    // ACTIVAR / DESACTIVAR GRUPO
    // ==========================================

    public async Task<ResultadoEquivalencia<bool>>
        CambiarEstadoGrupoAsync(
            int nutricionistaId,
            int grupoId,
            bool activo)
    {
        var grupo =
            await _context.GruposEquivalencias
                .FirstOrDefaultAsync(g =>
                    g.Id == grupoId
                    &&
                    g.NutricionistaId ==
                    nutricionistaId
                );


        if (grupo is null)
        {
            return new ResultadoEquivalencia<bool>
            {
                Exitoso = false,

                Error =
                    "Grupo de equivalencias no encontrado.",

                TipoError =
                    TipoErrorEquivalencia.NoEncontrado
            };
        }


        grupo.Activo = activo;

        await _context.SaveChangesAsync();


        return new ResultadoEquivalencia<bool>
        {
            Exitoso = true,

            Datos = true,

            TipoError =
                TipoErrorEquivalencia.Ninguno
        };
    }


    // ==========================================
    // AGREGAR ALIMENTO AL GRUPO
    // ==========================================

    public async Task<ResultadoEquivalencia<EquivalenciaAlimentoDto>>
        AgregarAlimentoAsync(
            int nutricionistaId,
            int grupoId,
            AgregarEquivalenciaAlimentoDto dto)
    {
        var grupo =
            await _context.GruposEquivalencias
                .FirstOrDefaultAsync(g =>
                    g.Id == grupoId
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


        var alimento =
            await _context.Alimentos
                .FirstOrDefaultAsync(a =>
                    a.Id == dto.AlimentoId
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


        var existente =
            await _context.EquivalenciasAlimentos
                .FirstOrDefaultAsync(e =>
                    e.GrupoEquivalenciaId ==
                    grupoId
                    &&
                    e.AlimentoId ==
                    dto.AlimentoId
                );


        /*
         * Tenemos índice único:
         *
         * GrupoEquivalenciaId + AlimentoId
         *
         * Por eso, si existía pero estaba inactiva,
         * la reactivamos en vez de crear otra fila.
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


            existente.CantidadEquivalente =
                dto.CantidadEquivalente;

            existente.UnidadMedida =
                dto.UnidadMedida;

            existente.Activa = true;


            await _context.SaveChangesAsync();


            return new ResultadoEquivalencia<EquivalenciaAlimentoDto>
            {
                Exitoso = true,

                TipoError =
                    TipoErrorEquivalencia.Ninguno,

                Datos =
                    MapearEquivalencia(
                        existente,
                        alimento.Nombre
                    )
            };
        }


        var equivalencia =
            new EquivalenciaAlimento
            {
                GrupoEquivalenciaId =
                    grupoId,

                AlimentoId =
                    alimento.Id,

                CantidadEquivalente =
                    dto.CantidadEquivalente,

                UnidadMedida =
                    dto.UnidadMedida,

                Activa = true
            };


        _context.EquivalenciasAlimentos
            .Add(equivalencia);


        await _context.SaveChangesAsync();


        return new ResultadoEquivalencia<EquivalenciaAlimentoDto>
        {
            Exitoso = true,

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
    // EDITAR EQUIVALENCIA
    // ==========================================

    public async Task<ResultadoEquivalencia<EquivalenciaAlimentoDto>>
        EditarEquivalenciaAsync(
            int nutricionistaId,
            int grupoId,
            int equivalenciaId,
            EditarEquivalenciaAlimentoDto dto)
    {
        var equivalencia =
            await _context.EquivalenciasAlimentos
                .Include(e => e.Alimento)
                .Include(e => e.GrupoEquivalencia)
                .FirstOrDefaultAsync(e =>
                    e.Id == equivalenciaId
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
            return ErrorEquivalencia(
                "Equivalencia no encontrada.",
                TipoErrorEquivalencia.NoEncontrado
            );
        }


        equivalencia.CantidadEquivalente =
            dto.CantidadEquivalente;

        equivalencia.UnidadMedida =
            dto.UnidadMedida;


        await _context.SaveChangesAsync();


        return new ResultadoEquivalencia<EquivalenciaAlimentoDto>
        {
            Exitoso = true,

            TipoError =
                TipoErrorEquivalencia.Ninguno,

            Datos =
                MapearEquivalencia(
                    equivalencia,
                    equivalencia.Alimento.Nombre
                )
        };
    }


    // ==========================================
    // ACTIVAR / DESACTIVAR EQUIVALENCIA
    // ==========================================

    public async Task<ResultadoEquivalencia<bool>>
        CambiarEstadoEquivalenciaAsync(
            int nutricionistaId,
            int grupoId,
            int equivalenciaId,
            bool activa)
    {
        var equivalencia =
            await _context.EquivalenciasAlimentos
                .Include(e => e.GrupoEquivalencia)
                .FirstOrDefaultAsync(e =>
                    e.Id == equivalenciaId
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
                Exitoso = false,

                Error =
                    "Equivalencia no encontrada.",

                TipoError =
                    TipoErrorEquivalencia.NoEncontrado
            };
        }


        equivalencia.Activa = activa;


        await _context.SaveChangesAsync();


        return new ResultadoEquivalencia<bool>
        {
            Exitoso = true,

            Datos = true,

            TipoError =
                TipoErrorEquivalencia.Ninguno
        };
    }


    // ==========================================
    // CONVERSIÓN AUTOMÁTICA
    // ==========================================

    public async Task<
        ResultadoEquivalencia<
            List<ConversionEquivalenciaDto>
        >>
        ConvertirAsync(
            int nutricionistaId,
            int grupoId,
            int alimentoOrigenId,
            decimal cantidad)
    {
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


        var grupo =
            await _context.GruposEquivalencias
                .AsNoTracking()
                .Include(g => g.Equivalencias)
                    .ThenInclude(e => e.Alimento)
                .FirstOrDefaultAsync(g =>
                    g.Id == grupoId
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


        var origen =
            grupo.Equivalencias
                .FirstOrDefault(e =>
                    e.AlimentoId ==
                    alimentoOrigenId
                    &&
                    e.Activa
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


        /*
         * EJEMPLO:
         *
         * Arroz = 100g
         * Papa  = 400g
         *
         * Usuario pide 150g arroz.
         *
         * factor = 150 / 100 = 1.5
         *
         * papa = 400 * 1.5 = 600g
         */

        var factor =
            cantidad /
            origen.CantidadEquivalente;


        var conversiones =
            grupo.Equivalencias
                .Where(e =>
                    e.Activa
                    &&
                    e.Alimento.Activo
                    &&
                    e.AlimentoId != alimentoOrigenId
                )
                .Select(e =>
                    new ConversionEquivalenciaDto
                    {
                        AlimentoId =
                            e.AlimentoId,

                        Alimento =
                            e.Alimento.Nombre,

                        Cantidad =
                            Math.Round(
                                e.CantidadEquivalente *
                                factor,
                                2
                            ),

                        UnidadMedida =
                            e.UnidadMedida.ToString()
                    }
                )
                .OrderBy(e => e.Alimento)
                .ToList();


        return new ResultadoEquivalencia<
            List<ConversionEquivalenciaDto>>
        {
            Exitoso = true,

            Datos = conversiones,

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
            Id = grupo.Id,

            Nombre = grupo.Nombre,

            Descripcion = grupo.Descripcion,

            Activo = grupo.Activo,

            FechaCreacion = grupo.FechaCreacion,

            Equivalencias =
                grupo.Equivalencias?
                    .Select(e =>
                        MapearEquivalencia(
                            e,
                            e.Alimento?.Nombre
                            ?? string.Empty
                        )
                    )
                    .OrderBy(e => e.Alimento)
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
            Id = equivalencia.Id,

            AlimentoId =
                equivalencia.AlimentoId,

            Alimento =
                alimento,

            CantidadEquivalente =
                equivalencia.CantidadEquivalente,

            UnidadMedida =
                equivalencia.UnidadMedida.ToString(),

            Activa =
                equivalencia.Activa
        };
    }


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
            Exitoso = false,

            Error = error,

            TipoError = tipo
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
            Exitoso = false,

            Error = error,

            TipoError = tipo
        };
    }
}