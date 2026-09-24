using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NutriApi.DTOs.Dietas;
using NutriApi.Services.Dietas;

using System.Security.Claims;

namespace NutriApi.Controllers;


[ApiController]
[Route(
    "api/pacientes/{pacienteId:int}/dietas/{dietaId:int}/comidas/{comidaId:int}/secciones/{seccionId:int}/opciones"
)]
[Authorize(Roles = Roles.Nutricionista)]
public class OpcionesComidaController : ControllerBase
{
    private readonly IOpcionComidaService
        _opcionService;


    public OpcionesComidaController(
        IOpcionComidaService opcionService)
    {
        _opcionService = opcionService;
    }


    // ==========================================
    // OPCIONES
    // ==========================================

    [HttpPost]
    public async Task<IActionResult> CrearOpcion(
        int pacienteId,
        int dietaId,
        int comidaId,
        int seccionId,
        CrearOpcionSeccionComidaDto dto)
    {
        var resultado =
            await _opcionService.CrearOpcionAsync(
                ObtenerUsuarioIdActual(),
                pacienteId,
                dietaId,
                comidaId,
                seccionId,
                dto
            );


        return ConvertirResultado(resultado);
    }


    [HttpGet]
    public async Task<IActionResult> ObtenerOpciones(
        int pacienteId,
        int dietaId,
        int comidaId,
        int seccionId)
    {
        var resultado =
            await _opcionService.ObtenerOpcionesAsync(
                ObtenerUsuarioIdActual(),
                pacienteId,
                dietaId,
                comidaId,
                seccionId
            );


        return ConvertirResultado(resultado);
    }


    [HttpGet("{opcionId:int}")]
    public async Task<IActionResult> ObtenerOpcion(
        int pacienteId,
        int dietaId,
        int comidaId,
        int seccionId,
        int opcionId)
    {
        var resultado =
            await _opcionService.ObtenerOpcionAsync(
                ObtenerUsuarioIdActual(),
                pacienteId,
                dietaId,
                comidaId,
                seccionId,
                opcionId
            );


        return ConvertirResultado(resultado);
    }


    [HttpPut("{opcionId:int}")]
    public async Task<IActionResult> EditarOpcion(
        int pacienteId,
        int dietaId,
        int comidaId,
        int seccionId,
        int opcionId,
        EditarOpcionSeccionComidaDto dto)
    {
        var resultado =
            await _opcionService.EditarOpcionAsync(
                ObtenerUsuarioIdActual(),
                pacienteId,
                dietaId,
                comidaId,
                seccionId,
                opcionId,
                dto
            );


        return ConvertirResultado(resultado);
    }



    [HttpDelete("{opcionId:int}")]
    public async Task<IActionResult> EliminarOpcion(
    int pacienteId,
    int dietaId,
    int comidaId,
    int seccionId,
    int opcionId)
    {
        var resultado =
            await _opcionService
                .EliminarOpcionAsync(
                    ObtenerUsuarioIdActual(),
                    pacienteId,
                    dietaId,
                    comidaId,
                    seccionId,
                    opcionId
                );


        return ConvertirResultado(resultado);
    }


    // ==========================================
    // ITEMS
    // ==========================================

    [HttpPost("{opcionId:int}/items")]
    public async Task<IActionResult> CrearItem(
        int pacienteId,
        int dietaId,
        int comidaId,
        int seccionId,
        int opcionId,
        CrearItemOpcionComidaDto dto)
    {
        var resultado =
            await _opcionService.CrearItemAsync(
                ObtenerUsuarioIdActual(),
                pacienteId,
                dietaId,
                comidaId,
                seccionId,
                opcionId,
                dto
            );


        return ConvertirResultado(resultado);
    }


    [HttpPut(
        "{opcionId:int}/items/{itemId:int}"
    )]
    public async Task<IActionResult> EditarItem(
        int pacienteId,
        int dietaId,
        int comidaId,
        int seccionId,
        int opcionId,
        int itemId,
        EditarItemOpcionComidaDto dto)
    {
        var resultado =
            await _opcionService.EditarItemAsync(
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
    [HttpDelete(
    "{opcionId:int}/items/{itemId:int}"
)]
    public async Task<IActionResult> EliminarItem(
    int pacienteId,
    int dietaId,
    int comidaId,
    int seccionId,
    int opcionId,
    int itemId)
    {
        var resultado =
            await _opcionService
                .EliminarItemAsync(
                    ObtenerUsuarioIdActual(),
                    pacienteId,
                    dietaId,
                    comidaId,
                    seccionId,
                    opcionId,
                    itemId
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
    // RESULTADOS
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
                    mensaje = resultado.Error
                }),

            TipoErrorDieta.NoEncontrado =>
                NotFound(new
                {
                    mensaje = resultado.Error
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