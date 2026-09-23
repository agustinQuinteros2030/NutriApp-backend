using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using NutriApi.DTOs.Turnos;
using NutriApi.Services.Turnos;

using System.Security.Claims;

namespace NutriApi.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public class TurnosController
    : ControllerBase
{
    private readonly ITurnoService
        _turnoService;


    public TurnosController(
        ITurnoService turnoService)
    {
        _turnoService =
            turnoService;
    }


    // ==========================================
    // NUTRICIONISTA - CREAR TURNO
    // ==========================================

    [Authorize(Roles = "Nutricionista")]
    [HttpPost("pacientes/{pacienteId:int}/turnos")]
    public async Task<IActionResult>
        Crear(
            int pacienteId,
            [FromBody] CrearTurnoDto dto)
    {
        var nutricionistaId =
            ObtenerUsuarioId();


        if (!nutricionistaId.HasValue)
        {
            return Unauthorized();
        }


        var resultado =
            await _turnoService
                .CrearAsync(
                    nutricionistaId.Value,
                    pacienteId,
                    dto
                );


        if (!resultado.Exitoso)
        {
            return ResponderError(
                resultado
            );
        }


        return CreatedAtAction(
            nameof(ObtenerDetalle),
            new
            {
                pacienteId,
                turnoId =
                    resultado.Datos!.Id
            },
            resultado.Datos
        );
    }


    // ==========================================
    // NUTRICIONISTA - EDITAR / REPROGRAMAR
    // ==========================================

    [Authorize(Roles = "Nutricionista")]
    [HttpPut(
        "pacientes/{pacienteId:int}/turnos/{turnoId:int}"
    )]
    public async Task<IActionResult>
        Editar(
            int pacienteId,
            int turnoId,
            [FromBody] EditarTurnoDto dto)
    {
        var nutricionistaId =
            ObtenerUsuarioId();


        if (!nutricionistaId.HasValue)
        {
            return Unauthorized();
        }


        var resultado =
            await _turnoService
                .EditarAsync(
                    nutricionistaId.Value,
                    pacienteId,
                    turnoId,
                    dto
                );


        if (!resultado.Exitoso)
        {
            return ResponderError(
                resultado
            );
        }


        return Ok(
            resultado.Datos
        );
    }


    // ==========================================
    // NUTRICIONISTA - CANCELAR
    // ==========================================

    [Authorize(Roles = "Nutricionista")]
    [HttpPut(
        "pacientes/{pacienteId:int}/turnos/{turnoId:int}/cancelar"
    )]
    public async Task<IActionResult>
        Cancelar(
            int pacienteId,
            int turnoId)
    {
        var nutricionistaId =
            ObtenerUsuarioId();


        if (!nutricionistaId.HasValue)
        {
            return Unauthorized();
        }


        var resultado =
            await _turnoService
                .CancelarAsync(
                    nutricionistaId.Value,
                    pacienteId,
                    turnoId
                );


        if (!resultado.Exitoso)
        {
            return ResponderError(
                resultado
            );
        }


        return Ok(
            resultado.Datos
        );
    }


    // ==========================================
    // NUTRICIONISTA - MARCAR REALIZADO
    // ==========================================

    [Authorize(Roles = "Nutricionista")]
    [HttpPut(
        "pacientes/{pacienteId:int}/turnos/{turnoId:int}/realizar"
    )]
    public async Task<IActionResult>
        MarcarRealizado(
            int pacienteId,
            int turnoId)
    {
        var nutricionistaId =
            ObtenerUsuarioId();


        if (!nutricionistaId.HasValue)
        {
            return Unauthorized();
        }


        var resultado =
            await _turnoService
                .MarcarRealizadoAsync(
                    nutricionistaId.Value,
                    pacienteId,
                    turnoId
                );


        if (!resultado.Exitoso)
        {
            return ResponderError(
                resultado
            );
        }


        return Ok(
            resultado.Datos
        );
    }


    // ==========================================
    // NUTRICIONISTA - PRÓXIMOS TURNOS
    // ==========================================

    [Authorize(Roles = "Nutricionista")]
    [HttpGet("nutricionista/turnos/proximos")]
    public async Task<IActionResult>
        ObtenerProximosNutricionista()
    {
        var nutricionistaId =
            ObtenerUsuarioId();


        if (!nutricionistaId.HasValue)
        {
            return Unauthorized();
        }


        var resultado =
            await _turnoService
                .ObtenerProximosNutricionistaAsync(
                    nutricionistaId.Value
                );


        if (!resultado.Exitoso)
        {
            return ResponderError(
                resultado
            );
        }


        return Ok(
            resultado.Datos
        );
    }


    // ==========================================
    // NUTRICIONISTA - HISTORIAL DEL PACIENTE
    // ==========================================

    [Authorize(Roles = "Nutricionista")]
    [HttpGet("pacientes/{pacienteId:int}/turnos")]
    public async Task<IActionResult>
        ObtenerPaciente(
            int pacienteId)
    {
        var nutricionistaId =
            ObtenerUsuarioId();


        if (!nutricionistaId.HasValue)
        {
            return Unauthorized();
        }


        var resultado =
            await _turnoService
                .ObtenerPacienteAsync(
                    nutricionistaId.Value,
                    pacienteId
                );


        if (!resultado.Exitoso)
        {
            return ResponderError(
                resultado
            );
        }


        return Ok(
            resultado.Datos
        );
    }


    // ==========================================
    // NUTRICIONISTA - DETALLE DE TURNO
    // ==========================================

    [Authorize(Roles = "Nutricionista")]
    [HttpGet(
        "pacientes/{pacienteId:int}/turnos/{turnoId:int}"
    )]
    public async Task<IActionResult>
        ObtenerDetalle(
            int pacienteId,
            int turnoId)
    {
        var nutricionistaId =
            ObtenerUsuarioId();


        if (!nutricionistaId.HasValue)
        {
            return Unauthorized();
        }


        var resultado =
            await _turnoService
                .ObtenerDetalleAsync(
                    nutricionistaId.Value,
                    pacienteId,
                    turnoId
                );


        if (!resultado.Exitoso)
        {
            return ResponderError(
                resultado
            );
        }


        return Ok(
            resultado.Datos
        );
    }


    // ==========================================
    // PACIENTE - MIS TURNOS
    // ==========================================

    [Authorize(Roles = "Paciente")]
    [HttpGet("paciente/mis-turnos")]
    public async Task<IActionResult>
        ObtenerMisTurnos()
    {
        var pacienteId =
            ObtenerUsuarioId();


        if (!pacienteId.HasValue)
        {
            return Unauthorized();
        }


        var resultado =
            await _turnoService
                .ObtenerPropiosAsync(
                    pacienteId.Value
                );


        if (!resultado.Exitoso)
        {
            return ResponderError(
                resultado
            );
        }


        return Ok(
            resultado.Datos
        );
    }


    // ==========================================
    // PACIENTE - PRÓXIMO TURNO
    // ==========================================

    [Authorize(Roles = "Paciente")]
    [HttpGet("paciente/proximo-turno")]
    public async Task<IActionResult>
        ObtenerProximoTurno()
    {
        var pacienteId =
            ObtenerUsuarioId();


        if (!pacienteId.HasValue)
        {
            return Unauthorized();
        }


        var resultado =
            await _turnoService
                .ObtenerProximoPacienteAsync(
                    pacienteId.Value
                );


        if (!resultado.Exitoso)
        {
            return ResponderError(
                resultado
            );
        }


        /*
         * null significa simplemente:
         *
         * "el paciente no tiene un próximo turno"
         *
         * No es un error.
         */

        return Ok(
            resultado.Datos
        );
    }


    // ==========================================
    // OBTENER ID DEL JWT
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
    // RESPUESTA DE ERRORES
    // ==========================================

    private IActionResult
        ResponderError<T>(
            ResultadoTurno<T> resultado)
    {
        var respuesta =
            new
            {
                error =
                    resultado.Error
            };


        return resultado.TipoError switch
        {
            TipoErrorTurno.Validacion =>
                BadRequest(
                    respuesta
                ),

            TipoErrorTurno.NoEncontrado =>
                NotFound(
                    respuesta
                ),

            TipoErrorTurno.Conflicto =>
                Conflict(
                    respuesta
                ),

            TipoErrorTurno.Interno =>
                StatusCode(
                    StatusCodes
                        .Status500InternalServerError,
                    respuesta
                ),

            _ =>
                StatusCode(
                    StatusCodes
                        .Status500InternalServerError,
                    respuesta
                )
        };
    }
}