using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using NutriApi.Services.Notificaciones;

using System.Security.Claims;

namespace NutriApi.Controllers;

[ApiController]
[Route("api/notificaciones")]
[Authorize]
public class NotificacionesController
    : ControllerBase
{
    private readonly INotificacionService
        _service;


    public NotificacionesController(
        INotificacionService service)
    {
        _service =
            service;
    }


    // ==========================================
    // LISTAR NOTIFICACIONES
    // ==========================================

    [HttpGet]
    public async Task<IActionResult>
        Obtener()
    {
        var usuarioId =
            ObtenerUsuarioId();


        if (!usuarioId.HasValue)
        {
            return Unauthorized();
        }


        var notificaciones =
            await _service
                .ObtenerAsync(
                    usuarioId.Value
                );


        return Ok(
            notificaciones
        );
    }


    // ==========================================
    // RESUMEN / NO LEÍDAS
    // ==========================================

    [HttpGet("resumen")]
    public async Task<IActionResult>
        ObtenerResumen()
    {
        var usuarioId =
            ObtenerUsuarioId();


        if (!usuarioId.HasValue)
        {
            return Unauthorized();
        }


        var resumen =
            await _service
                .ObtenerResumenAsync(
                    usuarioId.Value
                );


        return Ok(
            resumen
        );
    }


    // ==========================================
    // MARCAR UNA COMO LEÍDA
    // ==========================================

    [HttpPut("{notificacionId:int}/leer")]
    public async Task<IActionResult>
        MarcarComoLeida(
            int notificacionId)
    {
        var usuarioId =
            ObtenerUsuarioId();


        if (!usuarioId.HasValue)
        {
            return Unauthorized();
        }


        var resultado =
            await _service
                .MarcarComoLeidaAsync(
                    usuarioId.Value,
                    notificacionId
                );


        if (!resultado)
        {
            return NotFound(
                new
                {
                    error =
                        "Notificación no encontrada."
                }
            );
        }


        return Ok(
            new
            {
                mensaje =
                    "Notificación marcada como leída."
            }
        );
    }


    // ==========================================
    // MARCAR TODAS COMO LEÍDAS
    // ==========================================

    [HttpPut("leer-todas")]
    public async Task<IActionResult>
        MarcarTodasComoLeidas()
    {
        var usuarioId =
            ObtenerUsuarioId();


        if (!usuarioId.HasValue)
        {
            return Unauthorized();
        }


        var cantidad =
            await _service
                .MarcarTodasComoLeidasAsync(
                    usuarioId.Value
                );


        return Ok(
            new
            {
                mensaje =
                    "Notificaciones actualizadas correctamente.",

                cantidadActualizada =
                    cantidad
            }
        );
    }


    // ==========================================
    // ID JWT
    // ==========================================

    private int?
        ObtenerUsuarioId()
    {
        var claim =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier
            );


        if (!int.TryParse(
            claim,
            out var id))
        {
            return null;
        }


        return id;
    }
}