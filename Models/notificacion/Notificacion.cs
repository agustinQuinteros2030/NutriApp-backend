
using NutriApp.Enums.Notificaciones;
using NutriApp.Models.Usuarios;

namespace NutriApp.Models.Notificaciones;

public class Notificacion
{
    public int Id { get; set; }


    // ==========================================
    // USUARIO DESTINATARIO
    // ==========================================

    public int UsuarioId { get; set; }

    public UsuarioAplicacion Usuario { get; set; } =
        null!;


    // ==========================================
    // CONTENIDO
    // ==========================================

    public TipoNotificacion Tipo { get; set; }

    public string Titulo { get; set; } =
        string.Empty;

    public string Mensaje { get; set; } =
        string.Empty;


    // ==========================================
    // ESTADO
    // ==========================================

    public bool Leida { get; set; } =
        false;

    public DateTime FechaCreacion { get; set; } =
        DateTime.UtcNow;

    public DateTime? FechaLectura { get; set; }


    // ==========================================
    // RECURSO RELACIONADO
    // ==========================================

    /*
     * Estos campos permiten que el frontend
     * pueda navegar al recurso relacionado.
     *
     * Ej:
     *
     * RecursoTipo = "SeguimientoSemanal"
     * RecursoId   = 15
     *
     * o:
     *
     * RecursoTipo = "Paciente"
     * RecursoId   = 8
     */

    public string? RecursoTipo { get; set; }

    public int? RecursoId { get; set; }
}