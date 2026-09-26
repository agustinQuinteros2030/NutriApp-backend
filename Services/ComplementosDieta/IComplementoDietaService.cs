using NutriApi.DTOs.Dietas;

namespace NutriApi.Services.Dietas;

public interface IComplementoDietaService
{
    Task<ResultadoDieta<HidratacionDietaDto?>> ObtenerHidratacionAsync(
        int nutricionistaId,
        int pacienteId,
        int dietaId
    );

    Task<ResultadoDieta<HidratacionDietaDto>> GuardarHidratacionAsync(
        int nutricionistaId,
        int pacienteId,
        int dietaId,
        GuardarHidratacionDietaDto dto
    );

    Task<ResultadoDieta<SuplementacionDietaDto?>> ObtenerSuplementacionAsync(
        int nutricionistaId,
        int pacienteId,
        int dietaId
    );

    Task<ResultadoDieta<SuplementacionDietaDto>> GuardarSuplementacionAsync(
        int nutricionistaId,
        int pacienteId,
        int dietaId,
        GuardarSuplementacionDietaDto dto
    );

    Task<ResultadoDieta<ItemSuplementacionDto>> AgregarItemSuplementacionAsync(
        int nutricionistaId,
        int pacienteId,
        int dietaId,
        CrearItemSuplementacionDto dto
    );

    Task<ResultadoDieta<ItemSuplementacionDto>> EditarItemSuplementacionAsync(
        int nutricionistaId,
        int pacienteId,
        int dietaId,
        int itemId,
        EditarItemSuplementacionDto dto
    );

    Task<ResultadoDieta<bool>> EliminarItemSuplementacionAsync(
        int nutricionistaId,
        int pacienteId,
        int dietaId,
        int itemId
    );
}
