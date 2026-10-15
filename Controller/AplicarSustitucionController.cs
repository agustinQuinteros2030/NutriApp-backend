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
        + "comidas/{comidaId:int}/items/{itemId:int}/aplicar-sustitucion"
)]
[Authorize(Roles = Roles.Nutricionista)]
public class AplicarSustitucionController : ControllerBase
{
    private readonly IAplicarSustitucionService _service;

    public AplicarSustitucionController(IAplicarSustitucionService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> Aplicar(
        int pacienteId,
        int dietaId,
        int comidaId,
        int itemId,
        [FromBody] AplicarSustitucionDto dto,
        CancellationToken cancellationToken
    )
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(claim, out var nutricionistaId))
        {
            return Unauthorized(new { mensaje = "Usuario no válido." });
        }

        var resultado = await _service.AplicarAsync(
            nutricionistaId,
            pacienteId,
            dietaId,
            comidaId,
            itemId,
            dto,
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
