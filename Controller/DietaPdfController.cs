using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NutriApi.Services.Pdf;

namespace NutriApi.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public class DietaPdfController : ControllerBase
{
    private readonly IDietaPdfService _service;

    public DietaPdfController(IDietaPdfService service)
    {
        _service = service;
    }

    // ==========================================
    // NUTRICIONISTA
    // ==========================================

    [Authorize(Roles = Roles.Nutricionista)]
    [HttpGet("pacientes/{pacienteId:int}/dietas/{dietaId:int}/pdf")]
    public async Task<IActionResult> DescargarDieta(int pacienteId, int dietaId)
    {
        var nutricionistaId = ObtenerUsuarioId();

        if (!nutricionistaId.HasValue)
        {
            return Unauthorized();
        }

        var archivo = await _service.GenerarParaNutricionistaAsync(
            nutricionistaId.Value,
            pacienteId,
            dietaId
        );

        if (archivo is null)
        {
            return NotFound(new { error = "Dieta no encontrada." });
        }

        return File(archivo.Contenido, "application/pdf", archivo.NombreArchivo);
    }

    // ==========================================
    // PACIENTE
    // ==========================================

    [Authorize(Roles = Roles.Paciente)]
    [HttpGet("paciente/mi-plan/pdf")]
    public async Task<IActionResult> DescargarMiPlan()
    {
        var pacienteId = ObtenerUsuarioId();

        if (!pacienteId.HasValue)
        {
            return Unauthorized();
        }

        var archivo = await _service.GenerarParaPacienteAsync(pacienteId.Value);

        if (archivo is null)
        {
            return NotFound(new { error = "No tenés una dieta activa asignada." });
        }

        return File(archivo.Contenido, "application/pdf", archivo.NombreArchivo);
    }

    // ==========================================
    // JWT
    // ==========================================

    private int? ObtenerUsuarioId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(claim, out var id))
        {
            return null;
        }

        return id;
    }
}
