using Microsoft.EntityFrameworkCore;

using NutriApi.DTOs.Notas;

using NutriApp.Data;
using NutriApp.Models.Pacientes;

namespace NutriApi.Services.Notas;

public class NotaPacienteService
    : INotaPacienteService
{
    private readonly NutriAppDbContext _context;


    public NotaPacienteService(
        NutriAppDbContext context)
    {
        _context = context;
    }


    // ==========================================
    // CREAR NOTA
    // ==========================================

    public async Task<
        ResultadoNotaPaciente<NotaPacienteDto>>
        CrearAsync(
            int nutricionistaId,
            int pacienteId,
            CrearNotaPacienteDto dto)
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
            return Error<NotaPacienteDto>(
                "Paciente no encontrado.",
                TipoErrorNotaPaciente.NoEncontrado
            );
        }


        var contenido =
            dto.Contenido.Trim();


        if (string.IsNullOrWhiteSpace(contenido))
        {
            return Error<NotaPacienteDto>(
                "El contenido de la nota es obligatorio.",
                TipoErrorNotaPaciente.Validacion
            );
        }


        var nota =
            new NotaPaciente
            {
                PacienteId =
                    pacienteId,

                NutricionistaId =
                    nutricionistaId,

                Contenido =
                    contenido,

                FechaCreacion =
                    DateTime.UtcNow
            };


        _context.NotasPacientes.Add(
            nota
        );


        await _context.SaveChangesAsync();


        return new ResultadoNotaPaciente<
            NotaPacienteDto>
        {
            Exitoso = true,

            Datos =
                MapearNota(nota),

            TipoError =
                TipoErrorNotaPaciente.Ninguno
        };
    }


    // ==========================================
    // LISTAR NOTAS
    // ==========================================

    public async Task<
        ResultadoNotaPaciente<
            List<NotaPacienteDto>>>
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
            return Error<
                List<NotaPacienteDto>>(
                "Paciente no encontrado.",
                TipoErrorNotaPaciente.NoEncontrado
            );
        }


        var notas =
            await _context.NotasPacientes
                .AsNoTracking()
                .Where(n =>
                    n.PacienteId ==
                    pacienteId
                    &&
                    n.NutricionistaId ==
                    nutricionistaId
                )
                .OrderByDescending(n =>
                    n.FechaCreacion
                )
                .Select(n =>
                    new NotaPacienteDto
                    {
                        Id =
                            n.Id,

                        PacienteId =
                            n.PacienteId,

                        Contenido =
                            n.Contenido,

                        FechaCreacion =
                            n.FechaCreacion,

                        FechaActualizacion =
                            n.FechaActualizacion
                    }
                )
                .ToListAsync();


        return new ResultadoNotaPaciente<
            List<NotaPacienteDto>>
        {
            Exitoso = true,

            Datos =
                notas,

            TipoError =
                TipoErrorNotaPaciente.Ninguno
        };
    }


    // ==========================================
    // OBTENER NOTA
    // ==========================================

    public async Task<
        ResultadoNotaPaciente<NotaPacienteDto>>
        ObtenerPorIdAsync(
            int nutricionistaId,
            int pacienteId,
            int notaId)
    {
        var nota =
            await _context.NotasPacientes
                .AsNoTracking()
                .Where(n =>
                    n.Id ==
                    notaId
                    &&
                    n.PacienteId ==
                    pacienteId
                    &&
                    n.NutricionistaId ==
                    nutricionistaId
                    &&
                    n.Paciente.NutricionistaId ==
                    nutricionistaId
                )
                .Select(n =>
                    new NotaPacienteDto
                    {
                        Id =
                            n.Id,

                        PacienteId =
                            n.PacienteId,

                        Contenido =
                            n.Contenido,

                        FechaCreacion =
                            n.FechaCreacion,

                        FechaActualizacion =
                            n.FechaActualizacion
                    }
                )
                .FirstOrDefaultAsync();


        if (nota is null)
        {
            return Error<NotaPacienteDto>(
                "Nota no encontrada.",
                TipoErrorNotaPaciente.NoEncontrado
            );
        }


        return new ResultadoNotaPaciente<
            NotaPacienteDto>
        {
            Exitoso = true,

            Datos =
                nota,

            TipoError =
                TipoErrorNotaPaciente.Ninguno
        };
    }


    // ==========================================
    // EDITAR NOTA
    // ==========================================

    public async Task<
        ResultadoNotaPaciente<NotaPacienteDto>>
        EditarAsync(
            int nutricionistaId,
            int pacienteId,
            int notaId,
            EditarNotaPacienteDto dto)
    {
        var nota =
            await _context.NotasPacientes
                .Include(n =>
                    n.Paciente
                )
                .FirstOrDefaultAsync(n =>
                    n.Id ==
                    notaId
                    &&
                    n.PacienteId ==
                    pacienteId
                    &&
                    n.NutricionistaId ==
                    nutricionistaId
                    &&
                    n.Paciente.NutricionistaId ==
                    nutricionistaId
                );


        if (nota is null)
        {
            return Error<NotaPacienteDto>(
                "Nota no encontrada.",
                TipoErrorNotaPaciente.NoEncontrado
            );
        }


        var contenido =
            dto.Contenido.Trim();


        if (string.IsNullOrWhiteSpace(contenido))
        {
            return Error<NotaPacienteDto>(
                "El contenido de la nota es obligatorio.",
                TipoErrorNotaPaciente.Validacion
            );
        }


        nota.Contenido =
            contenido;

        nota.FechaActualizacion =
            DateTime.UtcNow;


        await _context.SaveChangesAsync();


        return new ResultadoNotaPaciente<
            NotaPacienteDto>
        {
            Exitoso = true,

            Datos =
                MapearNota(nota),

            TipoError =
                TipoErrorNotaPaciente.Ninguno
        };
    }


    // ==========================================
    // MAPPER
    // ==========================================

    private static NotaPacienteDto
        MapearNota(
            NotaPaciente nota)
    {
        return new NotaPacienteDto
        {
            Id =
                nota.Id,

            PacienteId =
                nota.PacienteId,

            Contenido =
                nota.Contenido,

            FechaCreacion =
                nota.FechaCreacion,

            FechaActualizacion =
                nota.FechaActualizacion
        };
    }


    // ==========================================
    // ERROR
    // ==========================================

    private static ResultadoNotaPaciente<T>
        Error<T>(
            string mensaje,
            TipoErrorNotaPaciente tipo)
    {
        return new ResultadoNotaPaciente<T>
        {
            Exitoso = false,

            Error =
                mensaje,

            TipoError =
                tipo
        };
    }
}