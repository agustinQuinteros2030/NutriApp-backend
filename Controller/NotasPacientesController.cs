using System.Security.Claims;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using NutriApi.DTOs.Notas;
using NutriApi.Services.Notas;

namespace NutriApi.Controllers;

[ApiController]
[Route(
    "api/pacientes/{pacienteId:int}/notas"
)]
[Authorize(
    Roles = Roles.Nutricionista
)]
public class NotasPacientesController
    : ControllerBase
{
    private readonly INotaPacienteService
        _notaPacienteService;


    public NotasPacientesController(
        INotaPacienteService notaPacienteService)
    {
        _notaPacienteService =
            notaPacienteService;
    }


    // ==========================================
    // CREAR
    // ==========================================

    [HttpPost]
    public async Task<IActionResult>
        Crear(
            int pacienteId,
            CrearNotaPacienteDto dto)
    {
        var nutricionistaId =
            ObtenerUsuarioId();


        if (nutricionistaId is null)
        {
            return Unauthorized();
        }


        var resultado =
            await _notaPacienteService
                .CrearAsync(
                    nutricionistaId.Value,
                    pacienteId,
                    dto
                );


        if (!resultado.Exitoso)
        {
            return ProcesarError(
                resultado
            );
        }


        return CreatedAtAction(
            nameof(ObtenerPorId),
            new
            {
                pacienteId,
                notaId =
                    resultado.Datos!.Id
            },
            resultado.Datos
        );
    }


    // ==========================================
    // LISTAR
    // ==========================================

    [HttpGet]
    public async Task<IActionResult>
        ObtenerTodas(
            int pacienteId)
    {
        var nutricionistaId =
            ObtenerUsuarioId();


        if (nutricionistaId is null)
        {
            return Unauthorized();
        }


        var resultado =
            await _notaPacienteService
                .ObtenerTodasAsync(
                    nutricionistaId.Value,
                    pacienteId
                );


        if (!resultado.Exitoso)
        {
            return ProcesarError(
                resultado
            );
        }


        return Ok(
            resultado.Datos
        );
    }


    // ==========================================
    // DETALLE
    // ==========================================

    [HttpGet("{notaId:int}")]
    public async Task<IActionResult>
        ObtenerPorId(
            int pacienteId,
            int notaId)
    {
        var nutricionistaId =
            ObtenerUsuarioId();


        if (nutricionistaId is null)
        {
            return Unauthorized();
        }


        var resultado =
            await _notaPacienteService
                .ObtenerPorIdAsync(
                    nutricionistaId.Value,
                    pacienteId,
                    notaId
                );


        if (!resultado.Exitoso)
        {
            return ProcesarError(
                resultado
            );
        }


        return Ok(
            resultado.Datos
        );
    }


    // ==========================================
    // EDITAR
    // ==========================================

    [HttpPut("{notaId:int}")]
    public async Task<IActionResult>
        Editar(
            int pacienteId,
            int notaId,
            EditarNotaPacienteDto dto)
    {
        var nutricionistaId =
            ObtenerUsuarioId();


        if (nutricionistaId is null)
        {
            return Unauthorized();
        }


        var resultado =
            await _notaPacienteService
                .EditarAsync(
                    nutricionistaId.Value,
                    pacienteId,
                    notaId,
                    dto
                );


        if (!resultado.Exitoso)
        {
            return ProcesarError(
                resultado
            );
        }


        return Ok(
            resultado.Datos
        );
    }


    // ==========================================
    // USUARIO JWT
    // ==========================================

    private int? ObtenerUsuarioId()
    {
        var claim =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier
            );


        if (!int.TryParse(
                claim,
                out var usuarioId))
        {
            return null;
        }


        return usuarioId;
    }


    // ==========================================
    // ERROR
    // ==========================================

    private IActionResult ProcesarError<T>(
        ResultadoNotaPaciente<T> resultado)
    {
        return resultado.TipoError switch
        {
            TipoErrorNotaPaciente.Validacion =>
                BadRequest(
                    new
                    {
                        mensaje =
                            resultado.Error
                    }
                ),

            TipoErrorNotaPaciente.NoEncontrado =>
                NotFound(
                    new
                    {
                        mensaje =
                            resultado.Error
                    }
                ),

            _ =>
                StatusCode(
                    StatusCodes
                        .Status500InternalServerError,
                    new
                    {
                        mensaje =
                            resultado.Error
                            ??
                            "Ocurrió un error interno."
                    }
                )
        };
    }
}