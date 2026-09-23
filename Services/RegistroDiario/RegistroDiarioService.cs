using Microsoft.EntityFrameworkCore;

using NutriApi.DTOs.RegistroDiario;

using NutriApp.Data;
using NutriApp.Models.Seguimiento;

namespace NutriApi.Services.RegistroDiario;

public class RegistroDiarioService
    : IRegistroDiarioService
{
    private readonly NutriAppDbContext
        _context;


    public RegistroDiarioService(
        NutriAppDbContext context)
    {
        _context =
            context;
    }


    // ==========================================
    // GUARDAR REGISTRO PROPIO
    // CREATE / UPDATE POR FECHA
    // ==========================================

    public async Task<
        ResultadoRegistroDiario<
            RegistroDiarioPacienteDto>>
        GuardarPropioAsync(
            int pacienteId,
            GuardarRegistroDiarioDto dto)
    {
        var pacienteExiste =
            await _context.Pacientes
                .AsNoTracking()
                .AnyAsync(p =>
                    p.Id ==
                    pacienteId
                );


        if (!pacienteExiste)
        {
            return Error<
                RegistroDiarioPacienteDto>(
                "Paciente no encontrado.",
                TipoErrorRegistroDiario
                    .NoEncontrado
            );
        }


        var errorValidacion =
            Validar(
                dto
            );


        if (errorValidacion is not null)
        {
            return Error<
                RegistroDiarioPacienteDto>(
                errorValidacion,
                TipoErrorRegistroDiario
                    .Validacion
            );
        }


        var registro =
            await _context
                .RegistrosDiariosPacientes
                .FirstOrDefaultAsync(r =>
                    r.PacienteId ==
                    pacienteId
                    &&
                    r.Fecha ==
                    dto.Fecha
                );


        /*
         * Un paciente tiene como máximo
         * un registro por fecha.
         *
         * Si no existe:
         *     creamos.
         *
         * Si existe:
         *     actualizamos.
         */

        if (registro is null)
        {
            registro =
                new RegistroDiarioPaciente
                {
                    PacienteId =
                        pacienteId,

                    Fecha =
                        dto.Fecha,

                    CumplioPlan =
                        dto.CumplioPlan,

                    CinturaCm =
                        dto.CinturaCm,

                    CaderaCm =
                        dto.CaderaCm,

                    GemeloCm =
                        dto.GemeloCm,

                    CuelloCm =
                        dto.CuelloCm,

                    Observaciones =
                        Limpiar(
                            dto.Observaciones
                        ),

                    FechaCreacion =
                        DateTime.UtcNow
                };


            _context
                .RegistrosDiariosPacientes
                .Add(
                    registro
                );
        }
        else
        {
            registro.CumplioPlan =
                dto.CumplioPlan;

            registro.CinturaCm =
                dto.CinturaCm;

            registro.CaderaCm =
                dto.CaderaCm;

            registro.GemeloCm =
                dto.GemeloCm;

            registro.CuelloCm =
                dto.CuelloCm;

            registro.Observaciones =
                Limpiar(
                    dto.Observaciones
                );

            registro.FechaActualizacion =
                DateTime.UtcNow;
        }


        await _context
            .SaveChangesAsync();


        return Exito(
            Mapear(
                registro
            )
        );
    }


    // ==========================================
    // PACIENTE - HISTORIAL PROPIO
    // ==========================================

    public async Task<
        ResultadoRegistroDiario<
            List<RegistroDiarioPacienteDto>>>
        ObtenerPropiosAsync(
            int pacienteId)
    {
        var registros =
            await _context
                .RegistrosDiariosPacientes
                .AsNoTracking()
                .Where(r =>
                    r.PacienteId ==
                    pacienteId
                )
                .OrderByDescending(r =>
                    r.Fecha
                )
                .ThenByDescending(r =>
                    r.Id
                )
                .Select(r =>
                    new RegistroDiarioPacienteDto
                    {
                        Id =
                            r.Id,

                        PacienteId =
                            r.PacienteId,

                        Fecha =
                            r.Fecha,

                        CumplioPlan =
                            r.CumplioPlan,

                        CinturaCm =
                            r.CinturaCm,

                        CaderaCm =
                            r.CaderaCm,

                        GemeloCm =
                            r.GemeloCm,

                        CuelloCm =
                            r.CuelloCm,

                        Observaciones =
                            r.Observaciones,

                        FechaCreacion =
                            r.FechaCreacion,

                        FechaActualizacion =
                            r.FechaActualizacion
                    }
                )
                .ToListAsync();


        return new ResultadoRegistroDiario<
            List<RegistroDiarioPacienteDto>>
        {
            Exitoso =
                true,

            Datos =
                registros,

            TipoError =
                TipoErrorRegistroDiario
                    .Ninguno
        };
    }


    // ==========================================
    // PACIENTE - REGISTRO POR FECHA
    // ==========================================

    public async Task<
        ResultadoRegistroDiario<
            RegistroDiarioPacienteDto>>
        ObtenerPropioPorFechaAsync(
            int pacienteId,
            DateOnly fecha)
    {
        var registro =
            await _context
                .RegistrosDiariosPacientes
                .AsNoTracking()
                .FirstOrDefaultAsync(r =>
                    r.PacienteId ==
                    pacienteId
                    &&
                    r.Fecha ==
                    fecha
                );


        if (registro is null)
        {
            return Error<
                RegistroDiarioPacienteDto>(
                "No existe un registro para esa fecha.",
                TipoErrorRegistroDiario
                    .NoEncontrado
            );
        }


        return Exito(
            Mapear(
                registro
            )
        );
    }


    // ==========================================
    // NUTRICIONISTA - HISTORIAL PACIENTE
    // ==========================================

    public async Task<
        ResultadoRegistroDiario<
            List<RegistroDiarioPacienteDto>>>
        ObtenerPacienteAsync(
            int nutricionistaId,
            int pacienteId)
    {
        /*
         * Ownership:
         *
         * El nutricionista solamente puede
         * consultar registros pertenecientes
         * a sus pacientes.
         */

        var pacientePertenece =
            await _context.Pacientes
                .AsNoTracking()
                .AnyAsync(p =>
                    p.Id ==
                    pacienteId
                    &&
                    p.NutricionistaId ==
                    nutricionistaId
                );


        if (!pacientePertenece)
        {
            return Error<
                List<RegistroDiarioPacienteDto>>(
                "Paciente no encontrado.",
                TipoErrorRegistroDiario
                    .NoEncontrado
            );
        }


        var registros =
            await _context
                .RegistrosDiariosPacientes
                .AsNoTracking()
                .Where(r =>
                    r.PacienteId ==
                    pacienteId
                )
                .OrderByDescending(r =>
                    r.Fecha
                )
                .ThenByDescending(r =>
                    r.Id
                )
                .Select(r =>
                    new RegistroDiarioPacienteDto
                    {
                        Id =
                            r.Id,

                        PacienteId =
                            r.PacienteId,

                        Fecha =
                            r.Fecha,

                        CumplioPlan =
                            r.CumplioPlan,

                        CinturaCm =
                            r.CinturaCm,

                        CaderaCm =
                            r.CaderaCm,

                        GemeloCm =
                            r.GemeloCm,

                        CuelloCm =
                            r.CuelloCm,

                        Observaciones =
                            r.Observaciones,

                        FechaCreacion =
                            r.FechaCreacion,

                        FechaActualizacion =
                            r.FechaActualizacion
                    }
                )
                .ToListAsync();


        return new ResultadoRegistroDiario<
            List<RegistroDiarioPacienteDto>>
        {
            Exitoso =
                true,

            Datos =
                registros,

            TipoError =
                TipoErrorRegistroDiario
                    .Ninguno
        };
    }


    // ==========================================
    // VALIDACIÓN
    // ==========================================

    private static string?
        Validar(
            GuardarRegistroDiarioDto dto)
    {
        if (dto.CinturaCm.HasValue &&
            dto.CinturaCm <= 0)
        {
            return
                "La medida de cintura debe ser mayor a cero.";
        }


        if (dto.CaderaCm.HasValue &&
            dto.CaderaCm <= 0)
        {
            return
                "La medida de cadera debe ser mayor a cero.";
        }


        if (dto.GemeloCm.HasValue &&
            dto.GemeloCm <= 0)
        {
            return
                "La medida de gemelo debe ser mayor a cero.";
        }


        if (dto.CuelloCm.HasValue &&
            dto.CuelloCm <= 0)
        {
            return
                "La medida de cuello debe ser mayor a cero.";
        }


        return null;
    }


    // ==========================================
    // MAPPER
    // ==========================================

    private static RegistroDiarioPacienteDto
        Mapear(
            RegistroDiarioPaciente registro)
    {
        return new RegistroDiarioPacienteDto
        {
            Id =
                registro.Id,

            PacienteId =
                registro.PacienteId,

            Fecha =
                registro.Fecha,

            CumplioPlan =
                registro.CumplioPlan,

            CinturaCm =
                registro.CinturaCm,

            CaderaCm =
                registro.CaderaCm,

            GemeloCm =
                registro.GemeloCm,

            CuelloCm =
                registro.CuelloCm,

            Observaciones =
                registro.Observaciones,

            FechaCreacion =
                registro.FechaCreacion,

            FechaActualizacion =
                registro.FechaActualizacion
        };
    }


    // ==========================================
    // HELPERS
    // ==========================================

    private static string?
        Limpiar(
            string? texto)
    {
        return string.IsNullOrWhiteSpace(
            texto)
            ? null
            : texto.Trim();
    }


    private static ResultadoRegistroDiario<
        RegistroDiarioPacienteDto>
        Exito(
            RegistroDiarioPacienteDto dto)
    {
        return new ResultadoRegistroDiario<
            RegistroDiarioPacienteDto>
        {
            Exitoso =
                true,

            Datos =
                dto,

            TipoError =
                TipoErrorRegistroDiario
                    .Ninguno
        };
    }


    private static ResultadoRegistroDiario<T>
        Error<T>(
            string mensaje,
            TipoErrorRegistroDiario tipo)
    {
        return new ResultadoRegistroDiario<T>
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