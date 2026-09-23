

using NutriApp.Enums.Notificaciones;

namespace NutriApi.DTOs.Notificaciones;

public class NotificacionDto
{
    public int Id { get; set; }

    public TipoNotificacion Tipo { get; set; }

    public string Titulo { get; set; } =
        string.Empty;

    public string Mensaje { get; set; } =
        string.Empty;

    public bool Leida { get; set; }

    public DateTime FechaCreacion { get; set; }

    public DateTime? FechaLectura { get; set; }

    public string? RecursoTipo { get; set; }

    public int? RecursoId { get; set; }
}