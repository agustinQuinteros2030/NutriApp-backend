using System.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using NutriApi.Calculos;
using NutriApi.DTOs.Dietas;
using NutriApi.Services.Dietas;
using NutriApp.Data;
using NutriApp.Enums;
using NutriApp.Models.Alimentos;

namespace NutriApi.Services.SustitucionesNutricionales;

public class AplicarSustitucionService : IAplicarSustitucionService
{
    private readonly NutriAppDbContext _context;

    public AplicarSustitucionService(NutriAppDbContext context)
    {
        _context = context;
    }

    // ==========================================
    // APLICAR SUSTITUCIÓN
    // ==========================================

    public async Task<ResultadoDieta<SustitucionAplicadaDto>> AplicarAsync(
        int nutricionistaId,
        int pacienteId,
        int dietaId,
        int comidaId,
        int itemId,
        AplicarSustitucionDto dto,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(dto);

        // ==========================================
        // VALIDACIONES DEL REQUEST
        // ==========================================

        if (
            dto.AlimentoDestinoId <= 0
            || dto.GrupoEquivalenciaId <= 0
            || dto.AlimentoOrigenEsperadoId <= 0
            || dto.CantidadOrigenEsperada <= 0m
            || dto.CantidadDestinoEsperada <= 0m
            || string.IsNullOrWhiteSpace(dto.UnidadOrigenEsperada)
        )
        {
            return Error("Los datos de la sustitución son inválidos.", TipoErrorDieta.Validacion);
        }

        // ==========================================
        // ESTRATEGIA DE EJECUCIÓN
        // Compatible con EnableRetryOnFailure.
        // ==========================================

        var strategy = _context.Database.CreateExecutionStrategy();

        try
        {
            return await strategy.ExecuteAsync(async () =>
            {
                _context.ChangeTracker.Clear();

                // ==================================
                // TRANSACCIÓN
                // ==================================

                await using var transaction = await _context.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable,
                    cancellationToken
                );

                // ==================================
                // OBTENER ITEM CON OWNERSHIP
                // ==================================

                var item = await _context
                    .ItemsOpcionesComidas.Include(i => i.Alimento)
                    .Include(i => i.Alternativas)
                        .ThenInclude(a => a.Alimento)
                    .Include(i => i.Alternativas)
                        .ThenInclude(a => a.GrupoEquivalencia)
                    .Include(i => i.OpcionSeccionComida)
                        .ThenInclude(o => o.SeccionComida)
                            .ThenInclude(s => s.Comida)
                                .ThenInclude(c => c.Dieta)
                    .AsSplitQuery()
                    .FirstOrDefaultAsync(
                        i =>
                            i.Id == itemId
                            && i.OpcionSeccionComida.SeccionComida.ComidaId == comidaId
                            && i.OpcionSeccionComida.SeccionComida.Comida.DietaId == dietaId
                            && i.OpcionSeccionComida.SeccionComida.Comida.Dieta.PacienteId
                                == pacienteId
                            && i.OpcionSeccionComida
                                .SeccionComida
                                .Comida
                                .Dieta
                                .Paciente
                                .NutricionistaId == nutricionistaId,
                        cancellationToken
                    );

                if (item is null)
                {
                    return Error("Ítem no encontrado.", TipoErrorDieta.NoEncontrado);
                }

                // ==================================
                // DIETA ARCHIVADA
                // ==================================

                var dieta = item.OpcionSeccionComida.SeccionComida.Comida.Dieta;

                if (dieta.Estado == EstadoDieta.Archivada)
                {
                    return Error(
                        "No se puede modificar una dieta archivada.",
                        TipoErrorDieta.Validacion
                    );
                }

                // ==================================
                // OPCIÓN PREDETERMINADA
                // ==================================

                if (!item.OpcionSeccionComida.EsPredeterminada)
                {
                    return Error(
                        "Solo se pueden aplicar sustituciones "
                            + "simuladas sobre opciones predeterminadas.",
                        TipoErrorDieta.Validacion
                    );
                }

                // ==================================
                // CONTROL DE CAMBIOS CONCURRENTES
                // ==================================

                if (
                    item.AlimentoId != dto.AlimentoOrigenEsperadoId
                    || item.Cantidad != dto.CantidadOrigenEsperada
                    || item.UnidadMedida.ToString() != dto.UnidadOrigenEsperada
                )
                {
                    return Error(
                        "La comida cambió desde la simulación. "
                            + "Volvé a analizar antes de confirmar.",
                        TipoErrorDieta.Validacion
                    );
                }

                // ==================================
                // ALIMENTO ORIGINAL
                // ==================================

                var origen = item.Alimento;

                if (
                    !origen.Activo
                    || origen.NutricionistaId != nutricionistaId
                    || item.UnidadMedida != origen.UnidadBase
                    || !DatosValidos(origen)
                )
                {
                    return Error(
                        "El alimento original no posee " + "datos válidos para la sustitución.",
                        TipoErrorDieta.Validacion
                    );
                }

                // ==================================
                // ALTERNATIVA SELECCIONADA
                // ==================================

                var alternativa = item.Alternativas.FirstOrDefault(a =>
                    a.Activa
                    && a.AlimentoId == dto.AlimentoDestinoId
                    && a.GrupoEquivalenciaId == dto.GrupoEquivalenciaId
                );

                if (alternativa is null)
                {
                    return Error(
                        "La alternativa seleccionada no está disponible.",
                        TipoErrorDieta.Validacion
                    );
                }

                var destino = alternativa.Alimento;
                var grupo = alternativa.GrupoEquivalencia;

                if (
                    origen.Id == destino.Id
                    || !destino.Activo
                    || destino.NutricionistaId != nutricionistaId
                    || !DatosValidos(destino)
                    || !grupo.Activo
                    || grupo.NutricionistaId != nutricionistaId
                    || !Enum.IsDefined(grupo.Criterio)
                )
                {
                    return Error(
                        "La alternativa o el grupo de equivalencia " + "no son válidos.",
                        TipoErrorDieta.Validacion
                    );
                }

                // ==================================
                // MEMBRESÍAS ACTIVAS
                // ==================================

                var alternativasActivas = item.Alternativas.Where(a => a.Activa).ToList();

                var gruposIds = alternativasActivas
                    .Select(a => a.GrupoEquivalenciaId)
                    .Distinct()
                    .ToList();

                var alimentosIds = alternativasActivas
                    .Select(a => a.AlimentoId)
                    .Append(origen.Id)
                    .Append(destino.Id)
                    .Distinct()
                    .ToList();

                var membresias = await _context
                    .EquivalenciasAlimentos.AsNoTracking()
                    .Where(e =>
                        e.Activa
                        && gruposIds.Contains(e.GrupoEquivalenciaId)
                        && alimentosIds.Contains(e.AlimentoId)
                    )
                    .Select(e => new { e.GrupoEquivalenciaId, e.AlimentoId })
                    .ToListAsync(cancellationToken);

                var membresiasValidas = membresias
                    .Select(e => (e.GrupoEquivalenciaId, e.AlimentoId))
                    .ToHashSet();

                if (
                    !membresiasValidas.Contains((grupo.Id, origen.Id))
                    || !membresiasValidas.Contains((grupo.Id, destino.Id))
                )
                {
                    return Error(
                        "Los alimentos ya no pertenecen activamente "
                            + "al grupo de equivalencias.",
                        TipoErrorDieta.Validacion
                    );
                }

                // ==================================
                // RECALCULAR CANTIDAD EN BACKEND
                // ==================================

                var cantidadTeorica = CalculadoraEquivalencias.CalcularCantidadDestino(
                    item.Cantidad,
                    origen,
                    destino,
                    grupo.Criterio
                );

                var cantidadNueva = AjustarCantidad(cantidadTeorica, destino.UnidadBase);

                if (cantidadNueva <= 0m)
                {
                    return Error(
                        "La equivalencia no permite obtener " + "una cantidad práctica válida.",
                        TipoErrorDieta.Validacion
                    );
                }

                // La cantidad debe coincidir con la
                // simulación previamente confirmada.
                if (cantidadNueva != dto.CantidadDestinoEsperada)
                {
                    return Error(
                        "La cantidad equivalente cambió. " + "Volvé a simular antes de aplicar.",
                        TipoErrorDieta.Validacion
                    );
                }

                // ==================================
                // CONSERVAR VALORES ANTERIORES
                // ==================================

                var alimentoAnteriorId = origen.Id;
                var nombreAnterior = origen.Nombre;
                var cantidadAnterior = item.Cantidad;
                var unidadAnterior = item.UnidadMedida.ToString();

                // ==================================
                // APLICAR REEMPLAZO
                // ==================================

                item.AlimentoId = destino.Id;
                item.Alimento = destino;
                item.Cantidad = cantidadNueva;
                item.UnidadMedida = destino.UnidadBase;

                // ==================================
                // REVISAR ALTERNATIVAS RESTANTES
                // ==================================
                // No eliminamos alternativas.
                // Conservamos las que siguen siendo
                // válidas con el nuevo alimento origen.

                var desactivadas = 0;

                foreach (var otra in alternativasActivas)
                {
                    var otroGrupo = otra.GrupoEquivalencia;
                    var otroAlimento = otra.Alimento;

                    var grupoValido =
                        otroGrupo.Activo
                        && otroGrupo.NutricionistaId == nutricionistaId
                        && Enum.IsDefined(otroGrupo.Criterio);

                    var alimentoValido =
                        otroAlimento.Activo
                        && otroAlimento.NutricionistaId == nutricionistaId
                        && DatosValidos(otroAlimento);

                    var membresiaOrigenValida = membresiasValidas.Contains(
                        (otroGrupo.Id, destino.Id)
                    );

                    var membresiaDestinoValida = membresiasValidas.Contains(
                        (otroGrupo.Id, otroAlimento.Id)
                    );

                    var criterioValido = false;

                    if (
                        grupoValido
                        && alimentoValido
                        && membresiaOrigenValida
                        && membresiaDestinoValida
                        && otroAlimento.Id != destino.Id
                    )
                    {
                        var valorOrigen = CalculadoraEquivalencias.ObtenerValorCriterio(
                            destino,
                            otroGrupo.Criterio
                        );

                        var valorDestino = CalculadoraEquivalencias.ObtenerValorCriterio(
                            otroAlimento,
                            otroGrupo.Criterio
                        );

                        criterioValido =
                            valorOrigen.HasValue
                            && valorOrigen.Value > 0m
                            && valorDestino.HasValue
                            && valorDestino.Value > 0m;
                    }

                    var conservar =
                        otroAlimento.Id != destino.Id
                        && grupoValido
                        && alimentoValido
                        && membresiaOrigenValida
                        && membresiaDestinoValida
                        && criterioValido;

                    if (!conservar)
                    {
                        otra.Activa = false;
                        desactivadas++;
                    }
                }

                // ==================================
                // GUARDAR CAMBIOS
                // ==================================

                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                // ==================================
                // RESPUESTA
                // ==================================

                return new ResultadoDieta<SustitucionAplicadaDto>
                {
                    Exitoso = true,
                    TipoError = TipoErrorDieta.Ninguno,

                    Datos = new SustitucionAplicadaDto
                    {
                        DietaId = dietaId,
                        ComidaId = comidaId,
                        ItemId = item.Id,

                        AlimentoAnteriorId = alimentoAnteriorId,
                        NombreAlimentoAnterior = nombreAnterior,
                        CantidadAnterior = cantidadAnterior,
                        UnidadAnterior = unidadAnterior,

                        AlimentoNuevoId = destino.Id,
                        NombreAlimentoNuevo = destino.Nombre,
                        CantidadNueva = cantidadNueva,
                        UnidadNueva = destino.UnidadBase.ToString(),

                        AlternativasDesactivadas = desactivadas,
                        Aplicada = true,
                    },
                };
            });
        }
        catch (PostgresException ex) when (ex.SqlState is "40001" or "40P01")
        {
            return Error(
                "La dieta se modificó concurrentemente. " + "Volvé a consultar y reintentá.",
                TipoErrorDieta.Validacion
            );
        }
    }

    // ==========================================
    // VALIDAR DATOS NUTRICIONALES
    // ==========================================

    private static bool DatosValidos(Alimento alimento)
    {
        return alimento.CantidadBase > 0m
            && alimento.Calorias is >= 0m
            && alimento.Proteinas is >= 0m
            && alimento.Carbohidratos is >= 0m
            && alimento.Grasas is >= 0m;
    }

    // ==========================================
    // CANTIDAD PRÁCTICA
    // Misma regla del simulador actual.
    // ==========================================

    private static decimal AjustarCantidad(decimal cantidad, UnidadMedida unidad)
    {
        var paso = unidad switch
        {
            UnidadMedida.Gramos => 5m,
            UnidadMedida.Mililitros => 50m,
            UnidadMedida.Unidad => 1m,
            UnidadMedida.Porcion => 0.5m,
            _ => 0m,
        };

        if (paso <= 0m)
            return 0m;

        return Math.Round(cantidad / paso, 0, MidpointRounding.AwayFromZero) * paso;
    }

    // ==========================================
    // ERROR
    // ==========================================

    private static ResultadoDieta<SustitucionAplicadaDto> Error(string mensaje, TipoErrorDieta tipo)
    {
        return new ResultadoDieta<SustitucionAplicadaDto>
        {
            Exitoso = false,
            Error = mensaje,
            TipoError = tipo,
        };
    }
}
