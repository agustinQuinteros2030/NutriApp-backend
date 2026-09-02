using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using NutriApi.DTOs.Auth;
using NutriApi.Services.Auth;

using System.Security.Claims;

namespace NutriApi.Controllers;


[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;


    public AuthController(
        IAuthService authService)
    {
        _authService = authService;
    }


    // =====================================
    // REGISTRO NUTRICIONISTA
    // =====================================

    [AllowAnonymous]
    [HttpPost("registro-nutricionista")]
    public async Task<IActionResult>
        RegistrarNutricionista(
            RegistroNutricionistaDto dto)
    {
        var resultado =
            await _authService
                .RegistrarNutricionistaAsync(dto);


        return ConvertirResultado(resultado);
    }


    // =====================================
    // LOGIN
    // =====================================

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult>
        Login(LoginDto dto)
    {
        var resultado =
            await _authService
                .LoginAsync(dto);


        return ConvertirResultado(resultado);
    }


    // =====================================
    // USUARIO ACTUAL
    // =====================================

    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        return Ok(new
        {
            UsuarioId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier
                ),

            Nombre =
                User.FindFirstValue(
                    ClaimTypes.Name
                ),

            Email =
                User.FindFirstValue(
                    ClaimTypes.Email
                ),

            Rol =
                User.FindFirstValue(
                    ClaimTypes.Role
                )
        });
    }


    private IActionResult ConvertirResultado(
        ResultadoAuth resultado)
    {
        if (resultado.Exitoso)
        {
            return Ok(resultado.Datos);
        }


        return resultado.TipoError switch
        {
            TipoErrorAuth.Validacion =>
                BadRequest(new
                {
                    mensaje = resultado.Error
                }),


            TipoErrorAuth.CredencialesInvalidas =>
                Unauthorized(new
                {
                    mensaje = resultado.Error
                }),


            TipoErrorAuth.UsuarioBloqueado =>
                Unauthorized(new
                {
                    mensaje = resultado.Error
                }),


            TipoErrorAuth.UsuarioDesactivado =>
                Unauthorized(new
                {
                    mensaje = resultado.Error
                }),


            _ =>
                StatusCode(
                    StatusCodes
                        .Status500InternalServerError,

                    new
                    {
                        mensaje =
                            "Ocurrió un error interno."
                    }
                )
        };
    }
}