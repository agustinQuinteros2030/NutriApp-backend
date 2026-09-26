using NutriApi.DTOs.Turnos;

namespace NutriApi.Services.Turnos;

public interface ITurnoService
{
    // ==========================================
    // NUTRICIONISTA - CREAR
    // ==========================================

    Task<ResultadoTurno<TurnoDto>> CrearAsync(
        int nutricionistaId,
        int pacienteId,
        CrearTurnoDto dto
    );

    // ==========================================
    // NUTRICIONISTA - EDITAR / REPROGRAMAR
    // ==========================================

    Task<ResultadoTurno<TurnoDto>> EditarAsync(
        int nutricionistaId,
        int pacienteId,
        int turnoId,
        EditarTurnoDto dto
    );

    // ==========================================
    // NUTRICIONISTA - CANCELAR
    // ==========================================

    Task<ResultadoTurno<TurnoDto>> CancelarAsync(int nutricionistaId, int pacienteId, int turnoId);

    // ==========================================
    // NUTRICIONISTA - MARCAR REALIZADO
    // ==========================================

    Task<ResultadoTurno<TurnoDto>> MarcarRealizadoAsync(
        int nutricionistaId,
        int pacienteId,
        int turnoId
    );

    // ==========================================
    // NUTRICIONISTA - PRÓXIMOS
    // ==========================================

    Task<ResultadoTurno<List<TurnoDto>>> ObtenerProximosNutricionistaAsync(int nutricionistaId);

    // ==========================================
    // NUTRICIONISTA - TURNOS DE UN PACIENTE
    // ==========================================

    Task<ResultadoTurno<List<TurnoDto>>> ObtenerPacienteAsync(int nutricionistaId, int pacienteId);

    // ==========================================
    // NUTRICIONISTA - DETALLE
    // ==========================================

    Task<ResultadoTurno<TurnoDto>> ObtenerDetalleAsync(
        int nutricionistaId,
        int pacienteId,
        int turnoId
    );

    // ==========================================
    // PACIENTE - MIS TURNOS
    // ==========================================

    Task<ResultadoTurno<List<TurnoDto>>> ObtenerPropiosAsync(int pacienteId);

    // ==========================================
    // PACIENTE - PRÓXIMO TURNO
    // ==========================================

    Task<ResultadoTurno<ProximoTurnoDto?>> ObtenerProximoPacienteAsync(int pacienteId);
}
