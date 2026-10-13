using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NutriApi.DTOs.Dietas;
using NutriApi.Services.Dietas;

namespace NutriApi.Controllers;

[ApiController]
[Route("api/pacientes/{pacienteId:int}/dietas")]
[Authorize(Roles = "Nutricionista")]
public class DietasController : ControllerBase
{
    private readonly IDietaService _dietaService;

    public DietasController(IDietaService dietaService)
    {
        _dietaService = dietaService;
    }

    // ==========================================
    // CREAR
    // ==========================================

    [HttpPost]
    public async Task<IActionResult> Crear(int pacienteId, [FromBody] CrearDietaDto dto)
    {
        var nutricionistaId = ObtenerUsuarioIdActual();

        var resultado = await _dietaService.CrearAsync(nutricionistaId, pacienteId, dto);

        return ConvertirResultado(resultado);
    }

    // ==========================================
    // LISTAR
    // ==========================================

    [HttpGet]
    public async Task<IActionResult> ObtenerTodas(int pacienteId)
    {
        var nutricionistaId = ObtenerUsuarioIdActual();

        var resultado = await _dietaService.ObtenerTodasAsync(nutricionistaId, pacienteId);

        return ConvertirResultado(resultado);
    }

    // ==========================================
    // DETALLE
    // ==========================================

    [HttpGet("{dietaId:int}")]
    public async Task<IActionResult> ObtenerPorId(int pacienteId, int dietaId)
    {
        var nutricionistaId = ObtenerUsuarioIdActual();

        var resultado = await _dietaService.ObtenerPorIdAsync(nutricionistaId, pacienteId, dietaId);

        return ConvertirResultado(resultado);
    }

    // ==========================================
    // EDITAR
    // ==========================================

    [HttpPut("{dietaId:int}")]
    public async Task<IActionResult> Editar(
        int pacienteId,
        int dietaId,
        [FromBody] EditarDietaDto dto
    )
    {
        var nutricionistaId = ObtenerUsuarioIdActual();

        var resultado = await _dietaService.EditarAsync(nutricionistaId, pacienteId, dietaId, dto);

        return ConvertirResultado(resultado);
    }

    // ==========================================
    // DUPLICAR
    // ==========================================

    [HttpPost("{dietaId:int}/duplicar")]
    public async Task<IActionResult> Duplicar(int pacienteId, int dietaId)
    {
        var nutricionistaId = ObtenerUsuarioIdActual();

        var resultado = await _dietaService.DuplicarAsync(nutricionistaId, pacienteId, dietaId);

        return ConvertirResultado(resultado);
    }

    // ==========================================
    // COPIAR A OTRO PACIENTE
    // ==========================================

    [HttpPost("{dietaId:int}/copiar-a/{pacienteDestinoId:int}")]
    public async Task<IActionResult> CopiarAOtroPaciente(
        int pacienteId,
        int dietaId,
        int pacienteDestinoId
    )
    {
        var nutricionistaId = ObtenerUsuarioIdActual();

        var resultado = await _dietaService.CopiarAOtroPacienteAsync(
            nutricionistaId,
            pacienteId,
            dietaId,
            pacienteDestinoId
        );

        return ConvertirResultado(resultado);
    }

    // ==========================================
    // ACTIVAR AHORA
    // ==========================================

    [HttpPatch("{dietaId:int}/activar")]
    public async Task<IActionResult> Activar(int pacienteId, int dietaId)
    {
        var nutricionistaId = ObtenerUsuarioIdActual();

        var resultado = await _dietaService.ActivarAsync(nutricionistaId, pacienteId, dietaId);

        return ConvertirResultado(resultado);
    }

    // ==========================================
    // PROGRAMAR ACTIVACIÓN
    // ==========================================

    [HttpPut("{dietaId:int}/programar-activacion")]
    public async Task<IActionResult> ProgramarActivacion(
        int pacienteId,
        int dietaId,
        [FromBody] ProgramarActivacionDietaDto dto
    )
    {
        var nutricionistaId = ObtenerUsuarioIdActual();

        var resultado = await _dietaService.ProgramarActivacionAsync(
            nutricionistaId,
            pacienteId,
            dietaId,
            dto
        );

        return ConvertirResultado(resultado);
    }

    // ==========================================
    // CANCELAR ACTIVACIÓN PROGRAMADA
    // ==========================================

    [HttpDelete("{dietaId:int}/programar-activacion")]
    public async Task<IActionResult> CancelarActivacionProgramada(int pacienteId, int dietaId)
    {
        var nutricionistaId = ObtenerUsuarioIdActual();

        var resultado = await _dietaService.CancelarActivacionProgramadaAsync(
            nutricionistaId,
            pacienteId,
            dietaId
        );

        return ConvertirResultado(resultado);
    }

    // ==========================================
    // ARCHIVAR
    // ==========================================

    [HttpPatch("{dietaId:int}/archivar")]
    public async Task<IActionResult> Archivar(int pacienteId, int dietaId)
    {
        var nutricionistaId = ObtenerUsuarioIdActual();

        var resultado = await _dietaService.ArchivarAsync(nutricionistaId, pacienteId, dietaId);

        return ConvertirResultado(resultado);
    }

    // ==========================================
    // ELIMINAR
    // ==========================================

    [HttpDelete("{dietaId:int}")]
    public async Task<IActionResult> Eliminar(int pacienteId, int dietaId)
    {
        var nutricionistaId = ObtenerUsuarioIdActual();

        var resultado = await _dietaService.EliminarAsync(nutricionistaId, pacienteId, dietaId);

        return ConvertirResultado(resultado);
    }

    // ==========================================
    // USUARIO ACTUAL
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

    // ==========================================
    // CONVERTIR RESULTADO
    // ==========================================

    private IActionResult ConvertirResultado<T>(ResultadoDieta<T> resultado)
    {
        if (resultado.Exitoso)
        {
            return Ok(resultado.Datos);
        }

        return resultado.TipoError switch
        {
            TipoErrorDieta.Validacion => BadRequest(new { error = resultado.Error }),

            TipoErrorDieta.NoEncontrado => NotFound(new { error = resultado.Error }),

            _ => StatusCode(
                StatusCodes.Status500InternalServerError,
                new { error = resultado.Error }
            ),
        };
    }
}
