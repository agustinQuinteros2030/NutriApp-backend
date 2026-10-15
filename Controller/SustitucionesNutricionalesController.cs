using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NutriApi.DTOs.Dietas;
using NutriApi.Services.Dietas;
using NutriApi.Services.SustitucionesNutricionales;

namespace NutriApi.Controllers;

[ApiController]
[Route(
    "api/pacientes/{pacienteId:int}/dietas/{dietaId:int}/"
        + "comidas/{comidaId:int}/sustituciones-nutricionales"
)]
[Authorize(Roles = Roles.Nutricionista)]
public class SustitucionesNutricionalesController : ControllerBase
{
    private readonly ISustitucionNutricionalService _service;

    public SustitucionesNutricionalesController(ISustitucionNutricionalService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Simular(
        int pacienteId,
        int dietaId,
        int comidaId,
        CancellationToken cancellationToken
    )
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(claim, out var nutricionistaId))
        {
            return Unauthorized(new { mensaje = "Usuario no válido." });
        }

        var resultado = await _service.SimularAsync(
            nutricionistaId,
            pacienteId,
            dietaId,
            comidaId,
            cancellationToken
        );

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
