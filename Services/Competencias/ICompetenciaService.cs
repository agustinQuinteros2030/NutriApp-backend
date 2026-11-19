using NutriApi.DTOs.Competencias;

namespace NutriApi.Services.Competencias;

public interface ICompetenciaService
{
    Task<ResultadoCompetencia<CompetenciaDto>> CrearAsync(
        int nutricionistaId,
        int pacienteId,
        CrearCompetenciaDto dto
    );

    Task<ResultadoCompetencia<CompetenciaDto>> EditarAsync(
        int nutricionistaId,
        int pacienteId,
        int competenciaId,
        EditarCompetenciaDto dto
    );

    Task<ResultadoCompetencia<CompetenciaDto>> CancelarAsync(
        int nutricionistaId,
        int pacienteId,
        int competenciaId
    );

    Task<ResultadoCompetencia<CompetenciaDto>> MarcarRealizadaAsync(
        int nutricionistaId,
        int pacienteId,
        int competenciaId
    );

    Task<ResultadoCompetencia<List<CompetenciaDto>>> ObtenerPacienteAsync(
        int nutricionistaId,
        int pacienteId
    );

    Task<ResultadoCompetencia<CompetenciaDto>> ObtenerDetalleAsync(
        int nutricionistaId,
        int pacienteId,
        int competenciaId
    );

    Task<ResultadoCompetencia<List<CompetenciaPacienteDto>>> ObtenerPropiosAsync(int pacienteId);
}
