using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

using NutriApi.Inicializadores;
using NutriApi.Services.ActivacionCuenta;
using NutriApi.Services.Alimentos;
using NutriApi.Services.Auth;
using NutriApi.Services.Dietas;
using NutriApi.Services.Equivalencias;
using NutriApi.Services.MiPerfil;
using NutriApi.Services.Notas;
using NutriApi.Services.Pacientes;
using NutriApi.Services.Pagos;

using NutriApi.Services.PlanPaciente;
using NutriApi.Services.RegistroDiario;
using NutriApp.Data;


var builder = WebApplication.CreateBuilder(args);


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
    });


// =====================================
// AUTORIZACIÓN
// =====================================

builder.Services.AddAuthorization();


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

builder.Services.AddScoped<
    IComplementoDietaService,
    ComplementoDietaService
>();


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
//------------------------------
// PERFIL DEL PACIENTE
//------------------------------
builder.Services.AddScoped<
    IPerfilPacienteService,
    PerfilPacienteService
>();
//------------------------------
// REGISTRO DIARIO DEL PACIENTE
//------------------------------

builder.Services.AddScoped<
    IRegistroDiarioService,
    RegistroDiarioService
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

var app = builder.Build();


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
// CORS
// -----------------------------

app.UseCors(
    "FrontendLocal"
);


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