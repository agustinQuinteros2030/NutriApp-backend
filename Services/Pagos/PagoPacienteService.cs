using Microsoft.EntityFrameworkCore;

using NutriApi.DTOs.Pagos;

using NutriApp.Data;
using NutriApp.Models.Pagos;

namespace NutriApi.Services.Pagos;

public class PagoPacienteService
    : IPagoPacienteService
{
    private readonly NutriAppDbContext _context;


    public PagoPacienteService(
        NutriAppDbContext context)
    {
        _context = context;
    }


    // ==========================================
    // CREAR PAGO
    // ==========================================

    public async Task<
        ResultadoPago<PagoPacienteDto>>
        CrearAsync(
            int nutricionistaId,
            int pacienteId,
            CrearPagoPacienteDto dto)
    {
        var pacienteExiste =
            await PacientePerteneceAsync(
                nutricionistaId,
                pacienteId
            );


        if (!pacienteExiste)
        {
            return Error<PagoPacienteDto>(
                "Paciente no encontrado.",
                TipoErrorPago.NoEncontrado
            );
        }


        var errorValidacion =
            ValidarPago(
                dto.FechaPago,
                dto.ProximoVencimiento,
                dto.Monto
            );


        if (errorValidacion is not null)
        {
            return Error<PagoPacienteDto>(
                errorValidacion,
                TipoErrorPago.Validacion
            );
        }


        var pago =
            new PagoPaciente
            {
                PacienteId =
                    pacienteId,

                FechaPago =
                    dto.FechaPago,

                ProximoVencimiento =
                    dto.ProximoVencimiento,

                Monto =
                    dto.Monto,

                MetodoPago =
                    Limpiar(
                        dto.MetodoPago
                    ),

                Observaciones =
                    Limpiar(
                        dto.Observaciones
                    ),

                FechaCreacion =
                    DateTime.UtcNow
            };


        _context.PagosPacientes
            .Add(pago);


        await _context.SaveChangesAsync();


        return new ResultadoPago<PagoPacienteDto>
        {
            Exitoso = true,

            Datos =
                MapearPago(
                    pago
                ),

            TipoError =
                TipoErrorPago.Ninguno
        };
    }


    // ==========================================
    // HISTORIAL
    // ==========================================

    public async Task<
        ResultadoPago<List<PagoPacienteDto>>>
        ObtenerTodosAsync(
            int nutricionistaId,
            int pacienteId)
    {
        var pacienteExiste =
            await PacientePerteneceAsync(
                nutricionistaId,
                pacienteId
            );


        if (!pacienteExiste)
        {
            return Error<List<PagoPacienteDto>>(
                "Paciente no encontrado.",
                TipoErrorPago.NoEncontrado
            );
        }


        var pagos =
            await _context.PagosPacientes
                .AsNoTracking()
                .Where(p =>
                    p.PacienteId ==
                    pacienteId
                )
                .OrderByDescending(p =>
                    p.FechaPago
                )
                .ThenByDescending(p =>
                    p.Id
                )
                .Select(p =>
                    new PagoPacienteDto
                    {
                        Id =
                            p.Id,

                        PacienteId =
                            p.PacienteId,

                        FechaPago =
                            p.FechaPago,

                        ProximoVencimiento =
                            p.ProximoVencimiento,

                        Monto =
                            p.Monto,

                        MetodoPago =
                            p.MetodoPago,

                        Observaciones =
                            p.Observaciones,

                        FechaCreacion =
                            p.FechaCreacion
                    }
                )
                .ToListAsync();


        return new ResultadoPago<
            List<PagoPacienteDto>>
        {
            Exitoso = true,

            Datos =
                pagos,

            TipoError =
                TipoErrorPago.Ninguno
        };
    }


    // ==========================================
    // ESTADO ACTUAL
    // ==========================================

    public async Task<
        ResultadoPago<EstadoPagoPacienteDto>>
        ObtenerEstadoAsync(
            int nutricionistaId,
            int pacienteId)
    {
        var pacienteExiste =
            await PacientePerteneceAsync(
                nutricionistaId,
                pacienteId
            );


        if (!pacienteExiste)
        {
            return Error<EstadoPagoPacienteDto>(
                "Paciente no encontrado.",
                TipoErrorPago.NoEncontrado
            );
        }


        /*
         * Tomamos el pago más reciente.
         */

        var ultimoPago =
            await _context.PagosPacientes
                .AsNoTracking()
                .Where(p =>
                    p.PacienteId ==
                    pacienteId
                )
                .OrderByDescending(p =>
                    p.FechaPago
                )
                .ThenByDescending(p =>
                    p.Id
                )
                .FirstOrDefaultAsync();


        if (ultimoPago is null)
        {
            return new ResultadoPago<
                EstadoPagoPacienteDto>
            {
                Exitoso = true,

                Datos =
                    new EstadoPagoPacienteDto
                    {
                        TienePagos =
                            false,

                        Estado =
                            "SinRegistro"
                    },

                TipoError =
                    TipoErrorPago.Ninguno
            };
        }


        var hoy =
            DateOnly.FromDateTime(
                DateTime.UtcNow
            );


        var diferenciaDias =
            ultimoPago
                .ProximoVencimiento
                .DayNumber
            -
            hoy.DayNumber;


        var vencido =
            diferenciaDias < 0;


        var venceHoy =
            diferenciaDias == 0;


        string estado;


        if (vencido)
        {
            estado =
                "Vencido";
        }
        else if (venceHoy)
        {
            estado =
                "VenceHoy";
        }
        else
        {
            estado =
                "AlDia";
        }


        var dto =
            new EstadoPagoPacienteDto
            {
                TienePagos =
                    true,

                UltimoPago =
                    ultimoPago.FechaPago,

                ProximoVencimiento =
                    ultimoPago.ProximoVencimiento,

                MontoUltimoPago =
                    ultimoPago.Monto,

                Estado =
                    estado,

                Vencido =
                    vencido,

                VenceHoy =
                    venceHoy,

                DiasParaVencer =
                    diferenciaDias > 0
                        ? diferenciaDias
                        : 0,

                DiasVencido =
                    diferenciaDias < 0
                        ? Math.Abs(
                            diferenciaDias
                        )
                        : 0
            };


        return new ResultadoPago<
            EstadoPagoPacienteDto>
        {
            Exitoso = true,

            Datos =
                dto,

            TipoError =
                TipoErrorPago.Ninguno
        };
    }


    // ==========================================
    // EDITAR PAGO
    // ==========================================

    public async Task<
        ResultadoPago<PagoPacienteDto>>
        EditarAsync(
            int nutricionistaId,
            int pacienteId,
            int pagoId,
            EditarPagoPacienteDto dto)
    {
        var pago =
            await _context.PagosPacientes
                .FirstOrDefaultAsync(p =>
                    p.Id ==
                    pagoId
                    &&
                    p.PacienteId ==
                    pacienteId
                    &&
                    p.Paciente
                        .NutricionistaId ==
                    nutricionistaId
                );


        if (pago is null)
        {
            return Error<PagoPacienteDto>(
                "Pago no encontrado.",
                TipoErrorPago.NoEncontrado
            );
        }


        var errorValidacion =
            ValidarPago(
                dto.FechaPago,
                dto.ProximoVencimiento,
                dto.Monto
            );


        if (errorValidacion is not null)
        {
            return Error<PagoPacienteDto>(
                errorValidacion,
                TipoErrorPago.Validacion
            );
        }


        pago.FechaPago =
            dto.FechaPago;

        pago.ProximoVencimiento =
            dto.ProximoVencimiento;

        pago.Monto =
            dto.Monto;

        pago.MetodoPago =
            Limpiar(
                dto.MetodoPago
            );

        pago.Observaciones =
            Limpiar(
                dto.Observaciones
            );


        await _context.SaveChangesAsync();


        return new ResultadoPago<PagoPacienteDto>
        {
            Exitoso = true,

            Datos =
                MapearPago(
                    pago
                ),

            TipoError =
                TipoErrorPago.Ninguno
        };
    }


    // ==========================================
    // OWNERSHIP
    // ==========================================

    private async Task<bool>
        PacientePerteneceAsync(
            int nutricionistaId,
            int pacienteId)
    {
        return await _context.Pacientes
            .AsNoTracking()
            .AnyAsync(p =>
                p.Id ==
                pacienteId
                &&
                p.NutricionistaId ==
                nutricionistaId
            );
    }


    // ==========================================
    // VALIDACIÓN
    // ==========================================

    private static string?
        ValidarPago(
            DateOnly fechaPago,
            DateOnly proximoVencimiento,
            decimal? monto)
    {
        if (proximoVencimiento <=
            fechaPago)
        {
            return
                "El próximo vencimiento debe ser posterior a la fecha de pago.";
        }


        if (monto.HasValue &&
            monto.Value <= 0)
        {
            return
                "El monto debe ser mayor a cero.";
        }


        return null;
    }


    // ==========================================
    // MAPPER
    // ==========================================

    private static PagoPacienteDto
        MapearPago(
            PagoPaciente pago)
    {
        return new PagoPacienteDto
        {
            Id =
                pago.Id,

            PacienteId =
                pago.PacienteId,

            FechaPago =
                pago.FechaPago,

            ProximoVencimiento =
                pago.ProximoVencimiento,

            Monto =
                pago.Monto,

            MetodoPago =
                pago.MetodoPago,

            Observaciones =
                pago.Observaciones,

            FechaCreacion =
                pago.FechaCreacion
        };
    }


    // ==========================================
    // LIMPIAR STRING
    // ==========================================

    private static string?
        Limpiar(
            string? valor)
    {
        if (string.IsNullOrWhiteSpace(
            valor))
        {
            return null;
        }


        return valor.Trim();
    }


    // ==========================================
    // ERROR
    // ==========================================

    private static ResultadoPago<T>
        Error<T>(
            string mensaje,
            TipoErrorPago tipo)
    {
        return new ResultadoPago<T>
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