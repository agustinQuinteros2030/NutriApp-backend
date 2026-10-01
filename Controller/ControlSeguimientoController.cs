using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NutriApi.DTOs.ControlSeguimiento;
using NutriApi.Services.ControlSeguimiento;
using NutriApi.Services.Dietas;

namespace NutriApi.Controllers;

[ApiController]
[Route("api")]
[Authorize(Roles = Roles.Nutricionista)]
public class ControlSeguimientoController : ControllerBase
{
    private readonly IControlSeguimientoService _controlSeguimientoService;

    public ControlSeguimientoController(IControlSeguimientoService controlSeguimientoService)
    {
        _controlSeguimientoService = controlSeguimientoService;
    }

    // ==========================================
    // CONFIGURAR PACIENTE
    // ==========================================

    [HttpPut("pacientes/{pacienteId:int}/control-seguimiento")]
    public async Task<IActionResult> Configurar(
        int pacienteId,
        [FromBody] ConfigurarControlSeguimientoDto dto
    )
    {
        var resultado = await _controlSeguimientoService.ConfigurarAsync(
            ObtenerUsuarioIdActual(),
            pacienteId,
            dto
        );

        return ConvertirResultado(resultado);
    }

    // ==========================================
    // OBTENER CONTROL DEL PACIENTE
    // ==========================================

    [HttpGet("pacientes/{pacienteId:int}/control-seguimiento")]
    public async Task<IActionResult> ObtenerPaciente(int pacienteId)
    {
        var resultado = await _controlSeguimientoService.ObtenerPacienteAsync(
            ObtenerUsuarioIdActual(),
            pacienteId
        );

        return ConvertirResultado(resultado);
    }

    // ==========================================
    // MARCAR SEGUIMIENTO REALIZADO
    // ==========================================

    [HttpPost("pacientes/{pacienteId:int}/control-seguimiento/marcar-realizado")]
    public async Task<IActionResult> MarcarRealizado(int pacienteId)
    {
        var resultado = await _controlSeguimientoService.MarcarRealizadoAsync(
            ObtenerUsuarioIdActual(),
            pacienteId
        );

        return ConvertirResultado(resultado);
    }

    // ==========================================
    // REPROGRAMAR
    // ==========================================

    [HttpPatch("pacientes/{pacienteId:int}/control-seguimiento/reprogramar")]
    public async Task<IActionResult> Reprogramar(
        int pacienteId,
        [FromBody] ReprogramarControlSeguimientoDto dto
    )
    {
        var resultado = await _controlSeguimientoService.ReprogramarAsync(
            ObtenerUsuarioIdActual(),
            pacienteId,
            dto
        );

        return ConvertirResultado(resultado);
    }

    // ==========================================
    // ACTIVAR
    // ==========================================

    [HttpPatch("pacientes/{pacienteId:int}/control-seguimiento/activar")]
    public async Task<IActionResult> Activar(int pacienteId)
    {
        var resultado = await _controlSeguimientoService.ActivarAsync(
            ObtenerUsuarioIdActual(),
            pacienteId
        );

        return ConvertirResultado(resultado);
    }

    // ==========================================
    // DESACTIVAR
    // ==========================================

    [HttpPatch("pacientes/{pacienteId:int}/control-seguimiento/desactivar")]
    public async Task<IActionResult> Desactivar(int pacienteId)
    {
        var resultado = await _controlSeguimientoService.DesactivarAsync(
            ObtenerUsuarioIdActual(),
            pacienteId
        );

        return ConvertirResultado(resultado);
    }

    // ==========================================
    // TODOS LOS PACIENTES
    // ==========================================

    [HttpGet("control-seguimientos")]
    public async Task<IActionResult> ObtenerTodos([FromQuery] bool incluirInactivos = false)
    {
        var resultado = await _controlSeguimientoService.ObtenerTodosAsync(
            ObtenerUsuarioIdActual(),
            incluirInactivos
        );

        return ConvertirResultado(resultado);
    }

    // ==========================================
    // RESUMEN DASHBOARD
    // ==========================================

    [HttpGet("control-seguimientos/resumen")]
    public async Task<IActionResult> ObtenerResumen()
    {
        var resultado = await _controlSeguimientoService.ObtenerResumenAsync(
            ObtenerUsuarioIdActual()
        );

        return ConvertirResultado(resultado);
    }

    // ==========================================
    // JWT
    // ==========================================

    private int ObtenerUsuarioIdActual()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(claim, out var id))
        {
            throw new UnauthorizedAccessException("Usuario no válido.");
        }

        return id;
    }

    // ==========================================
    // RESULTADOS
    // ==========================================

    private IActionResult ConvertirResultado<T>(ResultadoControlSeguimiento<T> resultado)
    {
        if (resultado.Exitoso)
        {
            return Ok(resultado.Datos);
        }

        return resultado.TipoError switch
        {
            TipoErrorControlSeguimiento.Validacion => BadRequest(new { mensaje = resultado.Error }),

            TipoErrorControlSeguimiento.NoEncontrado => NotFound(new { mensaje = resultado.Error }),

            TipoErrorControlSeguimiento.Conflicto => Conflict(new { mensaje = resultado.Error }),

            _ => StatusCode(
                StatusCodes.Status500InternalServerError,
                new { mensaje = "Ocurrió un error interno." }
            ),
        };
    }
}
