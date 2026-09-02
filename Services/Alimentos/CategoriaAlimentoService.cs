using Microsoft.EntityFrameworkCore;

using NutriApi.DTOs.Alimentos;

using NutriApp.Data;
using NutriApp.Models.Alimentos;

namespace NutriApi.Services.Alimentos;

public class CategoriaAlimentoService
    : ICategoriaAlimentoService
{
    private readonly NutriAppDbContext _context;

    public CategoriaAlimentoService(
        NutriAppDbContext context)
    {
        _context = context;
    }


    // =========================================
    // CREAR
    // =========================================

    public async Task<ResultadoAlimento<CategoriaAlimentoDto>>
        CrearAsync(
            int nutricionistaId,
            CrearCategoriaAlimentoDto dto)
    {
        var nombre = dto.Nombre.Trim();

        var existe =
            await _context.CategoriasAlimentos
                .AnyAsync(c =>
                    c.NutricionistaId == nutricionistaId
                    &&
                    EF.Functions.ILike(
                        c.Nombre,
                        nombre
                    )
                );

        if (existe)
        {
            return new ResultadoAlimento<CategoriaAlimentoDto>
            {
                Exitoso = false,
                Error = "Ya existe una categoría con ese nombre.",
                TipoError = TipoErrorAlimento.Validacion
            };
        }


        var categoria = new CategoriaAlimento
        {
            NutricionistaId = nutricionistaId,

            Nombre = nombre,

            Descripcion =
                dto.Descripcion?.Trim(),

            Activa = true,

            FechaCreacion = DateTime.UtcNow
        };


        _context.CategoriasAlimentos.Add(categoria);

        await _context.SaveChangesAsync();


        return new ResultadoAlimento<CategoriaAlimentoDto>
        {
            Exitoso = true,

            TipoError = TipoErrorAlimento.Ninguno,

            Datos = Mapear(categoria)
        };
    }


    // =========================================
    // LISTAR
    // =========================================

    public async Task<List<CategoriaAlimentoDto>>
        ObtenerTodasAsync(
            int nutricionistaId,
            bool incluirInactivas)
    {
        var query =
            _context.CategoriasAlimentos
                .AsNoTracking()
                .Where(c =>
                    c.NutricionistaId ==
                    nutricionistaId
                );


        if (!incluirInactivas)
        {
            query = query.Where(c => c.Activa);
        }


        return await query
            .OrderBy(c => c.Nombre)
            .Select(c => new CategoriaAlimentoDto
            {
                Id = c.Id,

                Nombre = c.Nombre,

                Descripcion = c.Descripcion,

                Activa = c.Activa,

                FechaCreacion = c.FechaCreacion
            })
            .ToListAsync();
    }


    // =========================================
    // EDITAR
    // =========================================

    public async Task<ResultadoAlimento<CategoriaAlimentoDto>>
        EditarAsync(
            int nutricionistaId,
            int categoriaId,
            EditarCategoriaAlimentoDto dto)
    {
        var categoria =
            await _context.CategoriasAlimentos
                .FirstOrDefaultAsync(c =>
                    c.Id == categoriaId
                    &&
                    c.NutricionistaId ==
                    nutricionistaId
                );


        if (categoria is null)
        {
            return NoEncontrada();
        }


        var nombre = dto.Nombre.Trim();


        var existeOtra =
            await _context.CategoriasAlimentos
                .AnyAsync(c =>
                    c.NutricionistaId ==
                    nutricionistaId
                    &&
                    c.Id != categoriaId
                    &&
                    EF.Functions.ILike(
                        c.Nombre,
                        nombre
                    )
                );


        if (existeOtra)
        {
            return new ResultadoAlimento<CategoriaAlimentoDto>
            {
                Exitoso = false,
                Error = "Ya existe otra categoría con ese nombre.",
                TipoError = TipoErrorAlimento.Validacion
            };
        }


        categoria.Nombre = nombre;

        categoria.Descripcion =
            dto.Descripcion?.Trim();


        await _context.SaveChangesAsync();


        return new ResultadoAlimento<CategoriaAlimentoDto>
        {
            Exitoso = true,

            TipoError = TipoErrorAlimento.Ninguno,

            Datos = Mapear(categoria)
        };
    }


    // =========================================
    // ACTIVAR / DESACTIVAR
    // =========================================

    public async Task<ResultadoAlimento<bool>>
        CambiarEstadoAsync(
            int nutricionistaId,
            int categoriaId,
            bool activa)
    {
        var categoria =
            await _context.CategoriasAlimentos
                .FirstOrDefaultAsync(c =>
                    c.Id == categoriaId
                    &&
                    c.NutricionistaId ==
                    nutricionistaId
                );


        if (categoria is null)
        {
            return new ResultadoAlimento<bool>
            {
                Exitoso = false,

                Error = "Categoría no encontrada.",

                TipoError =
                    TipoErrorAlimento.NoEncontrado
            };
        }


        categoria.Activa = activa;

        await _context.SaveChangesAsync();


        return new ResultadoAlimento<bool>
        {
            Exitoso = true,

            Datos = true,

            TipoError = TipoErrorAlimento.Ninguno
        };
    }


    private static CategoriaAlimentoDto Mapear(
        CategoriaAlimento categoria)
    {
        return new CategoriaAlimentoDto
        {
            Id = categoria.Id,

            Nombre = categoria.Nombre,

            Descripcion = categoria.Descripcion,

            Activa = categoria.Activa,

            FechaCreacion = categoria.FechaCreacion
        };
    }


    private static ResultadoAlimento<CategoriaAlimentoDto>
        NoEncontrada()
    {
        return new ResultadoAlimento<CategoriaAlimentoDto>
        {
            Exitoso = false,

            Error = "Categoría no encontrada.",

            TipoError =
                TipoErrorAlimento.NoEncontrado
        };
    }
}