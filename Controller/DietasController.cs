using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NutriApi.DTOs.Dietas;
using NutriApi.Services.Dietas;

namespace NutriApi.Controllers;

[ApiController]
[Route("api/pacientes/{pacienteId:int}/dietas")]
[Authorize(Roles = Roles.Nutricionista)]
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
    public async Task<IActionResult> Crear(int pacienteId, CrearDietaDto dto)
    {
        var resultado = await _dietaService.CrearAsync(ObtenerUsuarioIdActual(), pacienteId, dto);

        return ConvertirResultado(resultado);
    }

    // ==========================================
    // LISTAR
    // ==========================================

    [HttpGet]
    public async Task<IActionResult> ObtenerTodas(int pacienteId)
    {
        var resultado = await _dietaService.ObtenerTodasAsync(ObtenerUsuarioIdActual(), pacienteId);

        return ConvertirResultado(resultado);
    }

    // ==========================================
    // DETALLE
    // ==========================================

    [HttpGet("{dietaId:int}")]
    public async Task<IActionResult> ObtenerPorId(int pacienteId, int dietaId)
    {
        var resultado = await _dietaService.ObtenerPorIdAsync(
            ObtenerUsuarioIdActual(),
            pacienteId,
            dietaId
        );

        return ConvertirResultado(resultado);
    }

    // ==========================================
    // EDITAR
    // ==========================================

    [HttpPut("{dietaId:int}")]
    public async Task<IActionResult> Editar(int pacienteId, int dietaId, EditarDietaDto dto)
    {
        var resultado = await _dietaService.EditarAsync(
            ObtenerUsuarioIdActual(),
            pacienteId,
            dietaId,
            dto
        );

        return ConvertirResultado(resultado);
    }

    // ==========================================
    // ACTIVAR
    // ==========================================

    [HttpPatch("{dietaId:int}/activar")]
    public async Task<IActionResult> Activar(int pacienteId, int dietaId)
    {
        var resultado = await _dietaService.ActivarAsync(
            ObtenerUsuarioIdActual(),
            pacienteId,
            dietaId
        );

        return ConvertirResultado(resultado);
    }

    // ==========================================
    // DUPLICAR
    // ==========================================

    [HttpPost("{dietaId:int}/duplicar")]
    public async Task<IActionResult> Duplicar(int pacienteId, int dietaId)
    {
        var resultado = await _dietaService.DuplicarAsync(
            ObtenerUsuarioIdActual(),
            pacienteId,
            dietaId
        );

        return ConvertirResultado(resultado);
    }

    [HttpPost("{dietaId:int}/copiar-a/{pacienteDestinoId:int}")]
    public async Task<IActionResult> CopiarAOtroPaciente(
        int pacienteId,
        int dietaId,
        int pacienteDestinoId
    )
    {
        var resultado = await _dietaService.CopiarAOtroPacienteAsync(
            ObtenerUsuarioIdActual(),
            pacienteId,
            dietaId,
            pacienteDestinoId
        );

        return ConvertirResultado(resultado);
    }

    // ==========================================
    // ARCHIVAR
    // ==========================================

    [HttpPatch("{dietaId:int}/archivar")]
    public async Task<IActionResult> Archivar(int pacienteId, int dietaId)
    {
        var resultado = await _dietaService.ArchivarAsync(
            ObtenerUsuarioIdActual(),
            pacienteId,
            dietaId
        );

        return ConvertirResultado(resultado);
    }

    // ==========================================
    // ELIMINAR
    // ==========================================

    [HttpDelete("{dietaId:int}")]
    public async Task<IActionResult> Eliminar(int pacienteId, int dietaId)
    {
        var resultado = await _dietaService.EliminarAsync(
            ObtenerUsuarioIdActual(),
            pacienteId,
            dietaId
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
    // RESPUESTAS
    // ==========================================

    private IActionResult ConvertirResultado<T>(ResultadoDieta<T> resultado)
    {
        if (resultado.Exitoso)
        {
            return Ok(resultado.Datos);
        }

        return resultado.TipoError switch
        {
            TipoErrorDieta.Validacion => BadRequest(new { mensaje = resultado.Error }),

            TipoErrorDieta.NoEncontrado => NotFound(new { mensaje = resultado.Error }),

            _ => StatusCode(
                StatusCodes.Status500InternalServerError,
                new { mensaje = "Ocurrió un error interno." }
            ),
        };
    }
}
