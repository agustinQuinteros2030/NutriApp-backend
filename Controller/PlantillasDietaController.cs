using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NutriApi.DTOs.PlantillasDietas;
using NutriApi.Services.Dietas;
using NutriApi.Services.PlantillasDietas;

namespace NutriApi.Controllers;

[ApiController]
[Route("api/plantillas-dietas")]
[Authorize(Roles = Roles.Nutricionista)]
public class PlantillasDietaController : ControllerBase
{
    private readonly IPlantillaDietaService _plantillaService;

    public PlantillasDietaController(IPlantillaDietaService plantillaService)
    {
        _plantillaService = plantillaService;
    }

    // ==========================================
    // CREAR DESDE DIETA
    // ==========================================

    [HttpPost("desde-dieta/{pacienteId:int}/{dietaId:int}")]
    public async Task<IActionResult> CrearDesdeDieta(
        int pacienteId,
        int dietaId,
        CrearPlantillaDietaDto dto
    )
    {
        var resultado = await _plantillaService.CrearDesdeDietaAsync(
            ObtenerUsuarioIdActual(),
            pacienteId,
            dietaId,
            dto
        );

        return ConvertirResultado(resultado);
    }

    // ==========================================
    // LISTAR
    // ==========================================

    [HttpGet]
    public async Task<IActionResult> ObtenerTodas()
    {
        var resultado = await _plantillaService.ObtenerTodasAsync(ObtenerUsuarioIdActual());

        return ConvertirResultado(resultado);
    }

    // ==========================================
    // DETALLE
    // ==========================================

    [HttpGet("{plantillaId:int}")]
    public async Task<IActionResult> ObtenerPorId(int plantillaId)
    {
        var resultado = await _plantillaService.ObtenerPorIdAsync(
            ObtenerUsuarioIdActual(),
            plantillaId
        );

        return ConvertirResultado(resultado);
    }

    // ==========================================
    // APLICAR A PACIENTE
    // ==========================================

    [HttpPost("{plantillaId:int}/aplicar-a/{pacienteId:int}")]
    public async Task<IActionResult> AplicarAPaciente(int plantillaId, int pacienteId)
    {
        var resultado = await _plantillaService.AplicarAPacienteAsync(
            ObtenerUsuarioIdActual(),
            plantillaId,
            pacienteId
        );

        return ConvertirResultado(resultado);
    }

    // ==========================================
    // ELIMINAR
    // ==========================================

    [HttpDelete("{plantillaId:int}")]
    public async Task<IActionResult> Eliminar(int plantillaId)
    {
        var resultado = await _plantillaService.EliminarAsync(
            ObtenerUsuarioIdActual(),
            plantillaId
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
    // RESULTADO
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
