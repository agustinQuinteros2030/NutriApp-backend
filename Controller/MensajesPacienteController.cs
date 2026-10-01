using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NutriApi.DTOs.Mensajes;
using NutriApi.Services.Mensajes;


namespace NutriApi.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public class MensajesPacienteController : ControllerBase
{
    private readonly IMensajePacienteService _service;

    public MensajesPacienteController(IMensajePacienteService service)
    {
        _service = service;
    }

    // ==========================================
    // NUTRICIONISTA
    // ==========================================

    [HttpPost("pacientes/{pacienteId:int}/mensajes")]
    [Authorize(Roles = Roles.Nutricionista)]
    public async Task<IActionResult> Crear(int pacienteId, [FromBody] CrearMensajePacienteDto dto)
    {
        var nutricionistaId = ObtenerUsuarioIdActual();

        var resultado = await _service.CrearAsync(nutricionistaId, pacienteId, dto);

        return ConvertirResultado(resultado);
    }

    [HttpGet("pacientes/{pacienteId:int}/mensajes")]
    [Authorize(Roles = Roles.Nutricionista)]
    public async Task<IActionResult> ObtenerPaciente(int pacienteId)
    {
        var nutricionistaId = ObtenerUsuarioIdActual();

        var resultado = await _service.ObtenerPacienteAsync(nutricionistaId, pacienteId);

        return ConvertirResultado(resultado);
    }

    [HttpPut("pacientes/{pacienteId:int}/mensajes/{mensajeId:int}")]
    [Authorize(Roles = Roles.Nutricionista)]
    public async Task<IActionResult> Editar(
        int pacienteId,
        int mensajeId,
        [FromBody] EditarMensajePacienteDto dto
    )
    {
        var nutricionistaId = ObtenerUsuarioIdActual();

        var resultado = await _service.EditarAsync(nutricionistaId, pacienteId, mensajeId, dto);

        return ConvertirResultado(resultado);
    }

    [HttpDelete("pacientes/{pacienteId:int}/mensajes/{mensajeId:int}")]
    [Authorize(Roles = Roles.Nutricionista)]
    public async Task<IActionResult> Eliminar(int pacienteId, int mensajeId)
    {
        var nutricionistaId = ObtenerUsuarioIdActual();

        var resultado = await _service.EliminarAsync(nutricionistaId, pacienteId, mensajeId);

        return ConvertirResultado(resultado);
    }

    // ==========================================
    // PACIENTE
    // ==========================================

    [HttpGet("paciente/mensajes")]
    [Authorize(Roles = Roles.Paciente)]
    public async Task<IActionResult> ObtenerPropios()
    {
        var pacienteId = ObtenerUsuarioIdActual();

        var resultado = await _service.ObtenerPropiosAsync(pacienteId);

        return ConvertirResultado(resultado);
    }

    [HttpGet("paciente/mensajes/{mensajeId:int}")]
    [Authorize(Roles = Roles.Paciente)]
    public async Task<IActionResult> ObtenerDetalle(int mensajeId)
    {
        var pacienteId = ObtenerUsuarioIdActual();

        var resultado = await _service.ObtenerDetallePacienteAsync(pacienteId, mensajeId);

        return ConvertirResultado(resultado);
    }

    [HttpPut("paciente/mensajes/{mensajeId:int}/leer")]
    [Authorize(Roles = Roles.Paciente)]
    public async Task<IActionResult> MarcarComoLeido(int mensajeId)
    {
        var pacienteId = ObtenerUsuarioIdActual();

        var resultado = await _service.MarcarComoLeidoAsync(pacienteId, mensajeId);

        return ConvertirResultado(resultado);
    }

    // ==========================================
    // HELPERS
    // ==========================================

    private int ObtenerUsuarioIdActual()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(claim, out var usuarioId))
        {
            throw new UnauthorizedAccessException("No se pudo obtener el usuario autenticado.");
        }

        return usuarioId;
    }

    private IActionResult ConvertirResultado<T>(ResultadoMensajePaciente<T> resultado)
    {
        if (resultado.Exitoso)
        {
            return Ok(resultado.Datos);
        }

        return resultado.TipoError switch
        {
            TipoErrorMensajePaciente.Validacion => BadRequest(new { error = resultado.Error }),

            TipoErrorMensajePaciente.NoEncontrado => NotFound(new { error = resultado.Error }),

            TipoErrorMensajePaciente.Conflicto => Conflict(new { error = resultado.Error }),

            _ => StatusCode(
                StatusCodes.Status500InternalServerError,
                new { error = resultado.Error }
            ),
        };
    }
}
