namespace NutriApi.DTOs.ControlSeguimiento;

public class ConfigurarControlSeguimientoDto
{
    public int FrecuenciaDias { get; set; } = 7;

    /*
     * Opcional.
     *
     * Sirve por ejemplo si el nutricionista
     * ya venía siguiendo al paciente antes
     * de empezar a utilizar el sistema.
     */
    public DateOnly? UltimoSeguimiento { get; set; }

    /*
     * Si viene null, el backend calculará:
     *
     * hoy + FrecuenciaDias
     */
    public DateOnly? ProximoSeguimiento { get; set; }
}
