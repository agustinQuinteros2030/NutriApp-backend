using NutriApi.DTOs.Dietas;
using NutriApi.DTOs.Pdf;
using NutriApi.Services.Dietas;

namespace NutriApi.Services.Pdf;

public interface IDietaPdfService
{
    Task<ArchivoPdfDieta?> GenerarParaNutricionistaAsync(
        int nutricionistaId,
        int pacienteId,
        int dietaId
    );

    Task<ArchivoPdfDieta?> GenerarParaPacienteAsync(int pacienteId);

   
}
