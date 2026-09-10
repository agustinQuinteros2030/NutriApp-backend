using NutriApi.DTOs.Notas;

namespace NutriApi.Services.Notas;

public interface INotaPacienteService
{
    Task<ResultadoNotaPaciente<NotaPacienteDto>>
        CrearAsync(
            int nutricionistaId,
            int pacienteId,
            CrearNotaPacienteDto dto
        );


    Task<ResultadoNotaPaciente<List<NotaPacienteDto>>>
        ObtenerTodasAsync(
            int nutricionistaId,
            int pacienteId
        );


    Task<ResultadoNotaPaciente<NotaPacienteDto>>
        ObtenerPorIdAsync(
            int nutricionistaId,
            int pacienteId,
            int notaId
        );


    Task<ResultadoNotaPaciente<NotaPacienteDto>>
        EditarAsync(
            int nutricionistaId,
            int pacienteId,
            int notaId,
            EditarNotaPacienteDto dto
        );
}