using NutriApi.DTOs.Pagos;

namespace NutriApi.Services.Pagos;

public interface IPagoPacienteService
{
    Task<ResultadoPago<PagoPacienteDto>>
        CrearAsync(
            int nutricionistaId,
            int pacienteId,
            CrearPagoPacienteDto dto
        );


    Task<ResultadoPago<List<PagoPacienteDto>>>
        ObtenerTodosAsync(
            int nutricionistaId,
            int pacienteId
        );


    Task<ResultadoPago<EstadoPagoPacienteDto>>
        ObtenerEstadoAsync(
            int nutricionistaId,
            int pacienteId
        );


    Task<ResultadoPago<PagoPacienteDto>>
        EditarAsync(
            int nutricionistaId,
            int pacienteId,
            int pagoId,
            EditarPagoPacienteDto dto
        );
}