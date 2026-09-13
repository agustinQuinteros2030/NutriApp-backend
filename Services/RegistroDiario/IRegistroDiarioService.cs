using NutriApi.DTOs.RegistroDiario;

namespace NutriApi.Services.RegistroDiario;

public interface IRegistroDiarioService
{
    Task<
        ResultadoRegistroDiario<
            RegistroDiarioPacienteDto>>
        GuardarPropioAsync(
            int pacienteId,
            GuardarRegistroDiarioDto dto
        );


    Task<
        ResultadoRegistroDiario<
            List<RegistroDiarioPacienteDto>>>
        ObtenerPropiosAsync(
            int pacienteId
        );


    Task<
        ResultadoRegistroDiario<
            RegistroDiarioPacienteDto>>
        ObtenerPropioPorFechaAsync(
            int pacienteId,
            DateOnly fecha
        );


    Task<
        ResultadoRegistroDiario<
            List<RegistroDiarioPacienteDto>>>
        ObtenerPacienteAsync(
            int nutricionistaId,
            int pacienteId
        );
}