using Microsoft.EntityFrameworkCore;

using NutriApi.DTOs.Dashboard;

using NutriApp.Data;
using NutriApp.Enums;

namespace NutriApi.Services.Dashboard;

public class DashboardNutricionistaService
    : IDashboardNutricionistaService
{
    private readonly NutriAppDbContext
        _context;


    private const int
        DiasProximosVencimientos =
            7;


    public DashboardNutricionistaService(
        NutriAppDbContext context)
    {
        _context =
            context;
    }


    public async Task<DashboardNutricionistaDto>
        ObtenerAsync(
            int nutricionistaId)
    {
        var hoy =
            ObtenerFechaActualArgentina();


        var limiteProximosVencimientos =
            hoy.AddDays(
                DiasProximosVencimientos
            );


        // ======================================
        // PACIENTES
        // ======================================

        var pacientesQuery =
            _context.Pacientes
                .AsNoTracking()
                .Where(p =>
                    p.NutricionistaId ==
                    nutricionistaId
                );


        var totalPacientes =
            await pacientesQuery
                .CountAsync();


        var pacientesActivos =
            await pacientesQuery
                .CountAsync(p =>
                    p.Activo
                );


        var pacientesInactivos =
            await pacientesQuery
                .CountAsync(p =>
                    !p.Activo
                );


        /*
         * Los pacientes son creados inicialmente
         * sin contraseña.
         *
         * Identity establece PasswordHash cuando
         * el paciente activa la cuenta.
         */

        var pendientesActivacion =
            await pacientesQuery
                .CountAsync(p =>
                    p.PasswordHash == null
                );


        // ======================================
        // DIETAS
        // ======================================

        var dietasQuery =
            _context.Dietas
                .AsNoTracking()
                .Where(d =>
                    d.Paciente
                        .NutricionistaId ==
                    nutricionistaId
                );


        var dietasBorrador =
            await dietasQuery
                .CountAsync(d =>
                    d.Estado ==
                    EstadoDieta.Borrador
                );


        var dietasActivas =
            await dietasQuery
                .CountAsync(d =>
                    d.Estado ==
                    EstadoDieta.Activa
                );


        var dietasArchivadas =
            await dietasQuery
                .CountAsync(d =>
                    d.Estado ==
                    EstadoDieta.Archivada
                );


        // ======================================
        // SEGUIMIENTOS PENDIENTES
        // ======================================

        /*
         * Un seguimiento está pendiente mientras
         * no tenga FechaRevisionNutricionista.
         *
         * Solamente mostramos pacientes activos
         * del nutricionista autenticado.
         */

        var seguimientosPendientesQuery =
            _context
                .SeguimientosSemanalesPacientes
                .AsNoTracking()
                .Where(s =>
                    s.Paciente
                        .NutricionistaId ==
                    nutricionistaId
                    &&
                    s.Paciente.Activo
                    &&
                    s.FechaRevisionNutricionista ==
                    null
                );


        var cantidadSeguimientosPendientes =
            await seguimientosPendientesQuery
                .CountAsync();


        /*
         * Para la lista del dashboard mostramos
         * solamente los 5 más antiguos.
         *
         * El contador sí contiene TODOS.
         */

        var seguimientosPendientes =
            await seguimientosPendientesQuery
                .OrderBy(s =>
                    s.FechaRespuesta
                )
                .Take(5)
                .Select(s =>
                    new SeguimientoPendienteDashboardDto
                    {
                        SeguimientoId =
                            s.Id,

                        PacienteId =
                            s.PacienteId,

                        NombrePaciente =
                            s.Paciente.Nombre +
                            " " +
                            s.Paciente.Apellido,

                        FechaInicioSemana =
                            s.FechaInicioSemana,

                        FechaFinSemana =
                            s.FechaFinSemana,

                        FechaRespuesta =
                            s.FechaRespuesta
                    }
                )
                .ToListAsync();


        // ======================================
        // ÚLTIMO PAGO POR PACIENTE
        // ======================================

        /*
         * Para determinar el estado actual del
         * cobro NO debemos mirar todos los pagos
         * históricos.
         *
         * Tomamos el último pago registrado para
         * cada paciente y usamos su
         * ProximoVencimiento.
         */

        var ultimosPagos =
            await _context
                .PagosPacientes
                .AsNoTracking()
                .Where(p =>
                    p.Paciente
                        .NutricionistaId ==
                    nutricionistaId
                    &&
                    p.Paciente.Activo
                )
                .GroupBy(p =>
                    p.PacienteId
                )
                .Select(grupo =>
                    grupo
                        .OrderByDescending(p =>
                            p.FechaPago
                        )
                        .ThenByDescending(p =>
                            p.Id
                        )
                        .Select(p =>
                            new
                            {
                                p.PacienteId,

                                NombrePaciente =
                                    p.Paciente.Nombre +
                                    " " +
                                    p.Paciente.Apellido,

                                UltimoPago =
                                    p.FechaPago,

                                p.ProximoVencimiento
                            }
                        )
                        .First()
                )
                .ToListAsync();


        // ======================================
        // COBROS VENCIDOS
        // ======================================

        var cobrosVencidosTodos =
            ultimosPagos
                .Where(p =>
                    p.ProximoVencimiento <
                    hoy
                )
                .OrderBy(p =>
                    p.ProximoVencimiento
                )
                .ToList();


        var cantidadCobrosVencidos =
            cobrosVencidosTodos.Count;


        /*
         * Igual que seguimientos:
         *
         * contador -> todos
         * detalle  -> primeros 5
         */

        var cobrosVencidos =
            cobrosVencidosTodos
                .Take(5)
                .Select(p =>
                    new CobroVencidoDashboardDto
                    {
                        PacienteId =
                            p.PacienteId,

                        NombrePaciente =
                            p.NombrePaciente,

                        UltimoPago =
                            p.UltimoPago,

                        ProximoVencimiento =
                            p.ProximoVencimiento,

                        DiasVencido =
                            hoy.DayNumber -
                            p.ProximoVencimiento
                                .DayNumber
                    }
                )
                .ToList();


        // ======================================
        // COBROS PRÓXIMOS A VENCER
        // ======================================

        /*
         * Incluimos:
         *
         * hoy
         * hasta
         * hoy + 7 días
         *
         * Un vencimiento de hoy NO está vencido.
         */

        var cobrosProximosTodos =
            ultimosPagos
                .Where(p =>
                    p.ProximoVencimiento >=
                    hoy
                    &&
                    p.ProximoVencimiento <=
                    limiteProximosVencimientos
                )
                .OrderBy(p =>
                    p.ProximoVencimiento
                )
                .ToList();


        var cantidadCobrosProximos =
            cobrosProximosTodos.Count;


        var cobrosProximos =
            cobrosProximosTodos
                .Take(5)
                .Select(p =>
                    new CobroProximoDashboardDto
                    {
                        PacienteId =
                            p.PacienteId,

                        NombrePaciente =
                            p.NombrePaciente,

                        UltimoPago =
                            p.UltimoPago,

                        ProximoVencimiento =
                            p.ProximoVencimiento,

                        DiasParaVencimiento =
                            p.ProximoVencimiento
                                .DayNumber -
                            hoy.DayNumber
                    }
                )
                .ToList();


        // ======================================
        // PACIENTES RECIENTES
        // ======================================

        var pacientesRecientes =
            await pacientesQuery
                .OrderByDescending(p =>
                    p.FechaCreacion
                )
                .Take(5)
                .Select(p =>
                    new PacienteRecienteDashboardDto
                    {
                        Id =
                            p.Id,

                        NombreCompleto =
                            p.Nombre +
                            " " +
                            p.Apellido,

                        Email =
                            p.Email
                            ?? string.Empty,

                        Activo =
                            p.Activo,

                        CuentaActivada =
                            p.PasswordHash != null,

                        FechaCreacion =
                            p.FechaCreacion
                    }
                )
                .ToListAsync();


        // ======================================
        // RESPUESTA
        // ======================================

        return new DashboardNutricionistaDto
        {
            // ------------------------------
            // PACIENTES
            // ------------------------------

            TotalPacientes =
                totalPacientes,

            PacientesActivos =
                pacientesActivos,

            PacientesInactivos =
                pacientesInactivos,

            PacientesPendientesActivacion =
                pendientesActivacion,


            // ------------------------------
            // DIETAS
            // ------------------------------

            DietasBorrador =
                dietasBorrador,

            DietasActivas =
                dietasActivas,

            DietasArchivadas =
                dietasArchivadas,


            // ------------------------------
            // SEGUIMIENTOS
            // ------------------------------

            SeguimientosPendientesRevision =
                cantidadSeguimientosPendientes,

            SeguimientosPendientes =
                seguimientosPendientes,


            // ------------------------------
            // COBROS
            // ------------------------------

            CobrosVencidos =
                cantidadCobrosVencidos,

            CobrosProximosAVencer =
                cantidadCobrosProximos,

            CobrosVencidosDetalle =
                cobrosVencidos,

            CobrosProximosAVencerDetalle =
                cobrosProximos,


            // ------------------------------
            // ACTIVIDAD
            // ------------------------------

            PacientesRecientes =
                pacientesRecientes
        };
    }


    // ==========================================
    // FECHA LOCAL
    // ==========================================

    private static DateOnly
        ObtenerFechaActualArgentina()
    {
        try
        {
            var zonaHoraria =
                TimeZoneInfo
                    .FindSystemTimeZoneById(
                        "America/Argentina/Buenos_Aires"
                    );


            var fechaLocal =
                TimeZoneInfo
                    .ConvertTimeFromUtc(
                        DateTime.UtcNow,
                        zonaHoraria
                    );


            return DateOnly
                .FromDateTime(
                    fechaLocal
                );
        }
        catch (
            TimeZoneNotFoundException)
        {
            /*
             * Fallback para evitar romper
             * el dashboard si el sistema no
             * reconoce el identificador IANA.
             */

            return DateOnly
                .FromDateTime(
                    DateTime.UtcNow
                        .AddHours(-3)
                );
        }
        catch (
            InvalidTimeZoneException)
        {
            return DateOnly
                .FromDateTime(
                    DateTime.UtcNow
                        .AddHours(-3)
                );
        }
    }
}