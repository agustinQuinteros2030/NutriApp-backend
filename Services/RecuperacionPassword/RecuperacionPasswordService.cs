using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using NutriApi.Configuracion;
using NutriApi.DTOs.RecuperacionPassword;

using NutriApp.Data;
using NutriApp.Models.Usuarios;

using System.Text;

namespace NutriApi.Services.RecuperacionPassword;

public class RecuperacionPasswordService
    : IRecuperacionPasswordService
{
    private readonly NutriAppDbContext
        _context;

    private readonly UserManager<UsuarioAplicacion>
        _userManager;

   
    private readonly AplicacionOpciones
    _aplicacionOpciones;

    public RecuperacionPasswordService(
        NutriAppDbContext context,
        UserManager<UsuarioAplicacion> userManager,
        IOptions<AplicacionOpciones> aplicacionOpciones)
    {
        _context =
            context;

        _userManager =
            userManager;

        _aplicacionOpciones =
            aplicacionOpciones.Value;
    }


    // ==========================================
    // SOLICITUD PÚBLICA
    // ==========================================

    public async Task<
        ResultadoRecuperacionPassword<
            SolicitudRecuperacionGenerada>>
        SolicitarAsync(
            string email)
    {
        if (string.IsNullOrWhiteSpace(
            email))
        {
            return Error<
                SolicitudRecuperacionGenerada>(
                "El email es obligatorio.",
                TipoErrorRecuperacionPassword
                    .Validacion
            );
        }


        var emailNormalizado =
            email
                .Trim()
                .ToLowerInvariant();


        var usuario =
            await _userManager
                .FindByEmailAsync(
                    emailNormalizado
                );


        // ======================================
        // ANTI-ENUMERACIÓN
        // ======================================

        /*
         * Seguimos sin revelar si el usuario
         * existe o no.
         *
         * La recuperación ahora es gestionada
         * manualmente por el nutricionista.
         */

        if (usuario is null)
        {
            return Exito(
                new SolicitudRecuperacionGenerada
                {
                    DebeEnviar =
                        false
                }
            );
        }


        // ======================================
        // CUENTA SIN CONTRASEÑA
        // ======================================

        /*
         * Una cuenta sin contraseña todavía
         * debe utilizar activación.
         */

        var tienePassword =
            await _userManager
                .HasPasswordAsync(
                    usuario
                );


        if (!tienePassword)
        {
            return Exito(
                new SolicitudRecuperacionGenerada
                {
                    DebeEnviar =
                        false
                }
            );
        }


        /*
         * Ya no enviamos ningún email.
         *
         * Tampoco devolvemos información que
         * permita inferir si la cuenta existe.
         */

        return Exito(
            new SolicitudRecuperacionGenerada
            {
                DebeEnviar =
                    false
            }
        );
    }


    // ==========================================
    // GENERAR RECUPERACIÓN PARA PACIENTE
    // NUTRICIONISTA
    // ==========================================

    public async Task<
      ResultadoRecuperacionPassword<
          RecuperacionPasswordPacienteGeneradaDto>>
      GenerarParaPacienteAsync(
          int nutricionistaId,
          int pacienteId)
    {
        // ======================================
        // PACIENTE + OWNERSHIP
        // ======================================

        var paciente =
            await ObtenerPacientePropioAsync(
                nutricionistaId,
                pacienteId
            );


        if (paciente is null)
        {
            return Error<
                RecuperacionPasswordPacienteGeneradaDto>(
                "Paciente no encontrado.",
                TipoErrorRecuperacionPassword
                    .NoEncontrado
            );
        }


        // ======================================
        // CUENTA ACTIVA
        // ======================================

        if (!paciente.Activo)
        {
            return Error<
                RecuperacionPasswordPacienteGeneradaDto>(
                "La cuenta del paciente se encuentra desactivada.",
                TipoErrorRecuperacionPassword
                    .Validacion
            );
        }


        // ======================================
        // CUENTA ACTIVADA
        // ======================================

        /*
         * Recuperación solamente corresponde
         * para una cuenta que ya tenga contraseña.
         *
         * Si todavía no tiene contraseña,
         * corresponde el flujo de activación.
         */

        var tienePassword =
            await _userManager
                .HasPasswordAsync(
                    paciente
                );


        if (!tienePassword)
        {
            return Error<
                RecuperacionPasswordPacienteGeneradaDto>(
                "La cuenta del paciente todavía no fue activada. Debés utilizar el flujo de activación.",
                TipoErrorRecuperacionPassword
                    .Validacion
            );
        }


        // ======================================
        // EMAIL
        // ======================================

        if (string.IsNullOrWhiteSpace(
            paciente.Email))
        {
            return Error<
                RecuperacionPasswordPacienteGeneradaDto>(
                "El paciente no tiene un email válido asociado.",
                TipoErrorRecuperacionPassword
                    .Validacion
            );
        }


        // ======================================
        // FRONTEND URL
        // ======================================

        if (string.IsNullOrWhiteSpace(
            _aplicacionOpciones.FrontendUrl))
        {
            return Error<
                RecuperacionPasswordPacienteGeneradaDto>(
                "No se encuentra configurada la URL del frontend.",
                TipoErrorRecuperacionPassword
                    .Interno
            );
        }


        // ======================================
        // TOKEN IDENTITY
        // ======================================

        var tokenIdentity =
            await _userManager
                .GeneratePasswordResetTokenAsync(
                    paciente
                );


        var tokenCodificado =
            WebEncoders
                .Base64UrlEncode(
                    Encoding.UTF8
                        .GetBytes(
                            tokenIdentity
                        )
                );


        // ======================================
        // URL FRONTEND
        // ======================================

        var frontendUrl =
            _aplicacionOpciones
                .FrontendUrl
                .Trim()
                .TrimEnd('/');


        var emailUrl =
            Uri.EscapeDataString(
                paciente.Email
            );


        var tokenUrl =
            Uri.EscapeDataString(
                tokenCodificado
            );


        var enlaceRecuperacion =
            $"{frontendUrl}/restablecer-password" +
            $"?email={emailUrl}" +
            $"&token={tokenUrl}";


        // ======================================
        // RESPUESTA
        // ======================================

        return Exito(
            new RecuperacionPasswordPacienteGeneradaDto
            {
                PacienteId =
                    paciente.Id,

                Email =
                    paciente.Email,

                Telefono =
                    string.IsNullOrWhiteSpace(
                        paciente.PhoneNumber)
                        ? null
                        : paciente.PhoneNumber.Trim(),

                EnlaceRecuperacion =
                    enlaceRecuperacion
            }
        );
    }

    // ==========================================
    // RESTABLECER CONTRASEÑA
    // ==========================================

    public async Task<
     ResultadoRecuperacionPassword<bool>>
     RestablecerAsync(
         RestablecerPasswordDto dto)
    {
        if (string.IsNullOrWhiteSpace(
            dto.Email))
        {
            return TokenInvalido();
        }


        if (string.IsNullOrWhiteSpace(
            dto.Token))
        {
            return TokenInvalido();
        }


        if (string.IsNullOrWhiteSpace(
            dto.Password))
        {
            return Error<bool>(
                "La contraseña es obligatoria.",
                TipoErrorRecuperacionPassword
                    .Validacion
            );
        }


        if (dto.Password !=
            dto.ConfirmarPassword)
        {
            return Error<bool>(
                "Las contraseñas no coinciden.",
                TipoErrorRecuperacionPassword
                    .Validacion
            );
        }


        var usuario =
            await _userManager
                .FindByEmailAsync(
                    dto.Email
                        .Trim()
                        .ToLowerInvariant()
                );


        // ======================================
        // ANTI-ENUMERACIÓN
        // ======================================

        if (usuario is null)
        {
            return TokenInvalido();
        }


        // ======================================
        // CUENTA ACTIVA
        // ======================================

        /*
         * No revelamos públicamente si la cuenta
         * está desactivada.
         */

        if (!usuario.Activo)
        {
            return TokenInvalido();
        }


        // ======================================
        // CUENTA YA ACTIVADA
        // ======================================

        /*
         * Una cuenta todavía no activada
         * debe usar activación de cuenta.
         */

        var tienePassword =
            await _userManager
                .HasPasswordAsync(
                    usuario
                );


        if (!tienePassword)
        {
            return TokenInvalido();
        }


        // ======================================
        // DECODIFICAR TOKEN
        // ======================================

        string tokenIdentity;


        try
        {
            var bytes =
                WebEncoders
                    .Base64UrlDecode(
                        dto.Token.Trim()
                    );


            tokenIdentity =
                Encoding.UTF8
                    .GetString(
                        bytes
                    );
        }
        catch
        {
            return TokenInvalido();
        }


        // ======================================
        // RESET PASSWORD
        // ======================================

        var resultado =
            await _userManager
                .ResetPasswordAsync(
                    usuario,
                    tokenIdentity,
                    dto.Password
                );


        if (!resultado.Succeeded)
        {
            /*
             * Token inválido, vencido,
             * modificado o reutilizado.
             */

            var tokenIncorrecto =
                resultado.Errors.Any(e =>
                    e.Code.Contains(
                        "Token",
                        StringComparison
                            .OrdinalIgnoreCase
                    )
                );


            if (tokenIncorrecto)
            {
                return TokenInvalido();
            }


            /*
             * El resto normalmente corresponde
             * a políticas de contraseña de Identity.
             */

            var errores =
                resultado.Errors
                    .Select(e =>
                        e.Description
                    )
                    .Distinct()
                    .ToList();


            var mensaje =
                errores.Count > 0
                    ? string.Join(
                        " ",
                        errores
                    )
                    : "No se pudo restablecer la contraseña.";


            return Error<bool>(
                mensaje,
                TipoErrorRecuperacionPassword
                    .Validacion
            );
        }


        return Exito(
            true
        );
    }


    // ==========================================
    // OWNERSHIP PACIENTE
    // ==========================================

    private async Task<Paciente?>
        ObtenerPacientePropioAsync(
            int nutricionistaId,
            int pacienteId)
    {
        return await _context.Pacientes
            .FirstOrDefaultAsync(p =>
                p.Id ==
                pacienteId
                &&
                p.NutricionistaId ==
                nutricionistaId
            );
    }


    // ==========================================
    // TOKEN INVÁLIDO
    // ==========================================

    private static
        ResultadoRecuperacionPassword<bool>
        TokenInvalido()
    {
        return Error<bool>(
            "El enlace de recuperación no es válido o ya venció.",
            TipoErrorRecuperacionPassword
                .TokenInvalido
        );
    }


    // ==========================================
    // HELPERS
    // ==========================================

    private static
        ResultadoRecuperacionPassword<T>
        Exito<T>(
            T datos)
    {
        return new ResultadoRecuperacionPassword<T>
        {
            Exitoso =
                true,

            Datos =
                datos,

            TipoError =
                TipoErrorRecuperacionPassword
                    .Ninguno
        };
    }


    private static
        ResultadoRecuperacionPassword<T>
        Error<T>(
            string mensaje,
            TipoErrorRecuperacionPassword tipo)
    {
        return new ResultadoRecuperacionPassword<T>
        {
            Exitoso =
                false,

            Error =
                mensaje,

            TipoError =
                tipo
        };
    }
}