using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using NutriApi.DTOs.Pagos;
using NutriApi.Services.Pagos;

using System.Security.Claims;

namespace NutriApi.Controllers;

[ApiController]

[Authorize(
    Roles =
        Roles.Nutricionista
)]

[Route(
    "api/pacientes/{pacienteId:int}/pagos"
)]
public class PagosController
    : ControllerBase
{
    private readonly IPagoPacienteService
        _service;


    public PagosController(
        IPagoPacienteService service)
    {
        _service = service;
    }


    // ==========================================
    // CREAR
    // ==========================================

    [HttpPost]
    public async Task<IActionResult>
        Crear(
            int pacienteId,
            [FromBody]
            CrearPagoPacienteDto dto)
    {
        var nutricionistaId =
            ObtenerNutricionistaId();


        if (!nutricionistaId.HasValue)
        {
            return Unauthorized();
        }


        var resultado =
            await _service.CrearAsync(
                nutricionistaId.Value,
                pacienteId,
                dto
            );


        return Responder(
            resultado
        );
    }


    // ==========================================
    // HISTORIAL
    // ==========================================

    [HttpGet]
    public async Task<IActionResult>
        ObtenerTodos(
            int pacienteId)
    {
        var nutricionistaId =
            ObtenerNutricionistaId();


        if (!nutricionistaId.HasValue)
        {
            return Unauthorized();
        }


        var resultado =
            await _service
                .ObtenerTodosAsync(
                    nutricionistaId.Value,
                    pacienteId
                );


        return Responder(
            resultado
        );
    }


    // ==========================================
    // ESTADO ACTUAL
    // ==========================================

    [HttpGet("estado")]
    public async Task<IActionResult>
        ObtenerEstado(
            int pacienteId)
    {
        var nutricionistaId =
            ObtenerNutricionistaId();


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
    // EDITAR
    // ==========================================

    [HttpPut("{pagoId:int}")]
    public async Task<IActionResult>
        Editar(
            int pacienteId,
            int pagoId,
            [FromBody]
            EditarPagoPacienteDto dto)
    {
        var nutricionistaId =
            ObtenerNutricionistaId();


        if (!nutricionistaId.HasValue)
        {
            return Unauthorized();
        }


        var resultado =
            await _service.EditarAsync(
                nutricionistaId.Value,
                pacienteId,
                pagoId,
                dto
            );


        return Responder(
            resultado
        );
    }


    // ==========================================
    // CLAIM
    // ==========================================

    private int?
        ObtenerNutricionistaId()
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
            ResultadoPago<T> resultado)
    {
        if (resultado.Exitoso)
        {
            return Ok(
                resultado.Datos
            );
        }


        return resultado.TipoError switch
        {
            TipoErrorPago.NoEncontrado =>
                NotFound(
                    new
                    {
                        error =
                            resultado.Error
                    }
                ),

            TipoErrorPago.Validacion =>
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