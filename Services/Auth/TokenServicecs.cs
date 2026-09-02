using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;



using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace NutriApi.Services.Auth;

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;

    private readonly UserManager<UsuarioAplicacion> _userManager;

    public TokenService(
        IConfiguration configuration,
        UserManager<UsuarioAplicacion> userManager)
    {
        _configuration = configuration;
        _userManager = userManager;
    }


    public async Task<TokenResultado> GenerarTokenAsync(
        UsuarioAplicacion usuario)
    {
        var jwtKey =
            _configuration["Jwt:Key"]
            ?? throw new InvalidOperationException(
                "No se encontró Jwt:Key."
            );

        var issuer =
            _configuration["Jwt:Issuer"]
            ?? throw new InvalidOperationException(
                "No se encontró Jwt:Issuer."
            );

        var audience =
            _configuration["Jwt:Audience"]
            ?? throw new InvalidOperationException(
                "No se encontró Jwt:Audience."
            );

        var expirationMinutes =
            _configuration.GetValue<int?>(
                "Jwt:ExpirationMinutes"
            ) ?? 60;


        var roles =
            await _userManager.GetRolesAsync(usuario);


        var claims = new List<Claim>
        {
            new(
                ClaimTypes.NameIdentifier,
                usuario.Id.ToString()
            ),

            new(
                ClaimTypes.Email,
                usuario.Email ?? string.Empty
            ),

            new(
                ClaimTypes.Name,
                $"{usuario.Nombre} {usuario.Apellido}"
            ),

            new(
                JwtRegisteredClaimNames.Jti,
                Guid.NewGuid().ToString()
            )
        };


        foreach (var rol in roles)
        {
            claims.Add(
                new Claim(
                    ClaimTypes.Role,
                    rol
                )
            );
        }


        var key =
            new SymmetricSecurityKey(
                Convert.FromBase64String(jwtKey)
            );


        var credentials =
            new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256
            );


        var fechaExpiracion =
            DateTime.UtcNow
                .AddMinutes(expirationMinutes);


        var token =
            new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: fechaExpiracion,
                signingCredentials: credentials
            );


        return new TokenResultado
        {
            Token =
                new JwtSecurityTokenHandler()
                    .WriteToken(token),

            ExpiraEn = fechaExpiracion
        };
    }
}