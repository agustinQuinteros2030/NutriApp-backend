using NutriApi.DTOs.PlanPaciente;

namespace NutriApi.Services.PlanPaciente;

public interface IPlanPacienteService
{
    Task<MiPlanDto?> ObtenerMiPlanAsync(
        int pacienteId
    );

    Task<MiPlanDto?>
    ObtenerPlanPorDietaAsync(
        int nutricionistaId,
        int pacienteId,
        int dietaId
    );
}