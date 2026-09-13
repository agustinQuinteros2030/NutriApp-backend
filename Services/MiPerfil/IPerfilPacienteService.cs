using NutriApi.DTOs.PerfilPaciente;

namespace NutriApi.Services.MiPerfil;

public interface IPerfilPacienteService
{
    Task<MiPerfilPacienteDto?> ObtenerMiPerfilAsync(
        int pacienteId
    );
}