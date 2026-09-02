using NutriApi.DTOs.Auth;

namespace NutriApi.Services.Auth;

public interface IAuthService
{
    Task<ResultadoAuth> RegistrarNutricionistaAsync(
        RegistroNutricionistaDto dto
    );

    Task<ResultadoAuth> LoginAsync(
        LoginDto dto
    );
}