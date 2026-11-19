using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using NutriApi.DTOs.Auth;
using NutriApi.Services.Auth;

namespace NutriApi.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly UserManager<UsuarioAplicacion> _userManager;

    public AuthController(IAuthService authService, UserManager<UsuarioAplicacion> userManager)
    {
        _authService = authService;
        _userManager = userManager;
    }

    // =====================================
    // REGISTRO NUTRICIONISTA
    // =====================================

    [AllowAnonymous]
    [HttpPost("registro-nutricionista")]
    [EnableRateLimiting("AuthSensitive")]
    public async Task<IActionResult> RegistrarNutricionista(RegistroNutricionistaDto dto)
    {
        var resultado = await _authService.RegistrarNutricionistaAsync(dto);

        return ConvertirResultado(resultado);
    }

    // =====================================
    // LOGIN
    // =====================================

    [AllowAnonymous]
    [HttpPost("login")]
    [EnableRateLimiting("AuthSensitive")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var resultado = await _authService.LoginAsync(dto);

        return ConvertirResultado(resultado);
    }

    // =====================================
    // USUARIO ACTUAL
    // =====================================

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var usuario = await _userManager.GetUserAsync(User);

        if (usuario is null)
        {
            return Unauthorized(new { mensaje = "Usuario no encontrado." });
        }

        return Ok(
            new
            {
                UsuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier),

                Nombre = User.FindFirstValue(ClaimTypes.Name),

                Email = User.FindFirstValue(ClaimTypes.Email),

                Rol = User.FindFirstValue(ClaimTypes.Role),

                AvatarId = usuario.AvatarId,
            }
        );
    }

    private IActionResult ConvertirResultado(ResultadoAuth resultado)
    {
        if (resultado.Exitoso)
        {
            return Ok(resultado.Datos);
        }

        return resultado.TipoError switch
        {
            TipoErrorAuth.Validacion => BadRequest(new { mensaje = resultado.Error }),

            TipoErrorAuth.CredencialesInvalidas => Unauthorized(new { mensaje = resultado.Error }),

            TipoErrorAuth.UsuarioBloqueado => Unauthorized(new { mensaje = resultado.Error }),

            TipoErrorAuth.UsuarioDesactivado => Unauthorized(new { mensaje = resultado.Error }),

            _ => StatusCode(
                StatusCodes.Status500InternalServerError,
                new { mensaje = "Ocurrió un error interno." }
            ),
        };
    }
}
