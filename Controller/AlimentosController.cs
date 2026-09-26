using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using NutriApi.DTOs.Alimentos;
using NutriApi.Services.Alimentos;

using System.Security.Claims;

namespace NutriApi.Controllers;


[ApiController]
[Route("api/alimentos")]
[Authorize(Roles = Roles.Nutricionista)]
public class AlimentosController : ControllerBase
{
    private readonly IAlimentoService
        _alimentoService;


    public AlimentosController(
        IAlimentoService alimentoService)
    {
        _alimentoService = alimentoService;
    }


    // ========================================
    // CREAR
    // ========================================

    [HttpPost]
    public async Task<IActionResult> Crear(
        CrearAlimentoDto dto)
    {
        var resultado =
            await _alimentoService.CrearAsync(
                ObtenerUsuarioIdActual(),
                dto
            );


        return ConvertirResultado(resultado);
    }


    // ========================================
    // LISTAR / BUSCAR / FILTRAR
    // ========================================

    [HttpGet]
    public async Task<IActionResult> ObtenerTodos(
        [FromQuery] string? buscar,
        [FromQuery] int? categoriaId,
        [FromQuery] bool incluirInactivos = false)
    {
        var alimentos =
            await _alimentoService.ObtenerTodosAsync(
                ObtenerUsuarioIdActual(),
                buscar,
                categoriaId,
                incluirInactivos
            );


        return Ok(alimentos);
    }


    // ========================================
    // DETALLE
    // ========================================

    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObtenerPorId(
        int id)
    {
        var resultado =
            await _alimentoService.ObtenerPorIdAsync(
                ObtenerUsuarioIdActual(),
                id
            );


        return ConvertirResultado(resultado);
    }


    // ========================================
    // EDITAR
    // ========================================

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Editar(
        int id,
        EditarAlimentoDto dto)
    {
        var resultado =
            await _alimentoService.EditarAsync(
                ObtenerUsuarioIdActual(),
                id,
                dto
            );


        return ConvertirResultado(resultado);
    }


    // ========================================
    // ACTIVAR
    // ========================================

    [HttpPatch("{id:int}/activar")]
    public async Task<IActionResult> Activar(
        int id)
    {
        var resultado =
            await _alimentoService.CambiarEstadoAsync(
                ObtenerUsuarioIdActual(),
                id,
                true
            );


        return ConvertirResultado(resultado);
    }


    // ========================================
    // DESACTIVAR
    // ========================================

    [HttpPatch("{id:int}/desactivar")]
    public async Task<IActionResult> Desactivar(
        int id)
    {
        var resultado =
            await _alimentoService.CambiarEstadoAsync(
                ObtenerUsuarioIdActual(),
                id,
                false
            );


        return ConvertirResultado(resultado);
    }


    // ========================================
    // ID DEL JWT
    // ========================================

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


    private IActionResult ConvertirResultado<T>(
        ResultadoAlimento<T> resultado)
    {
        if (resultado.Exitoso)
        {
            return Ok(resultado.Datos);
        }


        return resultado.TipoError switch
        {
            TipoErrorAlimento.Validacion =>
                BadRequest(new
                {
                    mensaje = resultado.Error
                }),

            TipoErrorAlimento.NoEncontrado =>
                NotFound(new
                {
                    mensaje = resultado.Error
                }),

            _ =>
                StatusCode(
                     StatusCodes.Status500InternalServerError,
                    new
                    {
                        mensaje =
                            "Ocurrió un error interno."
                    }
                )
        };
    }
}