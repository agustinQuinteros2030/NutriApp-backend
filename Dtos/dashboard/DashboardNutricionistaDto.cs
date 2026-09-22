namespace NutriApi.DTOs.Dashboard;

public class DashboardNutricionistaDto
{
    // ======================================
    // PACIENTES
    // ======================================

    public int TotalPacientes { get; set; }

    public int PacientesActivos { get; set; }

    public int PacientesInactivos { get; set; }

    public int PacientesPendientesActivacion { get; set; }


    // ======================================
    // DIETAS
    // ======================================

    public int DietasBorrador { get; set; }

    public int DietasActivas { get; set; }

    public int DietasArchivadas { get; set; }


    // ======================================
    // SEGUIMIENTOS
    // ======================================

    public int SeguimientosPendientesRevision { get; set; }

    public List<SeguimientoPendienteDashboardDto>
        SeguimientosPendientes
    { get; set; } = [];


    // ======================================
    // COBROS
    // ======================================

    public int CobrosVencidos { get; set; }

    public int CobrosProximosAVencer { get; set; }

    public List<CobroVencidoDashboardDto>
        CobrosVencidosDetalle
    { get; set; } = [];

    public List<CobroProximoDashboardDto>
        CobrosProximosAVencerDetalle
    { get; set; } = [];


    // ======================================
    // ACTIVIDAD
    // ======================================

    public List<PacienteRecienteDashboardDto>
        PacientesRecientes
    { get; set; } = [];
}