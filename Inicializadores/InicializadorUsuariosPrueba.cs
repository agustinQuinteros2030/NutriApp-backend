using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using NutriApp.Data;
using NutriApp.Enums;
using NutriApp.Models.Pacientes;
using NutriApp.Models.Usuarios;

namespace NutriApi.Inicializadores;

public static class InicializadorUsuariosPrueba
{
    public static async Task InicializarAsync(
        IServiceProvider services,
        IConfiguration configuration)
    {
        using var scope = services.CreateScope();

        var userManager =
            scope.ServiceProvider
                .GetRequiredService<UserManager<UsuarioAplicacion>>();

        var context =
            scope.ServiceProvider
                .GetRequiredService<NutriAppDbContext>();


        // ==========================================
        // PASSWORDS DE DESARROLLO
        // ==========================================

        var passwordNutricionista =
            configuration["Seed:NutricionistaPassword"];

        var passwordPaciente =
            configuration["Seed:PacientePassword"];


        if (string.IsNullOrWhiteSpace(passwordNutricionista) ||
            string.IsNullOrWhiteSpace(passwordPaciente))
        {
            Console.WriteLine(
                "Seed de usuarios omitido: faltan passwords en User Secrets."
            );

            return;
        }


        // ==========================================
        // NUTRICIONISTA DE PRUEBA
        // ==========================================

        const string emailNutricionista =
            "nutri@test.com";


        var usuarioNutricionistaExistente =
            await userManager.FindByEmailAsync(
                emailNutricionista
            );


        Nutricionista nutricionista;


        if (usuarioNutricionistaExistente is null)
        {
            nutricionista =
                new Nutricionista
                {
                    Nombre =
                        "Nutricionista",

                    Apellido =
                        "Prueba",

                    UserName =
                        emailNutricionista,

                    Email =
                        emailNutricionista,

                    EmailConfirmed =
                        true,

                    PhoneNumber =
                        "1111111111",

                    Matricula =
                        "TEST-001",

                    Activo =
                        true,

                    FechaCreacion =
                        DateTime.UtcNow
                };


            var resultadoCreacion =
                await userManager.CreateAsync(
                    nutricionista,
                    passwordNutricionista
                );


            if (!resultadoCreacion.Succeeded)
            {
                var errores =
                    string.Join(
                        " | ",
                        resultadoCreacion.Errors
                            .Select(e =>
                                e.Description
                            )
                    );


                throw new Exception(
                    $"No se pudo crear el nutricionista de prueba: {errores}"
                );
            }


            var resultadoRol =
                await userManager.AddToRoleAsync(
                    nutricionista,
                    Roles.Nutricionista
                );


            if (!resultadoRol.Succeeded)
            {
                var errores =
                    string.Join(
                        " | ",
                        resultadoRol.Errors
                            .Select(e =>
                                e.Description
                            )
                    );


                throw new Exception(
                    $"No se pudo asignar el rol Nutricionista: {errores}"
                );
            }


            Console.WriteLine(
                $"Usuario de prueba creado: {emailNutricionista}"
            );
        }
        else
        {
            nutricionista =
                await context.Nutricionistas
                    .FirstOrDefaultAsync(n =>
                        n.Id ==
                        usuarioNutricionistaExistente.Id
                    )
                ??
                throw new Exception(
                    $"El usuario {emailNutricionista} existe, pero no es un Nutricionista."
                );


            // Por si existía antes pero no tenía rol.

            if (!await userManager.IsInRoleAsync(
                    nutricionista,
                    Roles.Nutricionista))
            {
                await userManager.AddToRoleAsync(
                    nutricionista,
                    Roles.Nutricionista
                );
            }
        }


        // ==========================================
        // PACIENTE DE PRUEBA
        // ==========================================

        const string emailPaciente =
            "paciente@test.com";


        var usuarioPacienteExistente =
            await userManager.FindByEmailAsync(
                emailPaciente
            );


        Paciente paciente;


        if (usuarioPacienteExistente is null)
        {
            paciente =
                new Paciente
                {
                    NutricionistaId =
                        nutricionista.Id,

                    Nombre =
                        "Paciente",

                    Apellido =
                        "Prueba",

                    UserName =
                        emailPaciente,

                    Email =
                        emailPaciente,

                    EmailConfirmed =
                        true,

                    PhoneNumber =
                        "2222222222",

                    FechaNacimiento =
                        new DateOnly(
                            2000,
                            1,
                            1
                        ),

                    Activo =
                        true,

                    FechaCreacion =
                        DateTime.UtcNow
                };


            var resultadoCreacion =
                await userManager.CreateAsync(
                    paciente,
                    passwordPaciente
                );


            if (!resultadoCreacion.Succeeded)
            {
                var errores =
                    string.Join(
                        " | ",
                        resultadoCreacion.Errors
                            .Select(e =>
                                e.Description
                            )
                    );


                throw new Exception(
                    $"No se pudo crear el paciente de prueba: {errores}"
                );
            }


            var resultadoRol =
                await userManager.AddToRoleAsync(
                    paciente,
                    Roles.Paciente
                );


            if (!resultadoRol.Succeeded)
            {
                var errores =
                    string.Join(
                        " | ",
                        resultadoRol.Errors
                            .Select(e =>
                                e.Description
                            )
                    );


                throw new Exception(
                    $"No se pudo asignar el rol Paciente: {errores}"
                );
            }


            Console.WriteLine(
                $"Usuario de prueba creado: {emailPaciente}"
            );
        }
        else
        {
            paciente =
                await context.Pacientes
                    .FirstOrDefaultAsync(p =>
                        p.Id ==
                        usuarioPacienteExistente.Id
                    )
                ??
                throw new Exception(
                    $"El usuario {emailPaciente} existe, pero no es un Paciente."
                );


            if (!await userManager.IsInRoleAsync(
                    paciente,
                    Roles.Paciente))
            {
                await userManager.AddToRoleAsync(
                    paciente,
                    Roles.Paciente
                );
            }
        }


        // ==========================================
        // PERFIL DEL PACIENTE
        // ==========================================

        var perfilExiste =
            await context.PerfilesPacientes
                .AnyAsync(p =>
                    p.PacienteId ==
                    paciente.Id
                );


        if (!perfilExiste)
        {
            var perfil =
                new PerfilPaciente
                {
                    PacienteId =
                        paciente.Id,

                    ObjetivoNutricional =
                        ObjetivoNutricional
                            .GanarMasaMuscular,

                    TipoActividad =
                        TipoActividad.Gimnasio,

                    PesoInicial =
                        80m,

                    Altura =
                        1.80m,

                    FechaInicio =
                        DateOnly.FromDateTime(
                            DateTime.UtcNow
                        ),

                    ActividadDescripcion =
                        "Entrenamiento de gimnasio 4 veces por semana.",

                    ObservacionesGenerales =
                        "Paciente generado automáticamente para pruebas de desarrollo.",

                    FechaCreacion =
                        DateTime.UtcNow
                };


            context.PerfilesPacientes.Add(
                perfil
            );


            await context.SaveChangesAsync();


            Console.WriteLine(
                "Perfil del paciente de prueba creado."
            );
        }


        Console.WriteLine(
            "Seed de usuarios de prueba finalizado."
        );
    }
}