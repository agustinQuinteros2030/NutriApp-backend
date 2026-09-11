using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using NutriApi.DTOs.Dietas;
using NutriApi.Services.Dietas;

using System.Security.Claims;

namespace NutriApi.Controllers;

[ApiController]

[Authorize(
    Roles =
        Roles.Nutricionista
)]

[Route(
    "api/pacientes/{pacienteId:int}/dietas/{dietaId:int}/complementos"
)]
public class ComplementosDietaController
    : ControllerBase
{
    private readonly IComplementoDietaService
        _service;


    public ComplementosDietaController(
        IComplementoDietaService service)
    {
        _service = service;
    }


    // ==========================================
    // HIDRATACIÓN - GET
    // ==========================================

    [HttpGet("hidratacion")]
    public async Task<IActionResult>
        ObtenerHidratacion(
            int pacienteId,
            int dietaId)
    {
        var nutricionistaId =
            ObtenerNutricionistaId();


        if (!nutricionistaId.HasValue)
        {
            return Unauthorized();
        }


        var resultado =
            await _service
                .ObtenerHidratacionAsync(
                    nutricionistaId.Value,
                    pacienteId,
                    dietaId
                );


        return Responder(
            resultado
        );
    }


    // ==========================================
    // HIDRATACIÓN - GUARDAR
    // ==========================================

    [HttpPut("hidratacion")]
    public async Task<IActionResult>
        GuardarHidratacion(
            int pacienteId,
            int dietaId,
            [FromBody]
            GuardarHidratacionDietaDto dto)
    {
        var nutricionistaId =
            ObtenerNutricionistaId();


        if (!nutricionistaId.HasValue)
        {
            return Unauthorized();
        }


        var resultado =
            await _service
                .GuardarHidratacionAsync(
                    nutricionistaId.Value,
                    pacienteId,
                    dietaId,
                    dto
                );


        return Responder(
            resultado
        );
    }


    // ==========================================
    // SUPLEMENTACIÓN - GET
    // ==========================================

    [HttpGet("suplementacion")]
    public async Task<IActionResult>
        ObtenerSuplementacion(
            int pacienteId,
            int dietaId)
    {
        var nutricionistaId =
            ObtenerNutricionistaId();


        if (!nutricionistaId.HasValue)
        {
            return Unauthorized();
        }


        var resultado =
            await _service
                .ObtenerSuplementacionAsync(
                    nutricionistaId.Value,
                    pacienteId,
                    dietaId
                );


        return Responder(
            resultado
        );
    }


    // ==========================================
    // SUPLEMENTACIÓN - GUARDAR
    // ==========================================

    [HttpPut("suplementacion")]
    public async Task<IActionResult>
        GuardarSuplementacion(
            int pacienteId,
            int dietaId,
            [FromBody]
            GuardarSuplementacionDietaDto dto)
    {
        var nutricionistaId =
            ObtenerNutricionistaId();


        if (!nutricionistaId.HasValue)
        {
            return Unauthorized();
        }


        var resultado =
            await _service
                .GuardarSuplementacionAsync(
                    nutricionistaId.Value,
                    pacienteId,
                    dietaId,
                    dto
                );


        return Responder(
            resultado
        );
    }


    // ==========================================
    // AGREGAR SUPLEMENTO
    // ==========================================

    [HttpPost("suplementacion/items")]
    public async Task<IActionResult>
        AgregarItemSuplementacion(
            int pacienteId,
            int dietaId,
            [FromBody]
            CrearItemSuplementacionDto dto)
    {
        var nutricionistaId =
            ObtenerNutricionistaId();


        if (!nutricionistaId.HasValue)
        {
            return Unauthorized();
        }


        var resultado =
            await _service
                .AgregarItemSuplementacionAsync(
                    nutricionistaId.Value,
                    pacienteId,
                    dietaId,
                    dto
                );


        return Responder(
            resultado
        );
    }


    // ==========================================
    // EDITAR SUPLEMENTO
    // ==========================================

    [HttpPut(
        "suplementacion/items/{itemId:int}"
    )]
    public async Task<IActionResult>
        EditarItemSuplementacion(
            int pacienteId,
            int dietaId,
            int itemId,
            [FromBody]
            EditarItemSuplementacionDto dto)
    {
        var nutricionistaId =
            ObtenerNutricionistaId();


        if (!nutricionistaId.HasValue)
        {
            return Unauthorized();
        }


        var resultado =
            await _service
                .EditarItemSuplementacionAsync(
                    nutricionistaId.Value,
                    pacienteId,
                    dietaId,
                    itemId,
                    dto
                );


        return Responder(
            resultado
        );
    }


    // ==========================================
    // ELIMINAR SUPLEMENTO
    // ==========================================

    [HttpDelete(
        "suplementacion/items/{itemId:int}"
    )]
    public async Task<IActionResult>
        EliminarItemSuplementacion(
            int pacienteId,
            int dietaId,
            int itemId)
    {
        var nutricionistaId =
            ObtenerNutricionistaId();


        if (!nutricionistaId.HasValue)
        {
            return Unauthorized();
        }


        var resultado =
            await _service
                .EliminarItemSuplementacionAsync(
                    nutricionistaId.Value,
                    pacienteId,
                    dietaId,
                    itemId
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
            out var nutricionistaId))
        {
            return null;
        }


        return nutricionistaId;
    }


    // ==========================================
    // RESPUESTA GENÉRICA
    // ==========================================

    private IActionResult
        Responder<T>(
            ResultadoDieta<T> resultado)
    {
        if (resultado.Exitoso)
        {
            return Ok(
                resultado.Datos
            );
        }


        return resultado.TipoError switch
        {
            TipoErrorDieta.NoEncontrado =>
                NotFound(
                    new
                    {
                        error =
                            resultado.Error
                    }
                ),

            TipoErrorDieta.Validacion =>
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