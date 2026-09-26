using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using NutriApi.DTOs.Alimentos;
using NutriApi.Services.Alimentos;

using System.Security.Claims;

namespace NutriApi.Controllers;


[ApiController]
[Route("api/categorias-alimentos")]
[Authorize(Roles = Roles.Nutricionista)]
public class CategoriasAlimentosController
    : ControllerBase
{
    private readonly ICategoriaAlimentoService
        _categoriaService;


    public CategoriasAlimentosController(
        ICategoriaAlimentoService categoriaService)
    {
        _categoriaService = categoriaService;
    }


    [HttpPost]
    public async Task<IActionResult> Crear(
        CrearCategoriaAlimentoDto dto)
    {
        var resultado =
            await _categoriaService.CrearAsync(
                ObtenerUsuarioIdActual(),
                dto
            );


        return ConvertirResultado(resultado);
    }


    [HttpGet]
    public async Task<IActionResult> ObtenerTodas(
        [FromQuery] bool incluirInactivas = false)
    {
        var categorias =
            await _categoriaService.ObtenerTodasAsync(
                ObtenerUsuarioIdActual(),
                incluirInactivas
            );


        return Ok(categorias);
    }


    [HttpPut("{id:int}")]
    public async Task<IActionResult> Editar(
        int id,
        EditarCategoriaAlimentoDto dto)
    {
        var resultado =
            await _categoriaService.EditarAsync(
                ObtenerUsuarioIdActual(),
                id,
                dto
            );


        return ConvertirResultado(resultado);
    }


    [HttpPatch("{id:int}/activar")]
    public async Task<IActionResult> Activar(int id)
    {
        var resultado =
            await _categoriaService.CambiarEstadoAsync(
                ObtenerUsuarioIdActual(),
                id,
                true
            );


        return ConvertirResultado(resultado);
    }


    [HttpPatch("{id:int}/desactivar")]
    public async Task<IActionResult> Desactivar(int id)
    {
        var resultado =
            await _categoriaService.CambiarEstadoAsync(
                ObtenerUsuarioIdActual(),
                id,
                false
            );


        return ConvertirResultado(resultado);
    }


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