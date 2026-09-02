using Microsoft.EntityFrameworkCore;

using NutriApi.DTOs.Alimentos;

using NutriApp.Data;
using NutriApp.Models.Alimentos;

namespace NutriApi.Services.Alimentos;

public class AlimentoService : IAlimentoService
{
    private readonly NutriAppDbContext _context;


    public AlimentoService(
        NutriAppDbContext context)
    {
        _context = context;
    }


    // =========================================
    // CREAR
    // =========================================

    public async Task<ResultadoAlimento<AlimentoDetalleDto>>
        CrearAsync(
            int nutricionistaId,
            CrearAlimentoDto dto)
    {
        var categoria =
            await _context.CategoriasAlimentos
                .FirstOrDefaultAsync(c =>
                    c.Id == dto.CategoriaAlimentoId
                    &&
                    c.NutricionistaId ==
                    nutricionistaId
                );


        if (categoria is null)
        {
            return new ResultadoAlimento<AlimentoDetalleDto>
            {
                Exitoso = false,

                Error = "La categoría seleccionada no existe.",

                TipoError =
                    TipoErrorAlimento.Validacion
            };
        }


        if (!categoria.Activa)
        {
            return new ResultadoAlimento<AlimentoDetalleDto>
            {
                Exitoso = false,

                Error = "No se pueden agregar alimentos a una categoría inactiva.",

                TipoError =
                    TipoErrorAlimento.Validacion
            };
        }


        var nombre = dto.Nombre.Trim();


        var existe =
            await _context.Alimentos
                .AnyAsync(a =>
                    a.NutricionistaId ==
                    nutricionistaId
                    &&
                    EF.Functions.ILike(
                        a.Nombre,
                        nombre
                    )
                );


        if (existe)
        {
            return new ResultadoAlimento<AlimentoDetalleDto>
            {
                Exitoso = false,

                Error = "Ya existe un alimento con ese nombre.",

                TipoError =
                    TipoErrorAlimento.Validacion
            };
        }


        var alimento = new Alimento
        {
            NutricionistaId =
                nutricionistaId,

            CategoriaAlimentoId =
                dto.CategoriaAlimentoId,

            Nombre =
                nombre,

            CantidadBase =
                dto.CantidadBase,

            UnidadBase =
                dto.UnidadBase,

            Calorias =
                dto.Calorias,

            Proteinas =
                dto.Proteinas,

            Carbohidratos =
                dto.Carbohidratos,

            Grasas =
                dto.Grasas,

            Activo = true,

            FechaCreacion =
                DateTime.UtcNow
        };


        _context.Alimentos.Add(alimento);

        await _context.SaveChangesAsync();


        return new ResultadoAlimento<AlimentoDetalleDto>
        {
            Exitoso = true,

            TipoError =
                TipoErrorAlimento.Ninguno,

            Datos = MapearDetalle(
                alimento,
                categoria.Nombre
            )
        };
    }


    // =========================================
    // LISTAR / BUSCAR
    // =========================================

    public async Task<List<AlimentoListadoDto>>
        ObtenerTodosAsync(
            int nutricionistaId,
            string? buscar,
            int? categoriaId,
            bool incluirInactivos)
    {
        var query =
            _context.Alimentos
                .AsNoTracking()
                .Where(a =>
                    a.NutricionistaId ==
                    nutricionistaId
                );


        if (!incluirInactivos)
        {
            query =
                query.Where(a => a.Activo);
        }


        if (categoriaId.HasValue)
        {
            query =
                query.Where(a =>
                    a.CategoriaAlimentoId ==
                    categoriaId.Value
                );
        }


        if (!string.IsNullOrWhiteSpace(buscar))
        {
            var termino = buscar.Trim();


            query = query.Where(a =>
                EF.Functions.ILike(
                    a.Nombre,
                    $"%{termino}%"
                )
            );
        }


        return await query
            .OrderBy(a => a.Nombre)
            .Select(a => new AlimentoListadoDto
            {
                Id = a.Id,

                Nombre = a.Nombre,

                CategoriaAlimentoId =
                    a.CategoriaAlimentoId,

                Categoria =
                    a.Categoria.Nombre,

                CantidadBase =
                    a.CantidadBase,

                UnidadBase =
                    a.UnidadBase.ToString(),

                Activo =
                    a.Activo
            })
            .ToListAsync();
    }


    // =========================================
    // DETALLE
    // =========================================

    public async Task<ResultadoAlimento<AlimentoDetalleDto>>
        ObtenerPorIdAsync(
            int nutricionistaId,
            int alimentoId)
    {
        var alimento =
            await _context.Alimentos
                .AsNoTracking()
                .Where(a =>
                    a.Id == alimentoId
                    &&
                    a.NutricionistaId ==
                    nutricionistaId
                )
                .Select(a =>
                    new AlimentoDetalleDto
                    {
                        Id = a.Id,

                        Nombre = a.Nombre,

                        CategoriaAlimentoId =
                            a.CategoriaAlimentoId,

                        Categoria =
                            a.Categoria.Nombre,

                        CantidadBase =
                            a.CantidadBase,

                        UnidadBase =
                            a.UnidadBase.ToString(),

                        Calorias =
                            a.Calorias,

                        Proteinas =
                            a.Proteinas,

                        Carbohidratos =
                            a.Carbohidratos,

                        Grasas =
                            a.Grasas,

                        Activo =
                            a.Activo,

                        FechaCreacion =
                            a.FechaCreacion,

                        FechaActualizacion =
                            a.FechaActualizacion
                    }
                )
                .FirstOrDefaultAsync();


        if (alimento is null)
        {
            return NoEncontrado();
        }


        return new ResultadoAlimento<AlimentoDetalleDto>
        {
            Exitoso = true,

            Datos = alimento,

            TipoError =
                TipoErrorAlimento.Ninguno
        };
    }


    // =========================================
    // EDITAR
    // =========================================

    public async Task<ResultadoAlimento<AlimentoDetalleDto>>
        EditarAsync(
            int nutricionistaId,
            int alimentoId,
            EditarAlimentoDto dto)
    {
        var alimento =
            await _context.Alimentos
                .FirstOrDefaultAsync(a =>
                    a.Id == alimentoId
                    &&
                    a.NutricionistaId ==
                    nutricionistaId
                );


        if (alimento is null)
        {
            return NoEncontrado();
        }


        var categoria =
            await _context.CategoriasAlimentos
                .FirstOrDefaultAsync(c =>
                    c.Id ==
                    dto.CategoriaAlimentoId
                    &&
                    c.NutricionistaId ==
                    nutricionistaId
                );


        if (categoria is null)
        {
            return new ResultadoAlimento<AlimentoDetalleDto>
            {
                Exitoso = false,

                Error = "La categoría seleccionada no existe.",

                TipoError =
                    TipoErrorAlimento.Validacion
            };
        }


        if (!categoria.Activa)
        {
            return new ResultadoAlimento<AlimentoDetalleDto>
            {
                Exitoso = false,

                Error = "La categoría seleccionada está inactiva.",

                TipoError =
                    TipoErrorAlimento.Validacion
            };
        }


        var nombre = dto.Nombre.Trim();


        var existeOtro =
            await _context.Alimentos
                .AnyAsync(a =>
                    a.NutricionistaId ==
                    nutricionistaId
                    &&
                    a.Id != alimentoId
                    &&
                    EF.Functions.ILike(
                        a.Nombre,
                        nombre
                    )
                );


        if (existeOtro)
        {
            return new ResultadoAlimento<AlimentoDetalleDto>
            {
                Exitoso = false,

                Error = "Ya existe otro alimento con ese nombre.",

                TipoError =
                    TipoErrorAlimento.Validacion
            };
        }


        alimento.Nombre =
            nombre;

        alimento.CategoriaAlimentoId =
            dto.CategoriaAlimentoId;

        alimento.CantidadBase =
            dto.CantidadBase;

        alimento.UnidadBase =
            dto.UnidadBase;

        alimento.Calorias =
            dto.Calorias;

        alimento.Proteinas =
            dto.Proteinas;

        alimento.Carbohidratos =
            dto.Carbohidratos;

        alimento.Grasas =
            dto.Grasas;

        alimento.FechaActualizacion =
            DateTime.UtcNow;


        await _context.SaveChangesAsync();


        return new ResultadoAlimento<AlimentoDetalleDto>
        {
            Exitoso = true,

            Datos =
                MapearDetalle(
                    alimento,
                    categoria.Nombre
                ),

            TipoError =
                TipoErrorAlimento.Ninguno
        };
    }


    // =========================================
    // ACTIVAR / DESACTIVAR
    // =========================================

    public async Task<ResultadoAlimento<bool>>
        CambiarEstadoAsync(
            int nutricionistaId,
            int alimentoId,
            bool activo)
    {
        var alimento =
            await _context.Alimentos
                .FirstOrDefaultAsync(a =>
                    a.Id == alimentoId
                    &&
                    a.NutricionistaId ==
                    nutricionistaId
                );


        if (alimento is null)
        {
            return new ResultadoAlimento<bool>
            {
                Exitoso = false,

                Error = "Alimento no encontrado.",

                TipoError =
                    TipoErrorAlimento.NoEncontrado
            };
        }


        alimento.Activo = activo;

        alimento.FechaActualizacion =
            DateTime.UtcNow;


        await _context.SaveChangesAsync();


        return new ResultadoAlimento<bool>
        {
            Exitoso = true,

            Datos = true,

            TipoError =
                TipoErrorAlimento.Ninguno
        };
    }


    // =========================================
    // HELPERS
    // =========================================

    private static AlimentoDetalleDto MapearDetalle(
        Alimento alimento,
        string categoria)
    {
        return new AlimentoDetalleDto
        {
            Id = alimento.Id,

            Nombre = alimento.Nombre,

            CategoriaAlimentoId =
                alimento.CategoriaAlimentoId,

            Categoria = categoria,

            CantidadBase =
                alimento.CantidadBase,

            UnidadBase =
                alimento.UnidadBase.ToString(),

            Calorias =
                alimento.Calorias,

            Proteinas =
                alimento.Proteinas,

            Carbohidratos =
                alimento.Carbohidratos,

            Grasas =
                alimento.Grasas,

            Activo =
                alimento.Activo,

            FechaCreacion =
                alimento.FechaCreacion,

            FechaActualizacion =
                alimento.FechaActualizacion
        };
    }


    private static ResultadoAlimento<AlimentoDetalleDto>
        NoEncontrado()
    {
        return new ResultadoAlimento<AlimentoDetalleDto>
        {
            Exitoso = false,

            Error = "Alimento no encontrado.",

            TipoError =
                TipoErrorAlimento.NoEncontrado
        };
    }
}