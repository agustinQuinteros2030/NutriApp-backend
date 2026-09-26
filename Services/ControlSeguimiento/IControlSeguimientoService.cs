using NutriApi.DTOs.ControlSeguimiento;

namespace NutriApi.Services.ControlSeguimiento;

public interface IControlSeguimientoService
{
    Task<ResultadoControlSeguimiento<ControlSeguimientoDto>> ConfigurarAsync(
        int nutricionistaId,
        int pacienteId,
        ConfigurarControlSeguimientoDto dto
    );

    Task<ResultadoControlSeguimiento<ControlSeguimientoDto>> ObtenerPacienteAsync(
        int nutricionistaId,
        int pacienteId
    );

    Task<ResultadoControlSeguimiento<ControlSeguimientoDto>> MarcarRealizadoAsync(
        int nutricionistaId,
        int pacienteId
    );

    Task<ResultadoControlSeguimiento<ControlSeguimientoDto>> ReprogramarAsync(
        int nutricionistaId,
        int pacienteId,
        ReprogramarControlSeguimientoDto dto
    );

    Task<ResultadoControlSeguimiento<ControlSeguimientoDto>> ActivarAsync(
        int nutricionistaId,
        int pacienteId
    );

    Task<ResultadoControlSeguimiento<ControlSeguimientoDto>> DesactivarAsync(
        int nutricionistaId,
        int pacienteId
    );

    Task<ResultadoControlSeguimiento<List<ControlSeguimientoDto>>> ObtenerTodosAsync(
        int nutricionistaId,
        bool incluirInactivos = false
    );

    Task<ResultadoControlSeguimiento<ResumenControlSeguimientoDto>> ObtenerResumenAsync(
        int nutricionistaId
    );
}
