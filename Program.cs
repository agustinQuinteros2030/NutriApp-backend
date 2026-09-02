using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

using NutriApi.Inicializadores;
using NutriApi.Services.Auth;
using NutriApi.Services.Pacientes;

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
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "No se encontró DefaultConnection."
    );

builder.Services.AddDbContext<NutriAppDbContext>(options =>
{
    options.UseNpgsql(
        connectionString,
        npgsqlOptions =>
        {
            npgsqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(5),
                errorCodesToAdd: null
            );
        }
    );
});


// =====================================
// IDENTITY
// =====================================

builder.Services
    .AddIdentity<UsuarioAplicacion, IdentityRole<int>>(options =>
    {
        // -----------------------------
        // USUARIO
        // -----------------------------

        options.User.RequireUniqueEmail = true;


        // -----------------------------
        // PASSWORD
        // -----------------------------

        options.Password.RequiredLength = 8;

        options.Password.RequireDigit = true;

        options.Password.RequireLowercase = true;

        options.Password.RequireUppercase = true;

        options.Password.RequireNonAlphanumeric = false;


        // -----------------------------
        // BLOQUEO DE CUENTA
        // -----------------------------

        options.Lockout.AllowedForNewUsers = true;

        options.Lockout.MaxFailedAccessAttempts = 5;

        options.Lockout.DefaultLockoutTimeSpan =
            TimeSpan.FromMinutes(15);
    })
    .AddEntityFrameworkStores<NutriAppDbContext>()
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
            JwtBearerDefaults.AuthenticationScheme;

        options.DefaultChallengeScheme =
            JwtBearerDefaults.AuthenticationScheme;

        options.DefaultScheme =
            JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,

                ValidateAudience = true,

                ValidateLifetime = true,

                ValidateIssuerSigningKey = true,

                ValidIssuer = jwtIssuer,

                ValidAudience = jwtAudience,

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Convert.FromBase64String(jwtKey)
                    ),

                ClockSkew = TimeSpan.Zero
            };
    });


// =====================================
// AUTORIZACIÓN
// =====================================

builder.Services.AddAuthorization();


// =====================================
// SERVICIOS
// =====================================

// Autenticación
builder.Services.AddScoped<ITokenService, TokenService>();

builder.Services.AddScoped<IAuthService, AuthService>();


// Pacientes
builder.Services.AddScoped<IPacienteService, PacienteService>();


// =====================================
// BUILD
// =====================================

var app = builder.Build();


// =====================================
// ROLES INICIALES
// =====================================

await InicializadorRoles.InicializarAsync(app.Services);


// =====================================
// PIPELINE HTTP
// =====================================

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();



app.UseAuthentication();



app.UseAuthorization();


app.MapControllers();

app.Run();