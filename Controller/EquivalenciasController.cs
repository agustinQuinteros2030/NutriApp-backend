using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using NutriApi.DTOs.Equivalencias;
using NutriApi.Services.Equivalencias;

using System.Security.Claims;

namespace NutriApi.Controllers;


[ApiController]
[Route("api/equivalencias")]
[Authorize(Roles = Roles.Nutricionista)]
public class EquivalenciasController : ControllerBase
{
    private readonly IEquivalenciaService
        _equivalenciaService;


    public EquivalenciasController(
        IEquivalenciaService equivalenciaService)
    {
        _equivalenciaService =
            equivalenciaService;
    }


    // ==========================================
    // GRUPOS
    // ==========================================

    [HttpPost("grupos")]
    public async Task<IActionResult> CrearGrupo(
        CrearGrupoEquivalenciaDto dto)
    {
        var resultado =
            await _equivalenciaService
                .CrearGrupoAsync(
                    ObtenerUsuarioIdActual(),
                    dto
                );


        return ConvertirResultado(resultado);
    }


    [HttpGet("grupos")]
    public async Task<IActionResult> ObtenerGrupos(
        [FromQuery] bool incluirInactivos = false)
    {
        var grupos =
            await _equivalenciaService
                .ObtenerGruposAsync(
                    ObtenerUsuarioIdActual(),
                    incluirInactivos
                );


        return Ok(grupos);
    }


    [HttpGet("grupos/{grupoId:int}")]
    public async Task<IActionResult> ObtenerGrupo(
        int grupoId)
    {
        var resultado =
            await _equivalenciaService
                .ObtenerGrupoPorIdAsync(
                    ObtenerUsuarioIdActual(),
                    grupoId
                );


        return ConvertirResultado(resultado);
    }


    [HttpPut("grupos/{grupoId:int}")]
    public async Task<IActionResult> EditarGrupo(
        int grupoId,
        EditarGrupoEquivalenciaDto dto)
    {
        var resultado =
            await _equivalenciaService
                .EditarGrupoAsync(
                    ObtenerUsuarioIdActual(),
                    grupoId,
                    dto
                );


        return ConvertirResultado(resultado);
    }


    [HttpPatch("grupos/{grupoId:int}/activar")]
    public async Task<IActionResult> ActivarGrupo(
        int grupoId)
    {
        var resultado =
            await _equivalenciaService
                .CambiarEstadoGrupoAsync(
                    ObtenerUsuarioIdActual(),
                    grupoId,
                    true
                );


        return ConvertirResultado(resultado);
    }


    [HttpPatch("grupos/{grupoId:int}/desactivar")]
    public async Task<IActionResult> DesactivarGrupo(
        int grupoId)
    {
        var resultado =
            await _equivalenciaService
                .CambiarEstadoGrupoAsync(
                    ObtenerUsuarioIdActual(),
                    grupoId,
                    false
                );


        return ConvertirResultado(resultado);
    }


    // ==========================================
    // ALIMENTOS DEL GRUPO
    // ==========================================

    [HttpPost(
        "grupos/{grupoId:int}/alimentos"
    )]
    public async Task<IActionResult> AgregarAlimento(
        int grupoId,
        AgregarEquivalenciaAlimentoDto dto)
    {
        var resultado =
            await _equivalenciaService
                .AgregarAlimentoAsync(
                    ObtenerUsuarioIdActual(),
                    grupoId,
                    dto
                );


        return ConvertirResultado(resultado);
    }




    [HttpPatch(
        "grupos/{grupoId:int}/alimentos/{equivalenciaId:int}/activar"
    )]
    public async Task<IActionResult>
        ActivarEquivalencia(
            int grupoId,
            int equivalenciaId)
    {
        var resultado =
            await _equivalenciaService
                .CambiarEstadoEquivalenciaAsync(
                    ObtenerUsuarioIdActual(),
                    grupoId,
                    equivalenciaId,
                    true
                );


        return ConvertirResultado(resultado);
    }


    [HttpPatch(
        "grupos/{grupoId:int}/alimentos/{equivalenciaId:int}/desactivar"
    )]
    public async Task<IActionResult>
        DesactivarEquivalencia(
            int grupoId,
            int equivalenciaId)
    {
        var resultado =
            await _equivalenciaService
                .CambiarEstadoEquivalenciaAsync(
                    ObtenerUsuarioIdActual(),
                    grupoId,
                    equivalenciaId,
                    false
                );


        return ConvertirResultado(resultado);
    }


    // ==========================================
    // MOTOR DE CONVERSIÓN
    // ==========================================

    [HttpGet(
        "grupos/{grupoId:int}/convertir"
    )]
    public async Task<IActionResult> Convertir(
        int grupoId,
        [FromQuery] int alimentoOrigenId,
        [FromQuery] decimal cantidad)
    {
        var resultado =
            await _equivalenciaService
                .ConvertirAsync(
                    ObtenerUsuarioIdActual(),
                    grupoId,
                    alimentoOrigenId,
                    cantidad
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
        ResultadoEquivalencia<T> resultado)
    {
        if (resultado.Exitoso)
        {
            return Ok(resultado.Datos);
        }


        return resultado.TipoError switch
        {
            TipoErrorEquivalencia.Validacion =>
                BadRequest(new
                {
                    mensaje = resultado.Error
                }),

            TipoErrorEquivalencia.NoEncontrado =>
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