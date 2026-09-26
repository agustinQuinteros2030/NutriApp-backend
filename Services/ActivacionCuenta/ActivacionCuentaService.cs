using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using NutriApi.Configuracion;
using NutriApi.DTOs.ActivacionCuenta;

using NutriApp.Data;
using NutriApp.Models.Usuarios;

using System.Text;

namespace NutriApi.Services.ActivacionCuenta;

public class ActivacionCuentaService
    : IActivacionCuentaService
{
    private readonly NutriAppDbContext
        _context;

    private readonly UserManager<UsuarioAplicacion>
        _userManager;

    private readonly AplicacionOpciones
     _aplicacionOpciones;


    public ActivacionCuentaService(
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
    // ESTADO DE ACTIVACIÓN
    // ==========================================

    public async Task<
        ResultadoActivacionCuenta<
            EstadoActivacionCuentaDto>>
        ObtenerEstadoAsync(
            int nutricionistaId,
            int pacienteId)
    {
        var paciente =
            await ObtenerPacientePropioAsync(
                nutricionistaId,
                pacienteId
            );


        if (paciente is null)
        {
            return Error<
                EstadoActivacionCuentaDto>(
                "Paciente no encontrado.",
                TipoErrorActivacionCuenta
                    .NoEncontrado
            );
        }


        var cuentaActivada =
            await _userManager
                .HasPasswordAsync(
                    paciente
                );


        return new ResultadoActivacionCuenta<
            EstadoActivacionCuentaDto>
        {
            Exitoso =
                true,

            Datos =
                new EstadoActivacionCuentaDto
                {
                    PacienteId =
                        paciente.Id,

                    Email =
                        paciente.Email
                        ?? string.Empty,

                    CuentaActivada =
                        cuentaActivada
                },

            TipoError =
                TipoErrorActivacionCuenta
                    .Ninguno
        };
    }


    // ==========================================
    // GENERAR ACTIVACIÓN
    // NUTRICIONISTA
    // ==========================================

    public async Task<
     ResultadoActivacionCuenta<
         ActivacionCuentaGeneradaDto>>
     GenerarActivacionAsync(
         int nutricionistaId,
         int pacienteId)
    {
        var paciente =
            await ObtenerPacientePropioAsync(
                nutricionistaId,
                pacienteId
            );


        // ======================================
        // PACIENTE
        // ======================================

        if (paciente is null)
        {
            return Error<
                ActivacionCuentaGeneradaDto>(
                "Paciente no encontrado.",
                TipoErrorActivacionCuenta
                    .NoEncontrado
            );
        }


        // ======================================
        // CUENTA ACTIVA
        // ======================================

        if (!paciente.Activo)
        {
            return Error<
                ActivacionCuentaGeneradaDto>(
                "La cuenta del paciente se encuentra desactivada.",
                TipoErrorActivacionCuenta
                    .Validacion
            );
        }


        // ======================================
        // CUENTA YA ACTIVADA
        // ======================================

        var cuentaActivada =
            await _userManager
                .HasPasswordAsync(
                    paciente
                );


        if (cuentaActivada)
        {
            return Error<
                ActivacionCuentaGeneradaDto>(
                "La cuenta del paciente ya se encuentra activada.",
                TipoErrorActivacionCuenta
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
                ActivacionCuentaGeneradaDto>(
                "El paciente no tiene un email válido asociado.",
                TipoErrorActivacionCuenta
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
                ActivacionCuentaGeneradaDto>(
                "No se encuentra configurada la URL del frontend.",
                TipoErrorActivacionCuenta
                    .ErrorInterno
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
        // ENLACE DE ACTIVACIÓN
        // ======================================

        var frontendUrl =
            _aplicacionOpciones
                .FrontendUrl
                .Trim()
                .TrimEnd('/');


        var emailCodificado =
            Uri.EscapeDataString(
                paciente.Email
            );


        var tokenUrl =
            Uri.EscapeDataString(
                tokenCodificado
            );


        var enlaceActivacion =
            $"{frontendUrl}/activar-cuenta" +
            $"?email={emailCodificado}" +
            $"&token={tokenUrl}";


        // ======================================
        // RESPUESTA
        // ======================================

        return new ResultadoActivacionCuenta<
            ActivacionCuentaGeneradaDto>
        {
            Exitoso =
                true,

            Datos =
                new ActivacionCuentaGeneradaDto
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

                    CuentaActivada =
                        false,

                    EnlaceActivacion =
                        enlaceActivacion
                },

            TipoError =
                TipoErrorActivacionCuenta
                    .Ninguno
        };
    }

    // ==========================================
    // ACTIVAR CUENTA
    // ==========================================

    public async Task<
     ResultadoActivacionCuenta<bool>>
     ActivarCuentaAsync(
         ActivarCuentaDto dto)
    {
        if (string.IsNullOrWhiteSpace(
            dto.Email))
        {
            return Error<bool>(
                "Los datos de activación no son válidos.",
                TipoErrorActivacionCuenta
                    .Validacion
            );
        }


        if (string.IsNullOrWhiteSpace(
            dto.Token))
        {
            return Error<bool>(
                "Los datos de activación no son válidos.",
                TipoErrorActivacionCuenta
                    .Validacion
            );
        }


        if (dto.Password !=
            dto.ConfirmarPassword)
        {
            return Error<bool>(
                "Las contraseñas no coinciden.",
                TipoErrorActivacionCuenta
                    .Validacion
            );
        }


        // ======================================
        // USUARIO
        // ======================================

        var usuario =
            await _userManager
                .FindByEmailAsync(
                    dto.Email.Trim()
                );


        /*
         * Además de existir, debe ser Paciente.
         *
         * Un nutricionista no puede usar este
         * flujo para modificar su contraseña.
         */

        if (usuario is not Paciente paciente)
        {
            return Error<bool>(
                "Los datos de activación no son válidos.",
                TipoErrorActivacionCuenta
                    .TokenInvalido
            );
        }


        // ======================================
        // CUENTA ACTIVA
        // ======================================

        /*
         * No revelamos mediante el endpoint público
         * que la cuenta está desactivada.
         */

        if (!paciente.Activo)
        {
            return Error<bool>(
                "Los datos de activación no son válidos.",
                TipoErrorActivacionCuenta
                    .TokenInvalido
            );
        }


        // ======================================
        // CUENTA YA ACTIVADA
        // ======================================

        var cuentaActivada =
            await _userManager
                .HasPasswordAsync(
                    paciente
                );


        if (cuentaActivada)
        {
            return Error<bool>(
                "La cuenta ya se encuentra activada.",
                TipoErrorActivacionCuenta
                    .Validacion
            );
        }


        // ======================================
        // TOKEN
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
            return Error<bool>(
                "El enlace de activación no es válido o ha expirado.",
                TipoErrorActivacionCuenta
                    .TokenInvalido
            );
        }


        // ======================================
        // ESTABLECER PRIMERA CONTRASEÑA
        // ======================================

        var resultado =
            await _userManager
                .ResetPasswordAsync(
                    paciente,
                    tokenIdentity,
                    dto.Password
                );


        if (!resultado.Succeeded)
        {
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
                return Error<bool>(
                    "El enlace de activación no es válido o ha expirado.",
                    TipoErrorActivacionCuenta
                        .TokenInvalido
                );
            }


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
                    : "No se pudo establecer la contraseña.";


            return Error<bool>(
                mensaje,
                TipoErrorActivacionCuenta
                    .Validacion
            );
        }


        return new ResultadoActivacionCuenta<bool>
        {
            Exitoso =
                true,

            Datos =
                true,

            TipoError =
                TipoErrorActivacionCuenta
                    .Ninguno
        };
    }

    // ==========================================
    // OWNERSHIP
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
    // ERROR
    // ==========================================

    private static ResultadoActivacionCuenta<T>
        Error<T>(
            string mensaje,
            TipoErrorActivacionCuenta tipo)
    {
        return new ResultadoActivacionCuenta<T>
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