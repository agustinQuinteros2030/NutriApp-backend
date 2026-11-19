using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NutriApi.Services.CatalogoInicial;

namespace NutriApi.Controllers;

[ApiController]
[Route("api/catalogo-inicial")]
[Authorize(Roles = Roles.Nutricionista)]
public sealed class CatalogoInicialController : ControllerBase
{
    private readonly ICatalogoInicialService _servicio;

    public CatalogoInicialController(ICatalogoInicialService servicio)
    {
        _servicio = servicio;
    }

    [HttpPost("inicializar")]
    public async Task<IActionResult> Inicializar(CancellationToken cancellationToken)
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(claim, out var nutricionistaId))
        {
            return Unauthorized(new { mensaje = "Usuario no válido." });
        }

        var resultado = await _servicio.InicializarAsync(nutricionistaId, cancellationToken);

        return resultado.Estado switch
        {
            EstadoCargaCatalogo.Importado => Ok(resultado),

            EstadoCargaCatalogo.YaInicializado => Ok(resultado),

            EstadoCargaCatalogo.CatalogoExistente => Conflict(
                new
                {
                    mensaje = "El nutricionista ya tiene un catálogo. "
                        + "La carga automática no mezclará "
                        + "ni sobrescribirá datos.",

                    resultado.Version,
                }
            ),

            _ => StatusCode(500, new { mensaje = "Error de inicialización." }),
        };
    }
}
