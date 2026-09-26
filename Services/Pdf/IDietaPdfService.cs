using NutriApi.DTOs.Pdf;

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
