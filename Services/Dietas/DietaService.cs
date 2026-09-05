using Microsoft.EntityFrameworkCore;

using NutriApi.DTOs.Dietas;

using NutriApp.Data;
using NutriApp.Enums;
using NutriApp.Models.Dietas;

namespace NutriApi.Services.Dietas;

public class DietaService : IDietaService
{
    private readonly NutriAppDbContext _context;


    public DietaService(
        NutriAppDbContext context)
    {
        _context = context;
    }


    // ==========================================
    // CREAR
    // ==========================================

    public async Task<ResultadoDieta<DietaDetalleDto>>
        CrearAsync(
            int nutricionistaId,
            int pacienteId,
            CrearDietaDto dto)
    {
        var pacienteExiste =
            await _context.Pacientes
                .AsNoTracking()
                .AnyAsync(p =>
                    p.Id == pacienteId
                    &&
                    p.NutricionistaId == nutricionistaId
                );


        if (!pacienteExiste)
        {
            return DietaNoEncontrada(
                "Paciente no encontrado."
            );
        }


        if (dto.FechaFin.HasValue &&
            dto.FechaFin.Value < dto.FechaInicio)
        {
            return ErrorValidacion(
                "La fecha de fin no puede ser anterior a la fecha de inicio."
            );
        }


        var ultimaVersion =
            await _context.Dietas
                .Where(d =>
                    d.PacienteId == pacienteId
                )
                .Select(d => (int?)d.Version)
                .MaxAsync()
                ?? 0;


        var dieta = new Dieta
        {
            PacienteId = pacienteId,

            Nombre = dto.Nombre.Trim(),

            Descripcion =
                dto.Descripcion?.Trim(),

            Version = ultimaVersion + 1,

            FechaInicio = dto.FechaInicio,

            FechaFin = dto.FechaFin,

            Estado = EstadoDieta.Borrador,

            ObservacionesGenerales =
                dto.ObservacionesGenerales?.Trim(),

            FechaCreacion = DateTime.UtcNow
        };


        _context.Dietas.Add(dieta);

        await _context.SaveChangesAsync();


        return new ResultadoDieta<DietaDetalleDto>
        {
            Exitoso = true,

            Datos = MapearDetalle(
                dieta,
                0
            ),

            TipoError =
                TipoErrorDieta.Ninguno
        };
    }


    // ==========================================
    // LISTAR DIETAS DEL PACIENTE
    // ==========================================

    public async Task<
        ResultadoDieta<List<DietaListadoDto>>>
        ObtenerTodasAsync(
            int nutricionistaId,
            int pacienteId)
    {
        var pacienteExiste =
            await _context.Pacientes
                .AsNoTracking()
                .AnyAsync(p =>
                    p.Id == pacienteId
                    &&
                    p.NutricionistaId ==
                    nutricionistaId
                );


        if (!pacienteExiste)
        {
            return new ResultadoDieta<
                List<DietaListadoDto>>
            {
                Exitoso = false,

                Error = "Paciente no encontrado.",

                TipoError =
                    TipoErrorDieta.NoEncontrado
            };
        }


        var dietas =
            await _context.Dietas
                .AsNoTracking()
                .Where(d =>
                    d.PacienteId == pacienteId
                )
                .OrderByDescending(d => d.Version)
                .Select(d =>
                    new DietaListadoDto
                    {
                        Id = d.Id,

                        Nombre = d.Nombre,

                        Version = d.Version,

                        Estado =
                            d.Estado.ToString(),

                        FechaInicio =
                            d.FechaInicio,

                        FechaFin =
                            d.FechaFin,

                        CantidadComidas =
                            d.Comidas.Count,

                        FechaCreacion =
                            d.FechaCreacion
                    }
                )
                .ToListAsync();


        return new ResultadoDieta<
            List<DietaListadoDto>>
        {
            Exitoso = true,

            Datos = dietas,

            TipoError =
                TipoErrorDieta.Ninguno
        };
    }


    // ==========================================
    // DETALLE
    // ==========================================

    public async Task<ResultadoDieta<DietaDetalleDto>>
        ObtenerPorIdAsync(
            int nutricionistaId,
            int pacienteId,
            int dietaId)
    {
        var dieta =
            await _context.Dietas
                .AsNoTracking()
                .Where(d =>
                    d.Id == dietaId
                    &&
                    d.PacienteId == pacienteId
                    &&
                    d.Paciente.NutricionistaId ==
                    nutricionistaId
                )
                .Select(d =>
                    new
                    {
                        Dieta = d,

                        CantidadComidas =
                            d.Comidas.Count
                    }
                )
                .FirstOrDefaultAsync();


        if (dieta is null)
        {
            return DietaNoEncontrada();
        }


        return new ResultadoDieta<DietaDetalleDto>
        {
            Exitoso = true,

            Datos =
                MapearDetalle(
                    dieta.Dieta,
                    dieta.CantidadComidas
                ),

            TipoError =
                TipoErrorDieta.Ninguno
        };
    }


    // ==========================================
    // EDITAR
    // ==========================================

    public async Task<ResultadoDieta<DietaDetalleDto>>
        EditarAsync(
            int nutricionistaId,
            int pacienteId,
            int dietaId,
            EditarDietaDto dto)
    {
        var dieta =
            await _context.Dietas
                .Include(d => d.Comidas)
                .FirstOrDefaultAsync(d =>
                    d.Id == dietaId
                    &&
                    d.PacienteId == pacienteId
                    &&
                    d.Paciente.NutricionistaId ==
                    nutricionistaId
                );


        if (dieta is null)
        {
            return DietaNoEncontrada();
        }


        if (dieta.Estado ==
            EstadoDieta.Archivada)
        {
            return ErrorValidacion(
                "No se puede editar una dieta archivada."
            );
        }


        if (dto.FechaFin.HasValue &&
            dto.FechaFin.Value < dto.FechaInicio)
        {
            return ErrorValidacion(
                "La fecha de fin no puede ser anterior a la fecha de inicio."
            );
        }


        dieta.Nombre =
            dto.Nombre.Trim();

        dieta.Descripcion =
            dto.Descripcion?.Trim();

        dieta.FechaInicio =
            dto.FechaInicio;

        dieta.FechaFin =
            dto.FechaFin;

        dieta.ObservacionesGenerales =
            dto.ObservacionesGenerales?.Trim();

        dieta.FechaActualizacion =
            DateTime.UtcNow;


        await _context.SaveChangesAsync();


        return new ResultadoDieta<DietaDetalleDto>
        {
            Exitoso = true,

            Datos =
                MapearDetalle(
                    dieta,
                    dieta.Comidas.Count
                ),

            TipoError =
                TipoErrorDieta.Ninguno
        };
    }


    // ==========================================
    // ACTIVAR
    // ==========================================

    public async Task<ResultadoDieta<DietaDetalleDto>>
        ActivarAsync(
            int nutricionistaId,
            int pacienteId,
            int dietaId)
    {
        var dieta =
            await _context.Dietas
                .Include(d => d.Comidas)
                .FirstOrDefaultAsync(d =>
                    d.Id == dietaId
                    &&
                    d.PacienteId == pacienteId
                    &&
                    d.Paciente.NutricionistaId ==
                    nutricionistaId
                );


        if (dieta is null)
        {
            return DietaNoEncontrada();
        }


        if (dieta.Estado ==
            EstadoDieta.Archivada)
        {
            return ErrorValidacion(
                "Una dieta archivada no puede volver a activarse."
            );
        }


        if (dieta.Estado ==
            EstadoDieta.Activa)
        {
            return ErrorValidacion(
                "La dieta ya se encuentra activa."
            );
        }


        /*
         * Solo permitimos una dieta activa
         * por paciente.
         *
         * Si existe una anterior,
         * la archivamos automáticamente.
         */

        var dietasActivasAnteriores =
            await _context.Dietas
                .Where(d =>
                    d.PacienteId == pacienteId
                    &&
                    d.Id != dietaId
                    &&
                    d.Estado ==
                    EstadoDieta.Activa
                )
                .ToListAsync();


        foreach (var anterior
                 in dietasActivasAnteriores)
        {
            anterior.Estado =
                EstadoDieta.Archivada;

            anterior.FechaFin ??=
                DateOnly.FromDateTime(
                    DateTime.UtcNow
                );

            anterior.FechaActualizacion =
                DateTime.UtcNow;
        }


        dieta.Estado =
            EstadoDieta.Activa;

        dieta.FechaActualizacion =
            DateTime.UtcNow;


        await _context.SaveChangesAsync();


        return new ResultadoDieta<DietaDetalleDto>
        {
            Exitoso = true,

            Datos =
                MapearDetalle(
                    dieta,
                    dieta.Comidas.Count
                ),

            TipoError =
                TipoErrorDieta.Ninguno
        };
    }


    // ==========================================
    // ARCHIVAR
    // ==========================================

    public async Task<ResultadoDieta<DietaDetalleDto>>
        ArchivarAsync(
            int nutricionistaId,
            int pacienteId,
            int dietaId)
    {
        var dieta =
            await _context.Dietas
                .Include(d => d.Comidas)
                .FirstOrDefaultAsync(d =>
                    d.Id == dietaId
                    &&
                    d.PacienteId == pacienteId
                    &&
                    d.Paciente.NutricionistaId ==
                    nutricionistaId
                );


        if (dieta is null)
        {
            return DietaNoEncontrada();
        }


        if (dieta.Estado ==
            EstadoDieta.Archivada)
        {
            return ErrorValidacion(
                "La dieta ya se encuentra archivada."
            );
        }


        dieta.Estado =
            EstadoDieta.Archivada;

        dieta.FechaFin ??=
            DateOnly.FromDateTime(
                DateTime.UtcNow
            );

        dieta.FechaActualizacion =
            DateTime.UtcNow;


        await _context.SaveChangesAsync();


        return new ResultadoDieta<DietaDetalleDto>
        {
            Exitoso = true,

            Datos =
                MapearDetalle(
                    dieta,
                    dieta.Comidas.Count
                ),

            TipoError =
                TipoErrorDieta.Ninguno
        };
    }


    // ==========================================
    // MAPPER
    // ==========================================

    private static DietaDetalleDto MapearDetalle(
        Dieta dieta,
        int cantidadComidas)
    {
        return new DietaDetalleDto
        {
            Id = dieta.Id,

            PacienteId =
                dieta.PacienteId,

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

            CantidadComidas =
                cantidadComidas,

            FechaCreacion =
                dieta.FechaCreacion,

            FechaActualizacion =
                dieta.FechaActualizacion
        };
    }


    // ==========================================
    // ERRORES
    // ==========================================

    private static ResultadoDieta<DietaDetalleDto>
        DietaNoEncontrada(
            string mensaje =
                "Dieta no encontrada.")
    {
        return new ResultadoDieta<DietaDetalleDto>
        {
            Exitoso = false,

            Error = mensaje,

            TipoError =
                TipoErrorDieta.NoEncontrado
        };
    }


    private static ResultadoDieta<DietaDetalleDto>
        ErrorValidacion(
            string mensaje)
    {
        return new ResultadoDieta<DietaDetalleDto>
        {
            Exitoso = false,

            Error = mensaje,

            TipoError =
                TipoErrorDieta.Validacion
        };
    }
}