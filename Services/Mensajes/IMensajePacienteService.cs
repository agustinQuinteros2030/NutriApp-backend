using NutriApi.DTOs.Mensajes;

namespace NutriApi.Services.Mensajes;

public interface IMensajePacienteService
{
    Task<ResultadoMensajePaciente<MensajePacienteDto>> CrearAsync(
        int nutricionistaId,
        int pacienteId,
        CrearMensajePacienteDto dto
    );

    Task<ResultadoMensajePaciente<List<MensajePacienteDto>>> ObtenerPacienteAsync(
        int nutricionistaId,
        int pacienteId
    );

    Task<ResultadoMensajePaciente<List<MensajePacienteDto>>> ObtenerPropiosAsync(int pacienteId);

    Task<ResultadoMensajePaciente<MensajePacienteDto>> ObtenerDetallePacienteAsync(
        int pacienteId,
        int mensajeId
    );

    Task<ResultadoMensajePaciente<MensajePacienteDto>> EditarAsync(
        int nutricionistaId,
        int pacienteId,
        int mensajeId,
        EditarMensajePacienteDto dto
    );

    Task<ResultadoMensajePaciente<bool>> EliminarAsync(
        int nutricionistaId,
        int pacienteId,
        int mensajeId
    );

    Task<ResultadoMensajePaciente<MensajePacienteDto>> MarcarComoLeidoAsync(
        int pacienteId,
        int mensajeId
    );
}
