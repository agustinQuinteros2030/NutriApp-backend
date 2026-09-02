using NutriApi.DTOs.Pacientes;

namespace NutriApi.Services.Pacientes;

public interface IPacienteService
{
    Task<ResultadoPaciente<PacienteDetalleDto>>
        CrearAsync(
            int nutricionistaId,
            CrearPacienteDto dto
        );

    Task<List<PacienteListadoDto>>
        ObtenerTodosAsync(
            int nutricionistaId,
            string? buscar
        );

    Task<ResultadoPaciente<PacienteDetalleDto>>
        ObtenerPorIdAsync(
            int nutricionistaId,
            int pacienteId
        );

    Task<ResultadoPaciente<PacienteDetalleDto>>
        EditarAsync(
            int nutricionistaId,
            int pacienteId,
            EditarPacienteDto dto
        );

    Task<ResultadoPaciente<bool>>
        CambiarEstadoAsync(
            int nutricionistaId,
            int pacienteId,
            bool activo
        );
}