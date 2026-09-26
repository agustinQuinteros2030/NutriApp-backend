using NutriApi.DTOs.Dietas;

namespace NutriApi.Services.Dietas;

public interface IOpcionComidaService
{
    // OPCIONES

    Task<ResultadoDieta<OpcionSeccionComidaDto>> CrearOpcionAsync(
        int nutricionistaId,
        int pacienteId,
        int dietaId,
        int comidaId,
        int seccionId,
        CrearOpcionSeccionComidaDto dto
    );

    Task<ResultadoDieta<List<OpcionSeccionComidaDto>>> ObtenerOpcionesAsync(
        int nutricionistaId,
        int pacienteId,
        int dietaId,
        int comidaId,
        int seccionId
    );

    Task<ResultadoDieta<OpcionSeccionComidaDto>> ObtenerOpcionAsync(
        int nutricionistaId,
        int pacienteId,
        int dietaId,
        int comidaId,
        int seccionId,
        int opcionId
    );

    Task<ResultadoDieta<OpcionSeccionComidaDto>> EditarOpcionAsync(
        int nutricionistaId,
        int pacienteId,
        int dietaId,
        int comidaId,
        int seccionId,
        int opcionId,
        EditarOpcionSeccionComidaDto dto
    );

    // ITEMS

    Task<ResultadoDieta<ItemOpcionComidaDto>> CrearItemAsync(
        int nutricionistaId,
        int pacienteId,
        int dietaId,
        int comidaId,
        int seccionId,
        int opcionId,
        CrearItemOpcionComidaDto dto
    );

    Task<ResultadoDieta<ItemOpcionComidaDto>> EditarItemAsync(
        int nutricionistaId,
        int pacienteId,
        int dietaId,
        int comidaId,
        int seccionId,
        int opcionId,
        int itemId,
        EditarItemOpcionComidaDto dto
    );

    Task<ResultadoDieta<bool>> EliminarOpcionAsync(
        int nutricionistaId,
        int pacienteId,
        int dietaId,
        int comidaId,
        int seccionId,
        int opcionId
    );

    Task<ResultadoDieta<bool>> EliminarItemAsync(
        int nutricionistaId,
        int pacienteId,
        int dietaId,
        int comidaId,
        int seccionId,
        int opcionId,
        int itemId
    );
}
