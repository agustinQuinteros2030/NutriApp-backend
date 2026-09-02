using Microsoft.AspNetCore.Identity;
using NutriApi.DTOs.Auth;

namespace NutriApi.Services.Auth;

public class AuthService : IAuthService
{
    private readonly UserManager<UsuarioAplicacion> _userManager;

    private readonly SignInManager<UsuarioAplicacion> _signInManager;

    private readonly ITokenService _tokenService;


    public AuthService(
        UserManager<UsuarioAplicacion> userManager,
        SignInManager<UsuarioAplicacion> signInManager,
        ITokenService tokenService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
    }


    // =========================================
    // REGISTRO NUTRICIONISTA
    // =========================================

    public async Task<ResultadoAuth>
        RegistrarNutricionistaAsync(
            RegistroNutricionistaDto dto)
    {
        var email =
            dto.Email.Trim().ToLowerInvariant();


        var usuarioExistente =
            await _userManager.FindByEmailAsync(email);


        if (usuarioExistente is not null)
        {
            return new ResultadoAuth
            {
                Exitoso = false,

                Error =
                    "Ya existe un usuario con ese email.",

                TipoError =
                    TipoErrorAuth.Validacion
            };
        }


        var nutricionista =
            new Nutricionista
            {
                Nombre = dto.Nombre.Trim(),

                Apellido = dto.Apellido.Trim(),

                Email = email,

                UserName = email,

                PhoneNumber =
                    dto.Telefono?.Trim(),

                Matricula =
                    dto.Matricula?.Trim(),

                Activo = true,

                FechaCreacion =
                    DateTime.UtcNow
            };


        var resultadoCreacion =
            await _userManager.CreateAsync(
                nutricionista,
                dto.Password
            );


        if (!resultadoCreacion.Succeeded)
        {
            var errores =
                string.Join(
                    " | ",
                    resultadoCreacion.Errors
                        .Select(e => e.Description)
                );


            return new ResultadoAuth
            {
                Exitoso = false,

                Error = errores,

                TipoError =
                    TipoErrorAuth.Validacion
            };
        }


        var resultadoRol =
            await _userManager.AddToRoleAsync(
                nutricionista,
                Roles.Nutricionista
            );


        if (!resultadoRol.Succeeded)
        {
            // Evitamos dejar un usuario
            // creado sin su rol correspondiente.

            await _userManager
                .DeleteAsync(nutricionista);


            return new ResultadoAuth
            {
                Exitoso = false,

                Error =
                    "No se pudo asignar el rol al usuario.",

                TipoError =
                    TipoErrorAuth.ErrorInterno
            };
        }


        var token =
            await _tokenService
                .GenerarTokenAsync(nutricionista);


        return new ResultadoAuth
        {
            Exitoso = true,

            TipoError =
                TipoErrorAuth.Ninguno,

            Datos =
                new AuthResponseDto
                {
                    UsuarioId =
                        nutricionista.Id,

                    Nombre =
                        nutricionista.Nombre,

                    Apellido =
                        nutricionista.Apellido,

                    Email =
                        nutricionista.Email!,

                    Rol =
                        Roles.Nutricionista,

                    Token =
                        token.Token,

                    ExpiraEn =
                        token.ExpiraEn
                }
        };
    }


    // =========================================
    // LOGIN
    // =========================================

    public async Task<ResultadoAuth>
        LoginAsync(LoginDto dto)
    {
        var email =
            dto.Email.Trim().ToLowerInvariant();


        var usuario =
            await _userManager
                .FindByEmailAsync(email);


        // No decimos si el email existe o no.
        if (usuario is null)
        {
            return CredencialesInvalidas();
        }


        if (!usuario.Activo)
        {
            return new ResultadoAuth
            {
                Exitoso = false,

                Error =
                    "La cuenta se encuentra desactivada.",

                TipoError =
                    TipoErrorAuth.UsuarioDesactivado
            };
        }


        var resultadoPassword =
            await _signInManager
                .CheckPasswordSignInAsync(
                    usuario,
                    dto.Password,
                    lockoutOnFailure: true
                );


        if (resultadoPassword.IsLockedOut)
        {
            return new ResultadoAuth
            {
                Exitoso = false,

                Error =
                    "La cuenta fue bloqueada temporalmente por demasiados intentos fallidos.",

                TipoError =
                    TipoErrorAuth.UsuarioBloqueado
            };
        }


        if (!resultadoPassword.Succeeded)
        {
            return CredencialesInvalidas();
        }


        var roles =
            await _userManager
                .GetRolesAsync(usuario);


        var rol =
            roles.FirstOrDefault();


        if (rol is null)
        {
            return new ResultadoAuth
            {
                Exitoso = false,

                Error =
                    "El usuario no posee un rol asignado.",

                TipoError =
                    TipoErrorAuth.ErrorInterno
            };
        }


        var token =
            await _tokenService
                .GenerarTokenAsync(usuario);


        return new ResultadoAuth
        {
            Exitoso = true,

            TipoError =
                TipoErrorAuth.Ninguno,

            Datos =
                new AuthResponseDto
                {
                    UsuarioId =
                        usuario.Id,

                    Nombre =
                        usuario.Nombre,

                    Apellido =
                        usuario.Apellido,

                    Email =
                        usuario.Email!,

                    Rol =
                        rol,

                    Token =
                        token.Token,

                    ExpiraEn =
                        token.ExpiraEn
                }
        };
    }


    private static ResultadoAuth
        CredencialesInvalidas()
    {
        return new ResultadoAuth
        {
            Exitoso = false,

            Error =
                "Email o contraseña incorrectos.",

            TipoError =
                TipoErrorAuth.CredencialesInvalidas
        };
    }
}