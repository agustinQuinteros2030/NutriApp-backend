using Microsoft.EntityFrameworkCore;

using NutriApi.DTOs.RegistroDiario;

using NutriApp.Data;
using NutriApp.Models.Seguimiento;

namespace NutriApi.Services.RegistroDiario;

public class RegistroDiarioService
    : IRegistroDiarioService
{
    private readonly NutriAppDbContext _context;


    public RegistroDiarioService(
        NutriAppDbContext context)
    {
        _context = context;
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
                    p.Id == pacienteId
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
            Validar(dto);


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
         * Si no existe, creamos.
         *
         * Si existe, actualizamos.
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

                    AdherenciaPorcentaje =
                        dto.AdherenciaPorcentaje,

                    Hambre =
                        dto.Hambre,

                    Energia =
                        dto.Energia,

                    Entreno =
                        dto.Entreno,

                    Observaciones =
                        Limpiar(
                            dto.Observaciones
                        ),

                    FechaCreacion =
                        DateTime.UtcNow
                };


            _context
                .RegistrosDiariosPacientes
                .Add(registro);
        }
        else
        {
            registro.AdherenciaPorcentaje =
                dto.AdherenciaPorcentaje;

            registro.Hambre =
                dto.Hambre;

            registro.Energia =
                dto.Energia;

            registro.Entreno =
                dto.Entreno;

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
            Mapear(registro)
        );
    }


    // ==========================================
    // HISTORIAL DEL PACIENTE AUTENTICADO
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

                        AdherenciaPorcentaje =
                            r.AdherenciaPorcentaje,

                        Hambre =
                            r.Hambre,

                        Energia =
                            r.Energia,

                        Entreno =
                            r.Entreno,

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
    // REGISTRO PROPIO POR FECHA
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
            Mapear(registro)
        );
    }


    // ==========================================
    // HISTORIAL PARA NUTRICIONISTA
    // ==========================================

    public async Task<
        ResultadoRegistroDiario<
            List<RegistroDiarioPacienteDto>>>
        ObtenerPacienteAsync(
            int nutricionistaId,
            int pacienteId)
    {
        /*
         * Ownership.
         *
         * Un nutricionista solamente puede
         * consultar registros de sus pacientes.
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

                        AdherenciaPorcentaje =
                            r.AdherenciaPorcentaje,

                        Hambre =
                            r.Hambre,

                        Energia =
                            r.Energia,

                        Entreno =
                            r.Entreno,

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
        if (dto.AdherenciaPorcentaje.HasValue &&
            (
                dto.AdherenciaPorcentaje < 0
                ||
                dto.AdherenciaPorcentaje > 100
            ))
        {
            return
                "La adherencia debe estar entre 0 y 100.";
        }


        if (dto.Hambre.HasValue &&
            (
                dto.Hambre < 1
                ||
                dto.Hambre > 5
            ))
        {
            return
                "El hambre debe estar entre 1 y 5.";
        }


        if (dto.Energia.HasValue &&
            (
                dto.Energia < 1
                ||
                dto.Energia > 5
            ))
        {
            return
                "La energía debe estar entre 1 y 5.";
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

            AdherenciaPorcentaje =
                registro.AdherenciaPorcentaje,

            Hambre =
                registro.Hambre,

            Energia =
                registro.Energia,

            Entreno =
                registro.Entreno,

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
            texto
        )
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