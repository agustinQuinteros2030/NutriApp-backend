using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NutriApi.DTOs.Dietas;
using NutriApi.Services.Dietas;

using System.Security.Claims;

namespace NutriApi.Controllers;


[ApiController]
[Route(
    "api/pacientes/{pacienteId:int}/dietas/{dietaId:int}/comidas"
)]
[Authorize(Roles = Roles.Nutricionista)]
public class ComidasController : ControllerBase
{
    private readonly IComidaService
        _comidaService;


    public ComidasController(
        IComidaService comidaService)
    {
        _comidaService = comidaService;
    }


    // ==========================================
    // COMIDAS
    // ==========================================

    [HttpPost]
    public async Task<IActionResult> CrearComida(
        int pacienteId,
        int dietaId,
        CrearComidaDto dto)
    {
        var resultado =
            await _comidaService.CrearComidaAsync(
                ObtenerUsuarioIdActual(),
                pacienteId,
                dietaId,
                dto
            );


        return ConvertirResultado(resultado);
    }


    [HttpGet]
    public async Task<IActionResult> ObtenerComidas(
        int pacienteId,
        int dietaId)
    {
        var resultado =
            await _comidaService.ObtenerComidasAsync(
                ObtenerUsuarioIdActual(),
                pacienteId,
                dietaId
            );


        return ConvertirResultado(resultado);
    }


    [HttpGet("{comidaId:int}")]
    public async Task<IActionResult> ObtenerComida(
        int pacienteId,
        int dietaId,
        int comidaId)
    {
        var resultado =
            await _comidaService.ObtenerComidaAsync(
                ObtenerUsuarioIdActual(),
                pacienteId,
                dietaId,
                comidaId
            );


        return ConvertirResultado(resultado);
    }


    [HttpPut("{comidaId:int}")]
    public async Task<IActionResult> EditarComida(
        int pacienteId,
        int dietaId,
        int comidaId,
        EditarComidaDto dto)
    {
        var resultado =
            await _comidaService.EditarComidaAsync(
                ObtenerUsuarioIdActual(),
                pacienteId,
                dietaId,
                comidaId,
                dto
            );


        return ConvertirResultado(resultado);
    }


    // ==========================================
    // SECCIONES
    // ==========================================

    [HttpPost("{comidaId:int}/secciones")]
    public async Task<IActionResult> CrearSeccion(
        int pacienteId,
        int dietaId,
        int comidaId,
        CrearSeccionComidaDto dto)
    {
        var resultado =
            await _comidaService.CrearSeccionAsync(
                ObtenerUsuarioIdActual(),
                pacienteId,
                dietaId,
                comidaId,
                dto
            );


        return ConvertirResultado(resultado);
    }


    [HttpGet("{comidaId:int}/secciones")]
    public async Task<IActionResult> ObtenerSecciones(
        int pacienteId,
        int dietaId,
        int comidaId)
    {
        var resultado =
            await _comidaService.ObtenerSeccionesAsync(
                ObtenerUsuarioIdActual(),
                pacienteId,
                dietaId,
                comidaId
            );


        return ConvertirResultado(resultado);
    }


    [HttpPut(
        "{comidaId:int}/secciones/{seccionId:int}"
    )]
    public async Task<IActionResult> EditarSeccion(
        int pacienteId,
        int dietaId,
        int comidaId,
        int seccionId,
        EditarSeccionComidaDto dto)
    {
        var resultado =
            await _comidaService.EditarSeccionAsync(
                ObtenerUsuarioIdActual(),
                pacienteId,
                dietaId,
                comidaId,
                seccionId,
                dto
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
    // RESPUESTAS
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

    [HttpDelete("{comidaId:int}")]
    public async Task<IActionResult> EliminarComida(
    int pacienteId,
    int dietaId,
    int comidaId)
    {
        var resultado =
            await _comidaService
                .EliminarComidaAsync(
                    ObtenerUsuarioIdActual(),
                    pacienteId,
                    dietaId,
                    comidaId
                );


        return ConvertirResultado(resultado);
    }


    [HttpDelete(
    "{comidaId:int}/secciones/{seccionId:int}"
)]
    public async Task<IActionResult> EliminarSeccion(
    int pacienteId,
    int dietaId,
    int comidaId,
    int seccionId)
    {
        var resultado =
            await _comidaService
                .EliminarSeccionAsync(
                    ObtenerUsuarioIdActual(),
                    pacienteId,
                    dietaId,
                    comidaId,
                    seccionId
                );


        return ConvertirResultado(resultado);
    }

}