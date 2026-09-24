using NutriApi.DTOs.Dietas;

namespace NutriApi.Services.Dietas;

public interface IComidaService
{
    // COMIDAS

    Task<ResultadoDieta<ComidaDetalleDto>>
        CrearComidaAsync(
            int nutricionistaId,
            int pacienteId,
            int dietaId,
            CrearComidaDto dto
        );

    Task<ResultadoDieta<List<ComidaListadoDto>>>
        ObtenerComidasAsync(
            int nutricionistaId,
            int pacienteId,
            int dietaId
        );

    Task<ResultadoDieta<ComidaDetalleDto>>
        ObtenerComidaAsync(
            int nutricionistaId,
            int pacienteId,
            int dietaId,
            int comidaId
        );

    Task<ResultadoDieta<ComidaDetalleDto>>
        EditarComidaAsync(
            int nutricionistaId,
            int pacienteId,
            int dietaId,
            int comidaId,
            EditarComidaDto dto
        );

    Task<ResultadoDieta<bool>>
        EliminarComidaAsync(
            int nutricionistaId,
            int pacienteId,
            int dietaId,
            int comidaId
        );


    // SECCIONES

    Task<ResultadoDieta<SeccionComidaDto>>
        CrearSeccionAsync(
            int nutricionistaId,
            int pacienteId,
            int dietaId,
            int comidaId,
            CrearSeccionComidaDto dto
        );

    Task<ResultadoDieta<List<SeccionComidaDto>>>
        ObtenerSeccionesAsync(
            int nutricionistaId,
            int pacienteId,
            int dietaId,
            int comidaId
        );

    Task<ResultadoDieta<SeccionComidaDto>>
        EditarSeccionAsync(
            int nutricionistaId,
            int pacienteId,
            int dietaId,
            int comidaId,
            int seccionId,
            EditarSeccionComidaDto dto
        );

    Task<ResultadoDieta<bool>>
        EliminarSeccionAsync(
            int nutricionistaId,
            int pacienteId,
            int dietaId,
            int comidaId,
            int seccionId
        );
}