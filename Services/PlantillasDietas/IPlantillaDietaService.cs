using NutriApi.DTOs.Dietas;
using NutriApi.DTOs.PlantillasDietas;
using NutriApi.Services.Dietas;

namespace NutriApi.Services.PlantillasDietas;

public interface IPlantillaDietaService
{
    Task<ResultadoDieta<PlantillaDietaDetalleDto>> CrearDesdeDietaAsync(
        int nutricionistaId,
        int pacienteId,
        int dietaId,
        CrearPlantillaDietaDto dto
    );

    Task<ResultadoDieta<List<PlantillaDietaListadoDto>>> ObtenerTodasAsync(int nutricionistaId);

    Task<ResultadoDieta<PlantillaDietaDetalleDto>> ObtenerPorIdAsync(
        int nutricionistaId,
        int plantillaId
    );

    Task<ResultadoDieta<DietaDetalleDto>> AplicarAPacienteAsync(
        int nutricionistaId,
        int plantillaId,
        int pacienteId
    );

    Task<ResultadoDieta<bool>> EliminarAsync(int nutricionistaId, int plantillaId);
}
