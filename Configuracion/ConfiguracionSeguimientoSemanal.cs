namespace NutriApi.Configuracion;

public static class ConfiguracionSeguimientoSemanal
{
    // El formulario semanal comienza a estar
    // disponible los viernes.
    public const DayOfWeek DiaSeguimiento =
        DayOfWeek.Friday;


    // El paciente puede completarlo hasta
    // el domingo inclusive.
    public const DayOfWeek UltimoDiaDisponible =
        DayOfWeek.Sunday;


    // Zona horaria utilizada para determinar
    // el día actual del paciente.
    public const string ZonaHorariaId =
        "America/Argentina/Buenos_Aires";
}