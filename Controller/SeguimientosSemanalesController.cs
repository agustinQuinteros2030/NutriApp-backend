using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using NutriApi.DTOs.SeguimientoSemanal;
using NutriApi.Services.SeguimientoSemanal;

using System.Security.Claims;

namespace NutriApi.Controllers;

[ApiController]
[Route("api")]
public class SeguimientosSemanalesController
    : ControllerBase
{
    private readonly ISeguimientoSemanalService
        _service;


    public SeguimientosSemanalesController(
        ISeguimientoSemanalService service)
    {
        _service = service;
    }


    // ==========================================
    // PACIENTE
    // ESTADO DEL SEGUIMIENTO ACTUAL
    // ==========================================

    [Authorize(
        Roles =
            Roles.Paciente
    )]
    [HttpGet(
        "paciente/seguimiento-semanal/actual"
    )]
    public async Task<IActionResult>
        ObtenerEstadoActual()
    {
        var pacienteId =
            ObtenerUsuarioId();


        if (!pacienteId.HasValue)
        {
            return Unauthorized();
        }


        var resultado =
            await _service
                .ObtenerEstadoActualAsync(
                    pacienteId.Value
                );


        return Responder(
            resultado
        );
    }


    // ==========================================
    // PACIENTE
    // COMPLETAR SEGUIMIENTO DE LA SEMANA
    // ==========================================

    [Authorize(
        Roles =
            Roles.Paciente
    )]
    [HttpPost(
        "paciente/seguimientos-semanales"
    )]
    public async Task<IActionResult>
        CrearSeguimiento(
            [FromBody]
            CrearSeguimientoSemanalDto dto)
    {
        var pacienteId =
            ObtenerUsuarioId();


        if (!pacienteId.HasValue)
        {
            return Unauthorized();
        }


        var resultado =
            await _service
                .CrearActualAsync(
                    pacienteId.Value,
                    dto
                );


        return Responder(
            resultado
        );
    }


    // ==========================================
    // PACIENTE
    // HISTORIAL PROPIO
    // ==========================================

    [Authorize(
        Roles =
            Roles.Paciente
    )]
    [HttpGet(
        "paciente/seguimientos-semanales"
    )]
    public async Task<IActionResult>
        ObtenerPropios()
    {
        var pacienteId =
            ObtenerUsuarioId();


        if (!pacienteId.HasValue)
        {
            return Unauthorized();
        }


        var resultado =
            await _service
                .ObtenerPropiosAsync(
                    pacienteId.Value
                );


        return Responder(
            resultado
        );
    }


    // ==========================================
    // NUTRICIONISTA
    // HISTORIAL DE UN PACIENTE
    // ==========================================

    [Authorize(
        Roles =
            Roles.Nutricionista
    )]
    [HttpGet(
        "pacientes/{pacienteId:int}/seguimientos-semanales"
    )]
    public async Task<IActionResult>
        ObtenerSeguimientosPaciente(
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
                .ObtenerPacienteAsync(
                    nutricionistaId.Value,
                    pacienteId
                );


        return Responder(
            resultado
        );
    }


    // ==========================================
    // NUTRICIONISTA
    // DETALLE DE UN SEGUIMIENTO
    // ==========================================

    [Authorize(
        Roles =
            Roles.Nutricionista
    )]
    [HttpGet(
        "pacientes/{pacienteId:int}/seguimientos-semanales/{seguimientoId:int}"
    )]
    public async Task<IActionResult>
        ObtenerDetalleSeguimiento(
            int pacienteId,
            int seguimientoId)
    {
        var nutricionistaId =
            ObtenerUsuarioId();


        if (!nutricionistaId.HasValue)
        {
            return Unauthorized();
        }


        var resultado =
            await _service
                .ObtenerDetallePacienteAsync(
                    nutricionistaId.Value,
                    pacienteId,
                    seguimientoId
                );


        return Responder(
            resultado
        );
    }


    // ==========================================
    // NUTRICIONISTA
    // REVISAR SEGUIMIENTO
    // ==========================================

    [Authorize(
        Roles =
            Roles.Nutricionista
    )]
    [HttpPut(
        "pacientes/{pacienteId:int}/seguimientos-semanales/{seguimientoId:int}/revision"
    )]
    public async Task<IActionResult>
        RevisarSeguimiento(
            int pacienteId,
            int seguimientoId,
            [FromBody]
            RevisarSeguimientoSemanalDto dto)
    {
        var nutricionistaId =
            ObtenerUsuarioId();


        if (!nutricionistaId.HasValue)
        {
            return Unauthorized();
        }


        var resultado =
            await _service
                .RevisarAsync(
                    nutricionistaId.Value,
                    pacienteId,
                    seguimientoId,
                    dto
                );


        return Responder(
            resultado
        );
    }


    // ==========================================
    // USUARIO AUTENTICADO
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
            out var usuarioId))
        {
            return null;
        }


        return usuarioId;
    }


    // ==========================================
    // RESPUESTA GENÉRICA
    // ==========================================

    private IActionResult
        Responder<T>(
            ResultadoSeguimientoSemanal<T>
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
            TipoErrorSeguimientoSemanal
                .NoEncontrado =>
                NotFound(
                    new
                    {
                        error =
                            resultado.Error
                    }
                ),

            TipoErrorSeguimientoSemanal
                .Validacion =>
                BadRequest(
                    new
                    {
                        error =
                            resultado.Error
                    }
                ),

            TipoErrorSeguimientoSemanal
                .Conflicto =>
                Conflict(
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