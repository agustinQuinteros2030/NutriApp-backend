using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

using NutriApi.DTOs.RecuperacionPassword;
using NutriApi.Services.RecuperacionPassword;

using System.Security.Claims;

namespace NutriApi.Controllers;

[ApiController]
[Route("api/auth")]
public class RecuperacionPasswordController
    : ControllerBase
{
    private readonly IRecuperacionPasswordService
        _service;


    public RecuperacionPasswordController(
        IRecuperacionPasswordService service)
    {
        _service =
            service;
    }


    // ==========================================
    // SOLICITAR RECUPERACIÓN
    // PÚBLICO
    // ==========================================

    /*
     * Este endpoint se conserva para no romper
     * clientes/frontend existentes.
     *
     * Ya no envía emails.
     *
     * Mantiene respuesta genérica para evitar
     * enumeración de usuarios.
     */

    [AllowAnonymous]
    [EnableRateLimiting("AuthSensitive")]
    [HttpPost("solicitar-recuperacion")]
    public async Task<IActionResult>
        SolicitarRecuperacion(
            [FromBody]
            SolicitarRecuperacionPasswordDto dto)
    {
        var resultado =
            await _service
                .SolicitarAsync(
                    dto.Email
                );


        if (!resultado.Exitoso)
        {
            return resultado.TipoError switch
            {
                TipoErrorRecuperacionPassword
                    .Validacion =>
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
                                "Ocurrió un error interno."
                        }
                    )
            };
        }


        return Ok(
            new MensajeRecuperacionPasswordDto
            {
                Mensaje =
                    "La recuperación de contraseña debe solicitarse a tu nutricionista."
            }
        );
    }


    // ==========================================
    // GENERAR RECUPERACIÓN PARA PACIENTE
    // NUTRICIONISTA
    // ==========================================

    [Authorize(
        Roles =
            Roles.Nutricionista
    )]
    [HttpPost(
        "~/api/pacientes/{pacienteId:int}/recuperacion-password"
    )]
    public async Task<IActionResult>
        GenerarRecuperacionPaciente(
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
                .GenerarParaPacienteAsync(
                    nutricionistaId.Value,
                    pacienteId
                );


        if (resultado.Exitoso)
        {
            return Ok(
                resultado.Datos
            );
        }


        return resultado.TipoError switch
        {
            TipoErrorRecuperacionPassword
                .NoEncontrado =>
                NotFound(
                    new
                    {
                        error =
                            resultado.Error
                    }
                ),

            TipoErrorRecuperacionPassword
                .Validacion =>
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


    // ==========================================
    // RESTABLECER CONTRASEÑA
    // PÚBLICO
    // ==========================================

    [AllowAnonymous]
    [EnableRateLimiting("AuthSensitive")]
    [HttpPost("restablecer-password")]
    public async Task<IActionResult>
        RestablecerPassword(
            [FromBody]
            RestablecerPasswordDto dto)
    {
        var resultado =
            await _service
                .RestablecerAsync(
                    dto
                );


        if (resultado.Exitoso)
        {
            return Ok(
                new MensajeRecuperacionPasswordDto
                {
                    Mensaje =
                        "La contraseña fue actualizada correctamente."
                }
            );
        }


        return resultado.TipoError switch
        {
            TipoErrorRecuperacionPassword
                .TokenInvalido =>
                BadRequest(
                    new
                    {
                        error =
                            resultado.Error
                    }
                ),

            TipoErrorRecuperacionPassword
                .Validacion =>
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
                            "Ocurrió un error interno."
                    }
                )
        };
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
}