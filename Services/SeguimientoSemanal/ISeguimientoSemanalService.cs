using NutriApi.DTOs.SeguimientoSemanal;

namespace NutriApi.Services.SeguimientoSemanal;

public interface ISeguimientoSemanalService
{
    // ==========================================
    // PACIENTE
    // ==========================================

    Task<
        ResultadoSeguimientoSemanal<
            EstadoSeguimientoSemanalDto>>
        ObtenerEstadoActualAsync(
            int pacienteId
        );


    Task<
        ResultadoSeguimientoSemanal<
            SeguimientoSemanalDto>>
        CrearActualAsync(
            int pacienteId,
            CrearSeguimientoSemanalDto dto
        );


    Task<
        ResultadoSeguimientoSemanal<
            List<SeguimientoSemanalDto>>>
        ObtenerPropiosAsync(
            int pacienteId
        );


    // ==========================================
    // NUTRICIONISTA
    // ==========================================

    Task<
        ResultadoSeguimientoSemanal<
            List<SeguimientoSemanalDto>>>
        ObtenerPacienteAsync(
            int nutricionistaId,
            int pacienteId
        );


    Task<
        ResultadoSeguimientoSemanal<
            SeguimientoSemanalDto>>
        ObtenerDetallePacienteAsync(
            int nutricionistaId,
            int pacienteId,
            int seguimientoId
        );


    Task<
        ResultadoSeguimientoSemanal<
            SeguimientoSemanalDto>>
        RevisarAsync(
            int nutricionistaId,
            int pacienteId,
            int seguimientoId,
            RevisarSeguimientoSemanalDto dto
        );
}