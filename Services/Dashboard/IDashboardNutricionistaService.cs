using NutriApi.DTOs.Dashboard;

namespace NutriApi.Services.Dashboard;

public interface IDashboardNutricionistaService
{
    Task<DashboardNutricionistaDto>
        ObtenerAsync(
            int nutricionistaId
        );
}