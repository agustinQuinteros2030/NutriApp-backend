using Microsoft.EntityFrameworkCore;

using NutriApi.Calculos;
using NutriApi.DTOs.Dietas;
using NutriApi.Services.Dietas;

using NutriApp.Data;

namespace NutriApi.Services.SustitucionesNutricionales;

public class SustitucionNutricionalService : ISustitucionNutricionalService
{
    private readonly NutriAppDbContext _context;

    public SustitucionNutricionalService(
        NutriAppDbContext context)
    {
        _context = context;
    }

    // ==========================================
    // SIMULAR SUSTITUCIONES NUTRICIONALES
    // ==========================================

    public async Task<ResultadoDieta<SimulacionSustitucionesDto>>
        SimularAsync(
            int nutricionistaId,
            int pacienteId,
            int dietaId,
            int comidaId,
            CancellationToken cancellationToken = default)
    {
        // ==========================================
        // 1. OBTENER COMIDA Y VALIDAR PERTENENCIA
        // ==========================================

        var comida = await _context.Comidas
            .AsNoTracking()
            .Include(c => c.Secciones)
                .ThenInclude(s => s.Opciones)
                    .ThenInclude(o => o.Items)
                        .ThenInclude(i => i.Alimento)
            .AsSplitQuery()
            .FirstOrDefaultAsync(
                c =>
                    c.Id == comidaId
                    && c.DietaId == dietaId
                    && c.Dieta.PacienteId == pacienteId
                    && c.Dieta.Paciente.NutricionistaId == nutricionistaId,
                cancellationToken
            );

        if (comida is null)
        {
            return new ResultadoDieta<SimulacionSustitucionesDto>
            {
                Exitoso = false,
                Error = "Comida no encontrada.",
                TipoError = TipoErrorDieta.NoEncontrado
            };
        }

        // ==========================================
        // 2. OBTENER ITEMS PREDETERMINADOS
        // ==========================================

        var items = comida.Secciones
            .SelectMany(s => s.Opciones)
            .Where(o => o.EsPredeterminada)
            .SelectMany(o => o.Items)
            .ToDictionary(i => i.Id);

        var itemIds = items.Keys.ToList();

        // ==========================================
        // 3. CONSULTAR ALTERNATIVAS AUTORIZADAS
        // ==========================================

        var alternativas = await _context.AlternativasItemsComidas
            .AsNoTracking()
            .Include(a => a.Alimento)
            .Include(a => a.GrupoEquivalencia)
            .Where(a =>

                // Solo items de esta comida.
                itemIds.Contains(a.ItemOpcionComidaId)

                // Alternativa activa.
                && a.Activa

                // Grupo activo y del nutricionista.
                && a.GrupoEquivalencia.Activo
                && a.GrupoEquivalencia.NutricionistaId
                    == nutricionistaId

                // Alimento destino activo y autorizado.
                && a.Alimento.Activo
                && a.Alimento.NutricionistaId
                    == nutricionistaId

                // Alimento original activo y autorizado.
                && a.ItemOpcionComida.Alimento.Activo
                && a.ItemOpcionComida.Alimento.NutricionistaId
                    == nutricionistaId

                // No sustituir un alimento por sí mismo.
                && a.ItemOpcionComida.AlimentoId
                    != a.AlimentoId

                // El origen debe pertenecer activamente al grupo.
                && _context.EquivalenciasAlimentos.Any(e =>
                    e.GrupoEquivalenciaId == a.GrupoEquivalenciaId
                    && e.AlimentoId == a.ItemOpcionComida.AlimentoId
                    && e.Activa
                )

                // El destino debe pertenecer activamente al grupo.
                && _context.EquivalenciasAlimentos.Any(e =>
                    e.GrupoEquivalenciaId == a.GrupoEquivalenciaId
                    && e.AlimentoId == a.AlimentoId
                    && e.Activa
                )
            )
            .ToListAsync(cancellationToken);

        // ==========================================
        // 4. CONSTRUIR CANDIDATOS
        // ==========================================

        var candidatos = alternativas
            .Where(a => items.ContainsKey(a.ItemOpcionComidaId))
            .Select(a => new CandidatoSustitucion(
                items[a.ItemOpcionComidaId],
                a.Alimento,
                a.GrupoEquivalenciaId,
                a.GrupoEquivalencia.Criterio
            ))
            .ToList();

        // ==========================================
        // 5. SIMULAR SUSTITUCIONES
        // ==========================================

        var simulacion =
            SimuladorSustitucionesNutricionales.Calcular(
                comida,
                candidatos
            );

        // ==========================================
        // 6. DEVOLVER RESULTADO
        // ==========================================

        return new ResultadoDieta<SimulacionSustitucionesDto>
        {
            Exitoso = true,
            Datos = simulacion,
            TipoError = TipoErrorDieta.Ninguno
        };
    }
}