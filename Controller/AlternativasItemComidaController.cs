using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using NutriApi.DTOs.Dietas;
using NutriApi.Services.Dietas;

using System.Security.Claims;

namespace NutriApi.Controllers;


[ApiController]
[Route(
    "api/pacientes/{pacienteId:int}" +
    "/dietas/{dietaId:int}" +
    "/comidas/{comidaId:int}" +
    "/secciones/{seccionId:int}" +
    "/opciones/{opcionId:int}" +
    "/items/{itemId:int}" +
    "/alternativas"
)]
[Authorize(Roles = Roles.Nutricionista)]
public class AlternativasItemComidaController
    : ControllerBase
{
    private readonly IAlternativaItemComidaService
        _alternativaService;


    public AlternativasItemComidaController(
        IAlternativaItemComidaService alternativaService)
    {
        _alternativaService =
            alternativaService;
    }


    // ==========================================
    // AGREGAR
    // ==========================================

    [HttpPost]
    public async Task<IActionResult> Agregar(
        int pacienteId,
        int dietaId,
        int comidaId,
        int seccionId,
        int opcionId,
        int itemId,
        AgregarAlternativaItemComidaDto dto)
    {
        var resultado =
            await _alternativaService.AgregarAsync(
                ObtenerUsuarioIdActual(),
                pacienteId,
                dietaId,
                comidaId,
                seccionId,
                opcionId,
                itemId,
                dto
            );


        return ConvertirResultado(resultado);
    }


    // ==========================================
    // LISTAR
    // ==========================================

    [HttpGet]
    public async Task<IActionResult> ObtenerTodas(
        int pacienteId,
        int dietaId,
        int comidaId,
        int seccionId,
        int opcionId,
        int itemId,
        [FromQuery] bool incluirInactivas = false)
    {
        var resultado =
            await _alternativaService
                .ObtenerTodasAsync(
                    ObtenerUsuarioIdActual(),
                    pacienteId,
                    dietaId,
                    comidaId,
                    seccionId,
                    opcionId,
                    itemId,
                    incluirInactivas
                );


        return ConvertirResultado(resultado);
    }


    // ==========================================
    // ACTIVAR
    // ==========================================

    [HttpPatch("{alternativaId:int}/activar")]
    public async Task<IActionResult> Activar(
        int pacienteId,
        int dietaId,
        int comidaId,
        int seccionId,
        int opcionId,
        int itemId,
        int alternativaId)
    {
        var resultado =
            await _alternativaService
                .CambiarEstadoAsync(
                    ObtenerUsuarioIdActual(),
                    pacienteId,
                    dietaId,
                    comidaId,
                    seccionId,
                    opcionId,
                    itemId,
                    alternativaId,
                    true
                );


        return ConvertirResultado(resultado);
    }


    // ==========================================
    // DESACTIVAR
    // ==========================================

    [HttpPatch("{alternativaId:int}/desactivar")]
    public async Task<IActionResult> Desactivar(
        int pacienteId,
        int dietaId,
        int comidaId,
        int seccionId,
        int opcionId,
        int itemId,
        int alternativaId)
    {
        var resultado =
            await _alternativaService
                .CambiarEstadoAsync(
                    ObtenerUsuarioIdActual(),
                    pacienteId,
                    dietaId,
                    comidaId,
                    seccionId,
                    opcionId,
                    itemId,
                    alternativaId,
                    false
                );


        return ConvertirResultado(resultado);
    }


    // ==========================================
    // JWT
    // ==========================================

    private int ObtenerUsuarioIdActual()
    {
        var claim =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier
            );


        if (!int.TryParse(claim, out var id))
        {
            throw new UnauthorizedAccessException(
                "Usuario no válido."
            );
        }


        return id;
    }


    // ==========================================
    // RESULTADO
    // ==========================================

    private IActionResult ConvertirResultado<T>(
        ResultadoDieta<T> resultado)
    {
        if (resultado.Exitoso)
        {
            return Ok(resultado.Datos);
        }


        return resultado.TipoError switch
        {
            TipoErrorDieta.Validacion =>
                BadRequest(new
                {
                    mensaje =
                        resultado.Error
                }),

            TipoErrorDieta.NoEncontrado =>
                NotFound(new
                {
                    mensaje =
                        resultado.Error
                }),

            _ =>
                StatusCode(
                    StatusCodes
                        .Status500InternalServerError,

                    new
                    {
                        mensaje =
                            "Ocurrió un error interno."
                    }
                )
        };
    }
}