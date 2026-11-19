using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NutriApi.Services.Calendario;

namespace NutriApi.Controllers;

[ApiController]
[Route("api/nutricionista/calendario")]
[Authorize(Roles = "Nutricionista")]
public class CalendarioController : ControllerBase
{
    private readonly ICalendarioService _calendarioService;

    public CalendarioController(ICalendarioService calendarioService)
    {
        _calendarioService = calendarioService;
    }

    // ==========================================
    // CALENDARIO UNIFICADO
    // ==========================================

    [HttpGet]
    public async Task<IActionResult> Obtener([FromQuery] DateOnly desde, [FromQuery] DateOnly hasta)
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(claim, out var nutricionistaId) || nutricionistaId <= 0)
        {
            return Unauthorized();
        }

        if (desde == default || hasta == default || desde > hasta || hasta == DateOnly.MaxValue)
        {
            return BadRequest(new { error = "El rango de fechas no es válido." });
        }

        if (hasta.DayNumber - desde.DayNumber > 366)
        {
            return BadRequest(new { error = "El rango no puede superar 366 días." });
        }

        var eventos = await _calendarioService.ObtenerEventosAsync(nutricionistaId, desde, hasta);

        return Ok(eventos);
    }
}
