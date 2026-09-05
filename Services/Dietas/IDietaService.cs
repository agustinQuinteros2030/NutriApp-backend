using NutriApi.DTOs.Dietas;

namespace NutriApi.Services.Dietas;

public interface IDietaService
{
    Task<ResultadoDieta<DietaDetalleDto>>
        CrearAsync(
            int nutricionistaId,
            int pacienteId,
            CrearDietaDto dto
        );

    Task<ResultadoDieta<List<DietaListadoDto>>>
        ObtenerTodasAsync(
            int nutricionistaId,
            int pacienteId
        );

    Task<ResultadoDieta<DietaDetalleDto>>
        ObtenerPorIdAsync(
            int nutricionistaId,
            int pacienteId,
            int dietaId
        );

    Task<ResultadoDieta<DietaDetalleDto>>
        EditarAsync(
            int nutricionistaId,
            int pacienteId,
            int dietaId,
            EditarDietaDto dto
        );

    Task<ResultadoDieta<DietaDetalleDto>>
        ActivarAsync(
            int nutricionistaId,
            int pacienteId,
            int dietaId
        );

    Task<ResultadoDieta<DietaDetalleDto>>
        ArchivarAsync(
            int nutricionistaId,
            int pacienteId,
            int dietaId
        );
}