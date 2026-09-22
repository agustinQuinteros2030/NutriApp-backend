using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

using NutriApi.DTOs.ActivacionCuenta;
using NutriApi.Services.ActivacionCuenta;

using System.Security.Claims;

namespace NutriApi.Controllers;

[ApiController]
[Route("api")]
public class ActivacionCuentaController
    : ControllerBase
{
    private readonly IActivacionCuentaService
        _service;


    public ActivacionCuentaController(
        IActivacionCuentaService service)
    {
        _service =
            service;
    }


    // ==========================================
    // ESTADO DE ACTIVACIÓN
    // NUTRICIONISTA
    // ==========================================

    [Authorize(
        Roles =
            Roles.Nutricionista
    )]
    [HttpGet(
        "pacientes/{pacienteId:int}/activacion"
    )]
    public async Task<IActionResult>
        ObtenerEstado(
            int pacienteId)
    {
        var nutricionistaId =
            ObtenerUsuarioId();


        if (!nutricionistaId.HasValue)
        {
            return Unauthorized();
        }


        var resultado =
            await _service
                .ObtenerEstadoAsync(
                    nutricionistaId.Value,
                    pacienteId
                );


        return Responder(
            resultado
        );
    }


    // ==========================================
    // GENERAR ENLACE DE ACTIVACIÓN
    // NUTRICIONISTA
    // ==========================================

    [Authorize(
        Roles =
            Roles.Nutricionista
    )]
    [HttpPost(
        "pacientes/{pacienteId:int}/activacion"
    )]
    public async Task<IActionResult>
        GenerarActivacion(
            int pacienteId)
    {
        var nutricionistaId =
            ObtenerUsuarioId();


        if (!nutricionistaId.HasValue)
        {
            return Unauthorized();
        }


        var resultado =
            await _service
                .GenerarActivacionAsync(
                    nutricionistaId.Value,
                    pacienteId
                );


        return Responder(
            resultado
        );
    }


    // ==========================================
    // ACTIVAR CUENTA
    // PÚBLICO
    // ==========================================

    [AllowAnonymous]
    [EnableRateLimiting("AuthSensitive")]
    [HttpPost(
        "auth/activar-cuenta"
    )]
    public async Task<IActionResult>
        ActivarCuenta(
            [FromBody]
            ActivarCuentaDto dto)
    {
        var resultado =
            await _service
                .ActivarCuentaAsync(
                    dto
                );


        return Responder(
            resultado
        );
    }


    // ==========================================
    // ID JWT
    // ==========================================

    private int?
        ObtenerUsuarioId()
    {
        var claim =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier
            );


        if (!int.TryParse(
            claim,
            out var id))
        {
            return null;
        }


        return id;
    }


    // ==========================================
    // RESPUESTA
    // ==========================================

    private IActionResult
        Responder<T>(
            ResultadoActivacionCuenta<T>
                resultado)
    {
        if (resultado.Exitoso)
        {
            return Ok(
                resultado.Datos
            );
        }


        return resultado.TipoError switch
        {
            TipoErrorActivacionCuenta
                .NoEncontrado =>
                NotFound(
                    new
                    {
                        error =
                            resultado.Error
                    }
                ),

            TipoErrorActivacionCuenta
                .Validacion =>
                BadRequest(
                    new
                    {
                        error =
                            resultado.Error
                    }
                ),

            TipoErrorActivacionCuenta
                .TokenInvalido =>
                BadRequest(
                    new
                    {
                        error =
                            resultado.Error
                    }
                ),

            _ =>
                StatusCode(
                    StatusCodes
                        .Status500InternalServerError,
                    new
                    {
                        error =
                            resultado.Error
                            ??
                            "Ocurrió un error interno."
                    }
                )
        };
    }
}