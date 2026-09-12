using NutriApi.DTOs.ActivacionCuenta;

namespace NutriApi.Services.ActivacionCuenta;

public interface IActivacionCuentaService
{
    Task<
        ResultadoActivacionCuenta<
            EstadoActivacionCuentaDto>>
        ObtenerEstadoAsync(
            int nutricionistaId,
            int pacienteId
        );


    Task<
        ResultadoActivacionCuenta<
            ActivacionCuentaGeneradaDto>>
        GenerarActivacionAsync(
            int nutricionistaId,
            int pacienteId
        );


    Task<
        ResultadoActivacionCuenta<bool>>
        ActivarCuentaAsync(
            ActivarCuentaDto dto
        );
}