using Microsoft.EntityFrameworkCore;
using NutriApi.DTOs.Pdf;
using NutriApi.DTOs.PlanPaciente;
using NutriApi.Services.PlanPaciente;
using NutriApp.Data;

namespace NutriApi.Services.Pdf;

public class DietaPdfService : IDietaPdfService
{
    private readonly NutriAppDbContext _context;

    private readonly IPlanPacienteService _planService;

    public DietaPdfService(NutriAppDbContext context, IPlanPacienteService planService)
    {
        _context = context;

        _planService = planService;
    }

    // ==========================================
    // NUTRICIONISTA
    // ==========================================

    public async Task<ArchivoPdfDieta?> GenerarParaNutricionistaAsync(
        int nutricionistaId,
        int pacienteId,
        int dietaId
    )
    {
        var plan = await _planService.ObtenerPlanPorDietaAsync(
            nutricionistaId,
            pacienteId,
            dietaId
        );

        if (plan is null)
        {
            return null;
        }

        var persona = await _context
            .Pacientes.AsNoTracking()
            .Where(p => p.Id == pacienteId && p.NutricionistaId == nutricionistaId)
            .Select(p => new
            {
                Paciente = p.Nombre + " " + p.Apellido,

                Nutricionista = p.Nutricionista.Nombre + " " + p.Nutricionista.Apellido,
            })
            .FirstOrDefaultAsync();

        if (persona is null)
        {
            return null;
        }

        return CrearArchivo(persona.Paciente, persona.Nutricionista, plan);
    }

    // ==========================================
    // PACIENTE
    // ==========================================

    public async Task<ArchivoPdfDieta?> GenerarParaPacienteAsync(int pacienteId)
    {
        var plan = await _planService.ObtenerMiPlanAsync(pacienteId);

        if (plan is null)
        {
            return null;
        }

        var persona = await _context
            .Pacientes.AsNoTracking()
            .Where(p => p.Id == pacienteId)
            .Select(p => new
            {
                Paciente = p.Nombre + " " + p.Apellido,

                Nutricionista = p.Nutricionista.Nombre + " " + p.Nutricionista.Apellido,
            })
            .FirstOrDefaultAsync();

        if (persona is null)
        {
            return null;
        }

        return CrearArchivo(persona.Paciente, persona.Nutricionista, plan);
    }

    // ==========================================
    // GENERAR ARCHIVO
    // ==========================================

    private static ArchivoPdfDieta CrearArchivo(
        string paciente,
        string nutricionista,
        MiPlanDto plan
    )
    {
        var datos = new DietaPdfDatos
        {
            NombrePaciente = paciente,

            NombreNutricionista = nutricionista,

            Plan = plan,
        };

        var documento = new DietaPdfDocument(datos);

        var bytes = documento.Generar();

        var nombreSeguro = paciente.Trim().Replace(" ", "-").ToLowerInvariant();

        return new ArchivoPdfDieta
        {
            Contenido = bytes,

            NombreArchivo = $"plan-nutricional-{nombreSeguro}-v{plan.Version}.pdf",
        };
    }
}
