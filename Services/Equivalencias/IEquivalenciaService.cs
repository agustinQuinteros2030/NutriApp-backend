using NutriApi.DTOs.Equivalencias;

namespace NutriApi.Services.Equivalencias;

public interface IEquivalenciaService
{
    Task<ResultadoEquivalencia<GrupoEquivalenciaDetalleDto>>
        CrearGrupoAsync(
            int nutricionistaId,
            CrearGrupoEquivalenciaDto dto
        );

    Task<List<GrupoEquivalenciaListadoDto>>
        ObtenerGruposAsync(
            int nutricionistaId,
            bool incluirInactivos
        );

    Task<ResultadoEquivalencia<GrupoEquivalenciaDetalleDto>>
        ObtenerGrupoPorIdAsync(
            int nutricionistaId,
            int grupoId
        );

    Task<ResultadoEquivalencia<GrupoEquivalenciaDetalleDto>>
        EditarGrupoAsync(
            int nutricionistaId,
            int grupoId,
            EditarGrupoEquivalenciaDto dto
        );

    Task<ResultadoEquivalencia<bool>>
        CambiarEstadoGrupoAsync(
            int nutricionistaId,
            int grupoId,
            bool activo
        );

    Task<ResultadoEquivalencia<EquivalenciaAlimentoDto>>
        AgregarAlimentoAsync(
            int nutricionistaId,
            int grupoId,
            AgregarEquivalenciaAlimentoDto dto
        );

   

    Task<ResultadoEquivalencia<bool>>
        CambiarEstadoEquivalenciaAsync(
            int nutricionistaId,
            int grupoId,
            int equivalenciaId,
            bool activa
        );

    Task<ResultadoEquivalencia<List<ConversionEquivalenciaDto>>>
        ConvertirAsync(
            int nutricionistaId,
            int grupoId,
            int alimentoOrigenId,
            decimal cantidad
        );
}