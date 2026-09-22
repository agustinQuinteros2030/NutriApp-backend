using NutriApi.DTOs.RecuperacionPassword;

namespace NutriApi.Services.RecuperacionPassword;

public interface IRecuperacionPasswordService
{
    Task<
        ResultadoRecuperacionPassword<
            SolicitudRecuperacionGenerada>>
        SolicitarAsync(
            string email
        );


    Task<
        ResultadoRecuperacionPassword<
            RecuperacionPasswordPacienteGeneradaDto>>
        GenerarParaPacienteAsync(
            int nutricionistaId,
            int pacienteId
        );


    Task<
        ResultadoRecuperacionPassword<bool>>
        RestablecerAsync(
            RestablecerPasswordDto dto
        );
}