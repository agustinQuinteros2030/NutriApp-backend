using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NutriApi.DTOs.Competencias;
using NutriApi.Services.Competencias;

namespace NutriApi.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public class CompetenciasController : ControllerBase
{
    private readonly ICompetenciaService _competenciaService;

    public CompetenciasController(ICompetenciaService competenciaService)
    {
        _competenciaService = competenciaService;
    }

    // ==========================================
    // NUTRICIONISTA - CREAR COMPETENCIA
    // ==========================================

    [Authorize(Roles = "Nutricionista")]
    [HttpPost("pacientes/{pacienteId:int}/competencias")]
    public async Task<IActionResult> Crear(int pacienteId, [FromBody] CrearCompetenciaDto dto)
    {
        var nutricionistaId = ObtenerUsuarioId();

        if (!nutricionistaId.HasValue)
        {
            return Unauthorized();
        }

        var resultado = await _competenciaService.CrearAsync(
            nutricionistaId.Value,
            pacienteId,
            dto
        );

        if (!resultado.Exitoso)
        {
            return ResponderError(resultado);
        }

        return CreatedAtAction(
            nameof(ObtenerDetalle),
            new { pacienteId, competenciaId = resultado.Datos!.Id },
            resultado.Datos
        );
    }

    // ==========================================
    // NUTRICIONISTA - EDITAR COMPETENCIA
    // ==========================================

    [Authorize(Roles = "Nutricionista")]
    [HttpPut("pacientes/{pacienteId:int}/competencias/{competenciaId:int}")]
    public async Task<IActionResult> Editar(
        int pacienteId,
        int competenciaId,
        [FromBody] EditarCompetenciaDto dto
    )
    {
        var nutricionistaId = ObtenerUsuarioId();

        if (!nutricionistaId.HasValue)
        {
            return Unauthorized();
        }

        var resultado = await _competenciaService.EditarAsync(
            nutricionistaId.Value,
            pacienteId,
            competenciaId,
            dto
        );

        if (!resultado.Exitoso)
        {
            return ResponderError(resultado);
        }

        return Ok(resultado.Datos);
    }

    // ==========================================
    // NUTRICIONISTA - CANCELAR COMPETENCIA
    // ==========================================

    [Authorize(Roles = "Nutricionista")]
    [HttpPut("pacientes/{pacienteId:int}/competencias/{competenciaId:int}/cancelar")]
    public async Task<IActionResult> Cancelar(int pacienteId, int competenciaId)
    {
        var nutricionistaId = ObtenerUsuarioId();

        if (!nutricionistaId.HasValue)
        {
            return Unauthorized();
        }

        var resultado = await _competenciaService.CancelarAsync(
            nutricionistaId.Value,
            pacienteId,
            competenciaId
        );

        if (!resultado.Exitoso)
        {
            return ResponderError(resultado);
        }

        return Ok(resultado.Datos);
    }

    // ==========================================
    // NUTRICIONISTA - MARCAR COMO REALIZADA
    // ==========================================

    [Authorize(Roles = "Nutricionista")]
    [HttpPut("pacientes/{pacienteId:int}/competencias/{competenciaId:int}/realizar")]
    public async Task<IActionResult> MarcarRealizada(int pacienteId, int competenciaId)
    {
        var nutricionistaId = ObtenerUsuarioId();

        if (!nutricionistaId.HasValue)
        {
            return Unauthorized();
        }

        var resultado = await _competenciaService.MarcarRealizadaAsync(
            nutricionistaId.Value,
            pacienteId,
            competenciaId
        );

        if (!resultado.Exitoso)
        {
            return ResponderError(resultado);
        }

        return Ok(resultado.Datos);
    }

    // ==========================================
    // NUTRICIONISTA - COMPETENCIAS DEL PACIENTE
    // ==========================================

    [Authorize(Roles = "Nutricionista")]
    [HttpGet("pacientes/{pacienteId:int}/competencias")]
    public async Task<IActionResult> ObtenerPaciente(int pacienteId)
    {
        var nutricionistaId = ObtenerUsuarioId();

        if (!nutricionistaId.HasValue)
        {
            return Unauthorized();
        }

        var resultado = await _competenciaService.ObtenerPacienteAsync(
            nutricionistaId.Value,
            pacienteId
        );

        if (!resultado.Exitoso)
        {
            return ResponderError(resultado);
        }

        return Ok(resultado.Datos);
    }

    // ==========================================
    // NUTRICIONISTA - DETALLE DE COMPETENCIA
    // ==========================================

    [Authorize(Roles = "Nutricionista")]
    [HttpGet("pacientes/{pacienteId:int}/competencias/{competenciaId:int}")]
    public async Task<IActionResult> ObtenerDetalle(int pacienteId, int competenciaId)
    {
        var nutricionistaId = ObtenerUsuarioId();

        if (!nutricionistaId.HasValue)
        {
            return Unauthorized();
        }

        var resultado = await _competenciaService.ObtenerDetalleAsync(
            nutricionistaId.Value,
            pacienteId,
            competenciaId
        );

        if (!resultado.Exitoso)
        {
            return ResponderError(resultado);
        }

        return Ok(resultado.Datos);
    }

    // ==========================================
    // PACIENTE - MIS COMPETENCIAS
    // ==========================================

    [Authorize(Roles = "Paciente")]
    [HttpGet("paciente/mis-competencias")]
    public async Task<IActionResult> ObtenerMisCompetencias()
    {
        var pacienteId = ObtenerUsuarioId();

        if (!pacienteId.HasValue)
        {
            return Unauthorized();
        }

        var resultado = await _competenciaService.ObtenerPropiosAsync(pacienteId.Value);

        if (!resultado.Exitoso)
        {
            return ResponderError(resultado);
        }

        return Ok(resultado.Datos);
    }

    // ==========================================
    // OBTENER ID DEL JWT
    // ==========================================

    private int? ObtenerUsuarioId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(claim, out var usuarioId))
        {
            return null;
        }

        return usuarioId;
    }

    // ==========================================
    // RESPUESTAS DE ERROR
    // ==========================================

    private IActionResult ResponderError<T>(ResultadoCompetencia<T> resultado)
    {
        var respuesta = new { error = resultado.Error };

        return resultado.TipoError switch
        {
            TipoErrorCompetencia.Validacion => BadRequest(respuesta),

            TipoErrorCompetencia.NoEncontrado => NotFound(respuesta),

            TipoErrorCompetencia.Conflicto => Conflict(respuesta),

            TipoErrorCompetencia.Interno => StatusCode(
                StatusCodes.Status500InternalServerError,
                respuesta
            ),

            _ => StatusCode(StatusCodes.Status500InternalServerError, respuesta),
        };
    }
}
