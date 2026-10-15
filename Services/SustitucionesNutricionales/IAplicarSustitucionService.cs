using global::NutriApi.DTOs.Dietas;
using global::NutriApi.Services.Dietas;

namespace NutriApi.Services.SustitucionesNutricionales;

public interface IAplicarSustitucionService
{
    Task<ResultadoDieta<SustitucionAplicadaDto>> AplicarAsync(
        int nutricionistaId,
        int pacienteId,
        int dietaId,
        int comidaId,
        int itemId,
        AplicarSustitucionDto dto,
        CancellationToken cancellationToken = default
    );
}
