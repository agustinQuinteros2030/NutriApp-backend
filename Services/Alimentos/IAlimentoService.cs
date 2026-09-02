using NutriApi.DTOs.Alimentos;

namespace NutriApi.Services.Alimentos;

public interface IAlimentoService
{
    Task<ResultadoAlimento<AlimentoDetalleDto>>
        CrearAsync(
            int nutricionistaId,
            CrearAlimentoDto dto
        );

    Task<List<AlimentoListadoDto>>
        ObtenerTodosAsync(
            int nutricionistaId,
            string? buscar,
            int? categoriaId,
            bool incluirInactivos
        );

    Task<ResultadoAlimento<AlimentoDetalleDto>>
        ObtenerPorIdAsync(
            int nutricionistaId,
            int alimentoId
        );

    Task<ResultadoAlimento<AlimentoDetalleDto>>
        EditarAsync(
            int nutricionistaId,
            int alimentoId,
            EditarAlimentoDto dto
        );

    Task<ResultadoAlimento<bool>>
        CambiarEstadoAsync(
            int nutricionistaId,
            int alimentoId,
            bool activo
        );
}