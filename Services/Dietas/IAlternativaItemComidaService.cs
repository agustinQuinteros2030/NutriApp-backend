using NutriApi.DTOs.Dietas;

namespace NutriApi.Services.Dietas;

public interface IAlternativaItemComidaService
{
    Task<ResultadoDieta<AlternativaItemComidaDto>>
        AgregarAsync(
            int nutricionistaId,
            int pacienteId,
            int dietaId,
            int comidaId,
            int seccionId,
            int opcionId,
            int itemId,
            AgregarAlternativaItemComidaDto dto
        );

    Task<ResultadoDieta<List<AlternativaItemComidaDto>>>
        ObtenerTodasAsync(
            int nutricionistaId,
            int pacienteId,
            int dietaId,
            int comidaId,
            int seccionId,
            int opcionId,
            int itemId,
            bool incluirInactivas
        );

    Task<ResultadoDieta<bool>>
        CambiarEstadoAsync(
            int nutricionistaId,
            int pacienteId,
            int dietaId,
            int comidaId,
            int seccionId,
            int opcionId,
            int itemId,
            int alternativaId,
            bool activa
        );
}