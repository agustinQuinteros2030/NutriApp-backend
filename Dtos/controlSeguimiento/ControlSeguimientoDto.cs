namespace NutriApi.DTOs.ControlSeguimiento;

public class ControlSeguimientoDto
{
    public int Id { get; set; }

    public int PacienteId { get; set; }

    public string NombrePaciente { get; set; } = string.Empty;

    public string ApellidoPaciente { get; set; } = string.Empty;

    public bool Activo { get; set; }

    public int FrecuenciaDias { get; set; }

    public DateOnly? UltimoSeguimiento { get; set; }

    public DateOnly ProximoSeguimiento { get; set; }

    /*
     * Calculado:
     *
     * Desactivado
     * Vencido
     * Hoy
     * Proximo
     * AlDia
     */
    public string Estado { get; set; } = string.Empty;

    /*
     * Ejemplos:
     *
     * -4 = vencido hace 4 días
     *  0 = hoy
     *  2 = faltan 2 días
     */
    public int DiasHastaSeguimiento { get; set; }
}
