using NutriApi.DTOs.Notificaciones;
using NutriApp.Enums.Notificaciones;

namespace NutriApi.Services.Notificaciones;

public interface INotificacionService
{
    Task<List<NotificacionDto>> ObtenerAsync(int usuarioId);

    Task<ResumenNotificacionesDto> ObtenerResumenAsync(int usuarioId);

    Task<bool> MarcarComoLeidaAsync(int usuarioId, int notificacionId);

    Task<int> MarcarTodasComoLeidasAsync(int usuarioId);

    Task CrearAsync(
        int usuarioId,
        TipoNotificacion tipo,
        string titulo,
        string mensaje,
        string? recursoTipo = null,
        int? recursoId = null
    );

    Task<int> MarcarComoLeidaPorRecursoAsync(int usuarioId, string recursoTipo, int recursoId);

    Task<int> EliminarPorRecursoAsync(int usuarioId, string recursoTipo, int recursoId);

    Task<int> ActualizarContenidoPorRecursoAsync(
        int usuarioId,
        string recursoTipo,
        int recursoId,
        string titulo,
        string mensaje
    );
}
