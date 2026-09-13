using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using NutriApi.DTOs.RegistroDiario;
using NutriApi.Services.RegistroDiario;

using System.Security.Claims;

namespace NutriApi.Controllers;

[ApiController]
[Route("api")]
public class RegistrosDiariosController
    : ControllerBase
{
    private readonly IRegistroDiarioService
        _service;


    public RegistrosDiariosController(
        IRegistroDiarioService service)
    {
        _service = service;
    }


    // ==========================================
    // PACIENTE - CREAR / ACTUALIZAR HOY/FECHA
    // ==========================================

    [Authorize(
        Roles =
            Roles.Paciente
    )]
    [HttpPut(
        "paciente/registros-diarios"
    )]
    public async Task<IActionResult>
        GuardarPropio(
            [FromBody]
            GuardarRegistroDiarioDto dto)
    {
        var pacienteId =
            ObtenerUsuarioId();


        if (!pacienteId.HasValue)
        {
            return Unauthorized();
        }


        var resultado =
            await _service
                .GuardarPropioAsync(
                    pacienteId.Value,
                    dto
                );


        return Responder(
            resultado
        );
    }


    // ==========================================
    // PACIENTE - HISTORIAL
    // ==========================================

    [Authorize(
        Roles =
            Roles.Paciente
    )]
    [HttpGet(
        "paciente/registros-diarios"
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
    // PACIENTE - UNA FECHA
    // ==========================================

    [Authorize(
        Roles =
            Roles.Paciente
    )]
    [HttpGet(
        "paciente/registros-diarios/{fecha}"
    )]
    public async Task<IActionResult>
        ObtenerPropioPorFecha(
            DateOnly fecha)
    {
        var pacienteId =
            ObtenerUsuarioId();


        if (!pacienteId.HasValue)
        {
            return Unauthorized();
        }


        var resultado =
            await _service
                .ObtenerPropioPorFechaAsync(
                    pacienteId.Value,
                    fecha
                );


        return Responder(
            resultado
        );
    }


    // ==========================================
    // NUTRICIONISTA - HISTORIAL DEL PACIENTE
    // ==========================================

    [Authorize(
        Roles =
            Roles.Nutricionista
    )]
    [HttpGet(
        "pacientes/{pacienteId:int}/registros-diarios"
    )]
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
    // JWT
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
            ResultadoRegistroDiario<T>
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
            TipoErrorRegistroDiario
                .NoEncontrado =>
                NotFound(
                    new
                    {
                        error =
                            resultado.Error
                    }
                ),

            TipoErrorRegistroDiario
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
}