

namespace NutriApi.Services.Auth;

public interface ITokenService
{
    Task<TokenResultado> GenerarTokenAsync(
        UsuarioAplicacion usuario
    );
}