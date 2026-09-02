using NutriApi.DTOs.Alimentos;

namespace NutriApi.Services.Alimentos;

public interface ICategoriaAlimentoService
{
    Task<ResultadoAlimento<CategoriaAlimentoDto>>
        CrearAsync(
            int nutricionistaId,
            CrearCategoriaAlimentoDto dto
        );

    Task<List<CategoriaAlimentoDto>>
        ObtenerTodasAsync(
            int nutricionistaId,
            bool incluirInactivas
        );

    Task<ResultadoAlimento<CategoriaAlimentoDto>>
        EditarAsync(
            int nutricionistaId,
            int categoriaId,
            EditarCategoriaAlimentoDto dto
        );

    Task<ResultadoAlimento<bool>>
        CambiarEstadoAsync(
            int nutricionistaId,
            int categoriaId,
            bool activa
        );
}