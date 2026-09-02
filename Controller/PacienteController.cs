using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


using NutriApi.DTOs.Pacientes;
using NutriApi.Services.Pacientes;

using System.Security.Claims;

namespace NutriApi.Controllers;


[ApiController]
[Route("api/pacientes")]
[Authorize(Roles = Roles.Nutricionista)]
public class PacientesController : ControllerBase
{
    private readonly IPacienteService _pacienteService;


    public PacientesController(
        IPacienteService pacienteService)
    {
        _pacienteService = pacienteService;
    }


    // ========================================
    // CREAR
    // ========================================

    [HttpPost]
    public async Task<IActionResult> Crear(
        CrearPacienteDto dto)
    {
        var nutricionistaId =
            ObtenerUsuarioIdActual();


        var resultado =
            await _pacienteService
                .CrearAsync(
                    nutricionistaId,
                    dto
                );


        return ConvertirResultado(resultado);
    }


    // ========================================
    // LISTAR / BUSCAR
    // ========================================

    [HttpGet]
    public async Task<IActionResult> ObtenerTodos(
        [FromQuery] string? buscar)
    {
        var nutricionistaId =
            ObtenerUsuarioIdActual();


        var pacientes =
            await _pacienteService
                .ObtenerTodosAsync(
                    nutricionistaId,
                    buscar
                );


        return Ok(pacientes);
    }


    // ========================================
    // DETALLE
    // ========================================

    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObtenerPorId(
        int id)
    {
        var resultado =
            await _pacienteService
                .ObtenerPorIdAsync(
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
        EditarPacienteDto dto)
    {
        var resultado =
            await _pacienteService
                .EditarAsync(
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
            await _pacienteService
                .CambiarEstadoAsync(
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
            await _pacienteService
                .CambiarEstadoAsync(
                    ObtenerUsuarioIdActual(),
                    id,
                    false
                );


        return ConvertirResultado(resultado);
    }


    // ========================================
    // OBTENER ID DESDE JWT
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
        ResultadoPaciente<T> resultado)
    {
        if (resultado.Exitoso)
        {
            return Ok(resultado.Datos);
        }


        return resultado.TipoError switch
        {
            TipoErrorPaciente.Validacion =>
                BadRequest(new
                {
                    mensaje = resultado.Error
                }),

            TipoErrorPaciente.NoEncontrado =>
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