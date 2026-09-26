using Microsoft.EntityFrameworkCore;

using NutriApi.DTOs.Notificaciones;

using NutriApp.Data;
using NutriApp.Enums.Notificaciones;
using NutriApp.Models.Notificaciones;

namespace NutriApi.Services.Notificaciones;

public class NotificacionService
    : INotificacionService
{
    private readonly NutriAppDbContext
        _context;


    public NotificacionService(
        NutriAppDbContext context)
    {
        _context =
            context;
    }


    // ==========================================
    // LISTAR
    // ==========================================

    public async Task<List<NotificacionDto>>
        ObtenerAsync(
            int usuarioId)
    {
        return await _context.Notificaciones
            .AsNoTracking()
            .Where(n =>
                n.UsuarioId ==
                usuarioId
            )
            .OrderByDescending(n =>
                n.FechaCreacion
            )
            .Select(n =>
                new NotificacionDto
                {
                    Id =
                        n.Id,

                    Tipo =
                        n.Tipo,

                    Titulo =
                        n.Titulo,

                    Mensaje =
                        n.Mensaje,

                    Leida =
                        n.Leida,

                    FechaCreacion =
                        n.FechaCreacion,

                    FechaLectura =
                        n.FechaLectura,

                    RecursoTipo =
                        n.RecursoTipo,

                    RecursoId =
                        n.RecursoId
                }
            )
            .ToListAsync();
    }


    // ==========================================
    // RESUMEN / NO LEÍDAS
    // ==========================================

    public async Task<ResumenNotificacionesDto>
        ObtenerResumenAsync(
            int usuarioId)
    {
        var cantidad =
            await _context.Notificaciones
                .AsNoTracking()
                .CountAsync(n =>
                    n.UsuarioId ==
                    usuarioId
                    &&
                    !n.Leida
                );


        return new ResumenNotificacionesDto
        {
            CantidadNoLeidas =
                cantidad
        };
    }


    // ==========================================
    // MARCAR UNA COMO LEÍDA
    // ==========================================

    public async Task<bool>
        MarcarComoLeidaAsync(
            int usuarioId,
            int notificacionId)
    {
        var notificacion =
            await _context.Notificaciones
                .FirstOrDefaultAsync(n =>
                    n.Id ==
                    notificacionId
                    &&
                    n.UsuarioId ==
                    usuarioId
                );


        if (notificacion is null)
        {
            return false;
        }


        /*
         * Hacemos la operación idempotente.
         *
         * Si ya estaba leída no volvemos a
         * modificar FechaLectura.
         */

        if (notificacion.Leida)
        {
            return true;
        }


        notificacion.Leida =
            true;

        notificacion.FechaLectura =
            DateTime.UtcNow;


        await _context
            .SaveChangesAsync();


        return true;
    }


    // ==========================================
    // MARCAR TODAS COMO LEÍDAS
    // ==========================================

    public async Task<int>
        MarcarTodasComoLeidasAsync(
            int usuarioId)
    {
        var ahora =
            DateTime.UtcNow;


        /*
         * ExecuteUpdateAsync evita traer todas
         * las notificaciones a memoria.
         */

        var cantidadActualizada =
            await _context.Notificaciones
                .Where(n =>
                    n.UsuarioId ==
                    usuarioId
                    &&
                    !n.Leida
                )
                .ExecuteUpdateAsync(
                    setters =>
                        setters
                            .SetProperty(
                                n => n.Leida,
                                true
                            )
                            .SetProperty(
                                n => n.FechaLectura,
                                ahora
                            )
                );


        return cantidadActualizada;
    }


    // ==========================================
    // CREAR
    // ==========================================

    public async Task
      CrearAsync(
          int usuarioId,
          TipoNotificacion tipo,
          string titulo,
          string mensaje,
          string? recursoTipo = null,
          int? recursoId = null)
    {
        // ==========================================
        // USUARIO
        // ==========================================

        if (usuarioId <= 0)
        {
            throw new ArgumentException(
                "El usuario destinatario no es válido.",
                nameof(usuarioId)
            );
        }


        // ==========================================
        // TIPO
        // ==========================================

        if (!Enum.IsDefined(tipo))
        {
            throw new ArgumentException(
                "El tipo de notificación no es válido.",
                nameof(tipo)
            );
        }


        // ==========================================
        // TÍTULO
        // ==========================================

        if (string.IsNullOrWhiteSpace(
            titulo))
        {
            throw new ArgumentException(
                "El título de la notificación es obligatorio.",
                nameof(titulo)
            );
        }


        var tituloLimpio =
            titulo.Trim();


        if (tituloLimpio.Length > 150)
        {
            throw new ArgumentException(
                "El título de la notificación no puede superar los 150 caracteres.",
                nameof(titulo)
            );
        }


        // ==========================================
        // MENSAJE
        // ==========================================

        if (string.IsNullOrWhiteSpace(
            mensaje))
        {
            throw new ArgumentException(
                "El mensaje de la notificación es obligatorio.",
                nameof(mensaje)
            );
        }


        var mensajeLimpio =
            mensaje.Trim();


        if (mensajeLimpio.Length > 500)
        {
            throw new ArgumentException(
                "El mensaje de la notificación no puede superar los 500 caracteres.",
                nameof(mensaje)
            );
        }


        // ==========================================
        // RECURSO
        // ==========================================

        string? recursoTipoLimpio =
            string.IsNullOrWhiteSpace(
                recursoTipo)
                ? null
                : recursoTipo.Trim();


        if (recursoTipoLimpio is not null
            &&
            recursoTipoLimpio.Length > 100)
        {
            throw new ArgumentException(
                "El tipo de recurso no puede superar los 100 caracteres.",
                nameof(recursoTipo)
            );
        }


        if (recursoId.HasValue
            &&
            recursoId.Value <= 0)
        {
            throw new ArgumentException(
                "El identificador del recurso no es válido.",
                nameof(recursoId)
            );
        }


        /*
         * Evitamos estados inconsistentes:
         *
         * RecursoTipo = null
         * RecursoId = 25
         *
         * o:
         *
         * RecursoTipo = "Turno"
         * RecursoId = null
         */

        if (
            (recursoTipoLimpio is null
                &&
             recursoId.HasValue)
            ||
            (recursoTipoLimpio is not null
                &&
             !recursoId.HasValue)
        )
        {
            throw new ArgumentException(
                "El tipo de recurso y su identificador deben informarse juntos."
            );
        }


        // ==========================================
        // NOTIFICACIÓN
        // ==========================================

        var notificacion =
            new Notificacion
            {
                UsuarioId =
                    usuarioId,

                Tipo =
                    tipo,

                Titulo =
                    tituloLimpio,

                Mensaje =
                    mensajeLimpio,

                Leida =
                    false,

                FechaCreacion =
                    DateTime.UtcNow,

                RecursoTipo =
                    recursoTipoLimpio,

                RecursoId =
                    recursoId
            };


        _context.Notificaciones
            .Add(
                notificacion
            );


        await _context
            .SaveChangesAsync();
    }
}