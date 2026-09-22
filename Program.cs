using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

using NutriApi.Configuracion;

using NutriApi.Inicializadores;

using NutriApi.Services.ActivacionCuenta;
using NutriApi.Services.Alimentos;
using NutriApi.Services.Auth;
using NutriApi.Services.Dashboard;
using NutriApi.Services.Dietas;
using NutriApi.Services.Email;
using NutriApi.Services.Equivalencias;
using NutriApi.Services.MiPerfil;
using NutriApi.Services.Notas;
using NutriApi.Services.Pacientes;
using NutriApi.Services.Pagos;
using NutriApi.Services.PlanPaciente;
using NutriApi.Services.RecuperacionPassword;
using NutriApi.Services.RegistroDiario;
using NutriApi.Services.SeguimientoSemanal;

using NutriApp.Data;
using System.Security.Claims;
using System.Threading.RateLimiting;


var builder =
    WebApplication.CreateBuilder(args);


// =====================================
// CONFIGURACIÓN DE EMAIL
// =====================================

builder.Services.Configure<EmailOpciones>(
    builder.Configuration.GetSection(
        EmailOpciones.Seccion
    )
);


// =====================================
// CONTROLLERS
// =====================================

builder.Services.AddControllers();


// =====================================
// OPEN API
// =====================================

builder.Services.AddOpenApi();


// =====================================
// BASE DE DATOS - SUPABASE
// =====================================

var connectionString =
    builder.Configuration.GetConnectionString(
        "DefaultConnection"
    )
    ?? throw new InvalidOperationException(
        "No se encontró DefaultConnection."
    );


builder.Services.AddDbContext<NutriAppDbContext>(
    options =>
    {
        options.UseNpgsql(
            connectionString,
            npgsqlOptions =>
            {
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay:
                        TimeSpan.FromSeconds(5),
                    errorCodesToAdd: null
                );
            }
        );
    }
);


// =====================================
// IDENTITY
// =====================================

builder.Services
    .AddIdentity<
        UsuarioAplicacion,
        IdentityRole<int>
    >(options =>
    {
        // -----------------------------
        // USUARIO
        // -----------------------------

        options.User.RequireUniqueEmail =
            true;


        // -----------------------------
        // PASSWORD
        // -----------------------------

        options.Password.RequiredLength =
            8;

        options.Password.RequireDigit =
            true;

        options.Password.RequireLowercase =
            true;

        options.Password.RequireUppercase =
            true;

        options.Password.RequireNonAlphanumeric =
            false;


        // -----------------------------
        // BLOQUEO DE CUENTA
        // -----------------------------

        options.Lockout.AllowedForNewUsers =
            true;

        options.Lockout.MaxFailedAccessAttempts =
            5;

        options.Lockout.DefaultLockoutTimeSpan =
            TimeSpan.FromMinutes(15);
    })
    .AddEntityFrameworkStores<
        NutriAppDbContext
    >()
    .AddDefaultTokenProviders();


// =====================================
// TOKENS DE IDENTITY
// =====================================
//
// Aplica a los tokens generados mediante
// los providers por defecto de Identity.
//
// Actualmente:
// - activación de cuenta
// - recuperación de contraseña
//
// Ambos utilizan password reset tokens.
// =====================================

builder.Services.Configure<
    DataProtectionTokenProviderOptions>(
    options =>
    {
        options.TokenLifespan =
            TimeSpan.FromHours(24);
    }
);


// =====================================
// JWT
// =====================================

var jwtKey =
    builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException(
        "No se encontró Jwt:Key."
    );


var jwtIssuer =
    builder.Configuration["Jwt:Issuer"]
    ?? throw new InvalidOperationException(
        "No se encontró Jwt:Issuer."
    );


var jwtAudience =
    builder.Configuration["Jwt:Audience"]
    ?? throw new InvalidOperationException(
        "No se encontró Jwt:Audience."
    );


builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme =
            JwtBearerDefaults
                .AuthenticationScheme;

        options.DefaultChallengeScheme =
            JwtBearerDefaults
                .AuthenticationScheme;

        options.DefaultScheme =
            JwtBearerDefaults
                .AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        // =====================================
        // VALIDACIÓN NORMAL DEL JWT
        // =====================================

        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer =
                    true,

                ValidateAudience =
                    true,

                ValidateLifetime =
                    true,

                ValidateIssuerSigningKey =
                    true,

                ValidIssuer =
                    jwtIssuer,

                ValidAudience =
                    jwtAudience,

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Convert.FromBase64String(
                            jwtKey
                        )
                    ),

                ClockSkew =
                    TimeSpan.Zero
            };


        // =====================================
        // VALIDACIÓN DEL USUARIO
        // =====================================

        options.Events =
            new JwtBearerEvents
            {
                OnTokenValidated =
                    async context =>
                    {
                        // -------------------------
                        // USER ID DEL TOKEN
                        // -------------------------

                        var usuarioId =
                            context.Principal?
                                .FindFirst(
                                    ClaimTypes
                                        .NameIdentifier
                                )?
                                .Value;


                        // -------------------------
                        // SECURITY STAMP DEL TOKEN
                        // -------------------------

                        var stampToken =
                            context.Principal?
                                .FindFirst(
                                    "security_stamp"
                                )?
                                .Value;


                        if (string.IsNullOrWhiteSpace(
                                usuarioId)
                            ||
                            string.IsNullOrWhiteSpace(
                                stampToken))
                        {
                            context.Fail(
                                "Token inválido."
                            );

                            return;
                        }


                        // -------------------------
                        // USER MANAGER
                        // -------------------------

                        var userManager =
                            context
                                .HttpContext
                                .RequestServices
                                .GetRequiredService<
                                    UserManager<
                                        UsuarioAplicacion
                                    >
                                >();


                        // -------------------------
                        // USUARIO ACTUAL
                        // -------------------------

                        var usuario =
                            await userManager
                                .FindByIdAsync(
                                    usuarioId
                                );


                        if (usuario is null)
                        {
                            context.Fail(
                                "Usuario inválido."
                            );

                            return;
                        }


                        // -------------------------
                        // USUARIO DESACTIVADO
                        // -------------------------

                        /*
                         * Esto hace que desactivar
                         * una cuenta invalide también
                         * los JWT existentes.
                         */

                        if (!usuario.Activo)
                        {
                            context.Fail(
                                "Usuario desactivado."
                            );

                            return;
                        }


                        // -------------------------
                        // SECURITY STAMP ACTUAL
                        // -------------------------

                        var stampActual =
                            await userManager
                                .GetSecurityStampAsync(
                                    usuario
                                );


                        /*
                         * JWT:
                         * security_stamp = ABC
                         *
                         * Identity actual:
                         * security_stamp = XYZ
                         *
                         * Si no coinciden, hubo un
                         * cambio de seguridad.
                         *
                         * Ejemplo:
                         * cambio/reset de contraseña.
                         */

                        if (!string.Equals(
                                stampToken,
                                stampActual,
                                StringComparison
                                    .Ordinal))
                        {
                            context.Fail(
                                "Token revocado."
                            );

                            return;
                        }
                    }
            };
    });

// =====================================
// AUTORIZACIÓN
// =====================================

builder.Services.AddAuthorization();


// =====================================
// RATE LIMITING
// =====================================
//
// Esta política NO se aplica globalmente.
//
// Solamente se aplica a endpoints que tengan:
//
// [EnableRateLimiting("AuthSensitive")]
//
// Ejemplo:
// - login
// - solicitar recuperación
// - restablecer contraseña
// - activar cuenta
//
// Límite actual:
// 5 requests / minuto / IP
// =====================================

builder.Services.AddRateLimiter(
    options =>
    {
        options.RejectionStatusCode =
            StatusCodes
                .Status429TooManyRequests;


        options.AddPolicy(
            "AuthSensitive",
            httpContext =>
            {
                var ip =
                    httpContext
                        .Connection
                        .RemoteIpAddress?
                        .ToString()
                    ?? "unknown";


                return RateLimitPartition
                    .GetFixedWindowLimiter(
                        partitionKey:
                            ip,

                        factory:
                            _ =>
                                new FixedWindowRateLimiterOptions
                                {
                                    PermitLimit =
                                        5,

                                    Window =
                                        TimeSpan
                                            .FromMinutes(1),

                                    QueueLimit =
                                        0,

                                    QueueProcessingOrder =
                                        QueueProcessingOrder
                                            .OldestFirst,

                                    AutoReplenishment =
                                        true
                                }
                    );
            }
        );
    }
);


// =====================================
// SERVICIOS
// =====================================

// -----------------------------
// AUTENTICACIÓN
// -----------------------------

builder.Services.AddScoped<
    ITokenService,
    TokenService
>();

builder.Services.AddScoped<
    IAuthService,
    AuthService
>();


// -----------------------------
// EMAIL
// -----------------------------

/*builder.Services.AddHttpClient<
    IEmailService,
    EmailService>(
    client =>
    {
        client.BaseAddress =
            new Uri(
                "https://api.resend.com/"
            );

        client.Timeout =
            TimeSpan.FromSeconds(15);
    }
);*/


// -----------------------------
// PACIENTES
// -----------------------------

builder.Services.AddScoped<
    IPacienteService,
    PacienteService
>();


// -----------------------------
// CATEGORÍAS DE ALIMENTOS
// -----------------------------

builder.Services.AddScoped<
    ICategoriaAlimentoService,
    CategoriaAlimentoService
>();


// -----------------------------
// ALIMENTOS
// -----------------------------

builder.Services.AddScoped<
    IAlimentoService,
    AlimentoService
>();


// -----------------------------
// EQUIVALENCIAS
// -----------------------------

builder.Services.AddScoped<
    IEquivalenciaService,
    EquivalenciaService
>();


// -----------------------------
// DIETAS
// -----------------------------

builder.Services.AddScoped<
    IDietaService,
    DietaService
>();


// -----------------------------
// COMIDAS
// -----------------------------

builder.Services.AddScoped<
    IComidaService,
    ComidaService
>();


// -----------------------------
// OPCIONES / ITEMS
// -----------------------------

builder.Services.AddScoped<
    IOpcionComidaService,
    OpcionComidaService
>();


// -----------------------------
// ALTERNATIVAS
// -----------------------------

builder.Services.AddScoped<
    IAlternativaItemComidaService,
    AlternativaItemComidaService
>();


// -----------------------------
// NOTAS DE PACIENTES
// -----------------------------

builder.Services.AddScoped<
    INotaPacienteService,
    NotaPacienteService
>();


// -----------------------------
// COMPLEMENTOS DE DIETA
// -----------------------------

builder.Services.AddScoped<
    IComplementoDietaService,
    ComplementoDietaService
>();


// -----------------------------
// PAGOS
// -----------------------------

builder.Services.AddScoped<
    IPagoPacienteService,
    PagoPacienteService
>();


// -----------------------------
// PLAN DEL PACIENTE
// -----------------------------

builder.Services.AddScoped<
    IPlanPacienteService,
    PlanPacienteService
>();


// -----------------------------
// ACTIVACIÓN DE CUENTA
// -----------------------------

builder.Services.AddScoped<
    IActivacionCuentaService,
    ActivacionCuentaService
>();


// -----------------------------
// PERFIL DEL PACIENTE
// -----------------------------

builder.Services.AddScoped<
    IPerfilPacienteService,
    PerfilPacienteService
>();


// -----------------------------
// REGISTRO DIARIO DEL PACIENTE
// -----------------------------

builder.Services.AddScoped<
    IRegistroDiarioService,
    RegistroDiarioService
>();


// -----------------------------
// SEGUIMIENTO SEMANAL
// -----------------------------

builder.Services.AddScoped<
    ISeguimientoSemanalService,
    SeguimientoSemanalService
>();


// -----------------------------
// RECUPERACIÓN DE PASSWORD
// -----------------------------

builder.Services.AddScoped<
    IRecuperacionPasswordService,
    RecuperacionPasswordService
>();


// -----------------------------
// DASHBOARD NUTRICIONISTA
// -----------------------------

builder.Services.AddScoped<
    IDashboardNutricionistaService,
    DashboardNutricionistaService
>();


// =====================================
// CORS
// =====================================

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "FrontendLocal",
        policy =>
        {
            policy
                .WithOrigins(
                    "http://localhost:5173",
                    "http://127.0.0.1:5173"
                )
                .AllowAnyHeader()
                .AllowAnyMethod();
        }
    );
});


// =====================================
// BUILD
// =====================================

var app =
    builder.Build();


// =====================================
// ROLES INICIALES
// =====================================

try
{
    await InicializadorRoles
        .InicializarAsync(
            app.Services
        );
}
catch (Exception ex)
{
    Console.WriteLine(
        "No se pudieron inicializar los roles."
    );

    Console.WriteLine(
        ex.ToString()
    );
}


// =====================================
// USUARIOS DE PRUEBA
// SOLO DESARROLLO
// =====================================

if (app.Environment.IsDevelopment())
{
    try
    {
        await InicializadorUsuariosPrueba
            .InicializarAsync(
                app.Services,
                app.Configuration
            );
    }
    catch (Exception ex)
    {
        Console.WriteLine(
            "No se pudieron inicializar los usuarios de prueba."
        );

        Console.WriteLine(
            ex.ToString()
        );
    }
}


// =====================================
// OPEN API
// =====================================

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}


// =====================================
// PIPELINE HTTP
// =====================================

app.UseHttpsRedirection();


// -----------------------------
// ROUTING
// -----------------------------

app.UseRouting();


// -----------------------------
// CORS
// -----------------------------

app.UseCors(
    "FrontendLocal"
);


// -----------------------------
// RATE LIMITING
// -----------------------------

app.UseRateLimiter();


// -----------------------------
// AUTENTICACIÓN
// -----------------------------

app.UseAuthentication();


// -----------------------------
// AUTORIZACIÓN
// -----------------------------

app.UseAuthorization();


// -----------------------------
// CONTROLLERS
// -----------------------------

app.MapControllers();


app.Run();