using NutriApi.DTOs.Dietas;
using NutriApi.Services.Dietas;

namespace NutriApi.Services.SustitucionesNutricionales;

public interface ISustitucionNutricionalService
{
    Task<ResultadoDieta<SimulacionSustitucionesDto>> SimularAsync(
        int nutricionistaId,
        int pacienteId,
        int dietaId,
        int comidaId,
        CancellationToken cancellationToken = default
    );
}
