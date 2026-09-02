using Microsoft.AspNetCore.Identity;

namespace NutriApi.Inicializadores;

public static class InicializadorRoles
{
    public static async Task InicializarAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var roleManager = scope.ServiceProvider
            .GetRequiredService<RoleManager<IdentityRole<int>>>();

        string[] roles =
        [
            Roles.Nutricionista,
            Roles.Paciente
        ];

        foreach (var rol in roles)
        {
            if (!await roleManager.RoleExistsAsync(rol))
            {
                var resultado = await roleManager.CreateAsync(
                    new IdentityRole<int>(rol)
                );

                if (!resultado.Succeeded)
                {
                    var errores = string.Join(
                        ", ",
                        resultado.Errors.Select(e => e.Description)
                    );

                    throw new Exception(
                        $"Error creando el rol {rol}: {errores}"
                    );
                }
            }
        }
    }
}