using Microsoft.EntityFrameworkCore;

using NutriApi.Calculos;
using NutriApi.DTOs.Dietas;

using NutriApp.Data;
using NutriApp.Enums;
using NutriApp.Models.Alimentos;
using NutriApp.Models.Dietas;

namespace NutriApi.Services.Dietas;

public class OpcionComidaService : IOpcionComidaService
{
    private readonly NutriAppDbContext _context;


    public OpcionComidaService(
        NutriAppDbContext context)
    {
        _context = context;
    }


    // ==========================================
    // CREAR OPCIÓN
    // ==========================================

    public async Task<
        ResultadoDieta<OpcionSeccionComidaDto>>
        CrearOpcionAsync(
            int nutricionistaId,
            int pacienteId,
            int dietaId,
            int comidaId,
            int seccionId,
            CrearOpcionSeccionComidaDto dto)
    {
        var seccion =
            await ObtenerSeccionModificableAsync(
                nutricionistaId,
                pacienteId,
                dietaId,
                comidaId,
                seccionId
            );


        if (seccion is null)
        {
            return Error<OpcionSeccionComidaDto>(
                "Sección no encontrada.",
                TipoErrorDieta.NoEncontrado
            );
        }


        if (seccion.Comida.Dieta.Estado ==
            EstadoDieta.Archivada)
        {
            return Error<OpcionSeccionComidaDto>(
                "No se puede modificar una dieta archivada.",
                TipoErrorDieta.Validacion
            );
        }


        /*
         * Si esta opción pasa a ser la predeterminada,
         * quitamos la marca de las demás opciones.
         */

        if (dto.EsPredeterminada)
        {
            var opcionesPredeterminadas =
                await _context
                    .OpcionesSeccionesComidas
                    .Where(o =>
                        o.SeccionComidaId ==
                        seccionId
                        &&
                        o.EsPredeterminada
                    )
                    .ToListAsync();


            foreach (var opcion
                     in opcionesPredeterminadas)
            {
                opcion.EsPredeterminada =
                    false;
            }
        }


        var nuevaOpcion =
            new OpcionSeccionComida
            {
                SeccionComidaId =
                    seccionId,

                Nombre =
                    dto.Nombre.Trim(),

                Orden =
                    dto.Orden,

                EsPredeterminada =
                    dto.EsPredeterminada,

                Observaciones =
                    dto.Observaciones?.Trim()
            };


        _context.OpcionesSeccionesComidas
            .Add(nuevaOpcion);


        await _context.SaveChangesAsync();


        return new ResultadoDieta<
            OpcionSeccionComidaDto>
        {
            Exitoso = true,

            Datos =
                MapearOpcion(
                    nuevaOpcion,
                    new List<ItemOpcionComidaDto>()
                ),

            TipoError =
                TipoErrorDieta.Ninguno
        };
    }


    // ==========================================
    // LISTAR OPCIONES
    // ==========================================

    public async Task<
        ResultadoDieta<List<OpcionSeccionComidaDto>>>
        ObtenerOpcionesAsync(
            int nutricionistaId,
            int pacienteId,
            int dietaId,
            int comidaId,
            int seccionId)
    {
        var existe =
            await _context.SeccionesComidas
                .AsNoTracking()
                .AnyAsync(s =>
                    s.Id == seccionId
                    &&
                    s.ComidaId ==
                    comidaId
                    &&
                    s.Comida.DietaId ==
                    dietaId
                    &&
                    s.Comida.Dieta.PacienteId ==
                    pacienteId
                    &&
                    s.Comida.Dieta.Paciente
                        .NutricionistaId ==
                    nutricionistaId
                );


        if (!existe)
        {
            return Error<
                List<OpcionSeccionComidaDto>>(
                "Sección no encontrada.",
                TipoErrorDieta.NoEncontrado
            );
        }


        /*
         * Primero traemos las entidades.
         *
         * La CalculadoraNutricional es código C#,
         * por lo que calculamos los valores
         * después de ejecutar la consulta SQL.
         */

        var entidades =
            await _context
                .OpcionesSeccionesComidas
                .AsNoTracking()
                .Include(o => o.Items)
                    .ThenInclude(i =>
                        i.Alimento
                    )
                .Include(o => o.Items)
                    .ThenInclude(i =>
                        i.Alternativas
                    )
                .Where(o =>
                    o.SeccionComidaId ==
                    seccionId
                )
                .OrderBy(o =>
                    o.Orden
                )
                .ThenBy(o =>
                    o.Id
                )
                .AsSplitQuery()
                .ToListAsync();


        var opciones =
            entidades
                .Select(o =>
                    MapearOpcion(
                        o,
                        o.Items
                            .OrderBy(i =>
                                i.Orden
                            )
                            .ThenBy(i =>
                                i.Id
                            )
                            .Select(i =>
                                MapearItem(i)
                            )
                            .ToList()
                    )
                )
                .ToList();


        return new ResultadoDieta<
            List<OpcionSeccionComidaDto>>
        {
            Exitoso = true,

            Datos =
                opciones,

            TipoError =
                TipoErrorDieta.Ninguno
        };
    }


    // ==========================================
    // DETALLE OPCIÓN
    // ==========================================

    public async Task<
        ResultadoDieta<OpcionSeccionComidaDto>>
        ObtenerOpcionAsync(
            int nutricionistaId,
            int pacienteId,
            int dietaId,
            int comidaId,
            int seccionId,
            int opcionId)
    {
        var opcion =
            await _context
                .OpcionesSeccionesComidas
                .AsNoTracking()
                .Include(o => o.Items)
                    .ThenInclude(i =>
                        i.Alimento
                    )
                .Include(o => o.Items)
                    .ThenInclude(i =>
                        i.Alternativas
                    )
                .Where(o =>
                    o.Id ==
                    opcionId
                    &&
                    o.SeccionComidaId ==
                    seccionId
                    &&
                    o.SeccionComida.ComidaId ==
                    comidaId
                    &&
                    o.SeccionComida
                        .Comida.DietaId ==
                    dietaId
                    &&
                    o.SeccionComida
                        .Comida.Dieta
                        .PacienteId ==
                    pacienteId
                    &&
                    o.SeccionComida
                        .Comida.Dieta
                        .Paciente.NutricionistaId ==
                    nutricionistaId
                )
                .AsSplitQuery()
                .FirstOrDefaultAsync();


        if (opcion is null)
        {
            return Error<OpcionSeccionComidaDto>(
                "Opción no encontrada.",
                TipoErrorDieta.NoEncontrado
            );
        }


        var items =
            opcion.Items
                .OrderBy(i =>
                    i.Orden
                )
                .ThenBy(i =>
                    i.Id
                )
                .Select(i =>
                    MapearItem(i)
                )
                .ToList();


        return new ResultadoDieta<
            OpcionSeccionComidaDto>
        {
            Exitoso = true,

            Datos =
                MapearOpcion(
                    opcion,
                    items
                ),

            TipoError =
                TipoErrorDieta.Ninguno
        };
    }


    // ==========================================
    // EDITAR OPCIÓN
    // ==========================================

    public async Task<
        ResultadoDieta<OpcionSeccionComidaDto>>
        EditarOpcionAsync(
            int nutricionistaId,
            int pacienteId,
            int dietaId,
            int comidaId,
            int seccionId,
            int opcionId,
            EditarOpcionSeccionComidaDto dto)
    {
        var opcion =
            await _context
                .OpcionesSeccionesComidas
                .Include(o => o.Items)
                    .ThenInclude(i =>
                        i.Alimento
                    )
                .Include(o => o.Items)
                    .ThenInclude(i =>
                        i.Alternativas
                    )
                .Include(o =>
                    o.SeccionComida
                )
                    .ThenInclude(s =>
                        s.Comida
                    )
                        .ThenInclude(c =>
                            c.Dieta
                        )
                            .ThenInclude(d =>
                                d.Paciente
                            )
                .FirstOrDefaultAsync(o =>
                    o.Id ==
                    opcionId
                    &&
                    o.SeccionComidaId ==
                    seccionId
                    &&
                    o.SeccionComida.ComidaId ==
                    comidaId
                    &&
                    o.SeccionComida
                        .Comida.DietaId ==
                    dietaId
                    &&
                    o.SeccionComida
                        .Comida.Dieta
                        .PacienteId ==
                    pacienteId
                    &&
                    o.SeccionComida
                        .Comida.Dieta
                        .Paciente.NutricionistaId ==
                    nutricionistaId
                );


        if (opcion is null)
        {
            return Error<OpcionSeccionComidaDto>(
                "Opción no encontrada.",
                TipoErrorDieta.NoEncontrado
            );
        }


        if (opcion.SeccionComida
                .Comida
                .Dieta
                .Estado ==
            EstadoDieta.Archivada)
        {
            return Error<OpcionSeccionComidaDto>(
                "No se puede modificar una dieta archivada.",
                TipoErrorDieta.Validacion
            );
        }


        /*
         * Si esta opción pasa a ser predeterminada,
         * quitamos la marca de las demás.
         */

        if (dto.EsPredeterminada &&
            !opcion.EsPredeterminada)
        {
            var otras =
                await _context
                    .OpcionesSeccionesComidas
                    .Where(o =>
                        o.SeccionComidaId ==
                        seccionId
                        &&
                        o.Id !=
                        opcionId
                        &&
                        o.EsPredeterminada
                    )
                    .ToListAsync();


            foreach (var otra
                     in otras)
            {
                otra.EsPredeterminada =
                    false;
            }
        }


        opcion.Nombre =
            dto.Nombre.Trim();

        opcion.Orden =
            dto.Orden;

        opcion.EsPredeterminada =
            dto.EsPredeterminada;

        opcion.Observaciones =
            dto.Observaciones?.Trim();


        await _context.SaveChangesAsync();


        var items =
            opcion.Items
                .OrderBy(i =>
                    i.Orden
                )
                .ThenBy(i =>
                    i.Id
                )
                .Select(i =>
                    MapearItem(i)
                )
                .ToList();


        return new ResultadoDieta<
            OpcionSeccionComidaDto>
        {
            Exitoso = true,

            Datos =
                MapearOpcion(
                    opcion,
                    items
                ),

            TipoError =
                TipoErrorDieta.Ninguno
        };
    }


    // ==========================================
    // CREAR ITEM
    // ==========================================

    public async Task<
     ResultadoDieta<ItemOpcionComidaDto>>
     CrearItemAsync(
         int nutricionistaId,
         int pacienteId,
         int dietaId,
         int comidaId,
         int seccionId,
         int opcionId,
         CrearItemOpcionComidaDto dto)
    {
        var opcion =
            await _context
                .OpcionesSeccionesComidas
                .Include(o =>
                    o.SeccionComida
                )
                    .ThenInclude(s =>
                        s.Comida
                    )
                        .ThenInclude(c =>
                            c.Dieta
                        )
                            .ThenInclude(d =>
                                d.Paciente
                            )
                .FirstOrDefaultAsync(o =>
                    o.Id ==
                    opcionId
                    &&
                    o.SeccionComidaId ==
                    seccionId
                    &&
                    o.SeccionComida
                        .ComidaId ==
                    comidaId
                    &&
                    o.SeccionComida
                        .Comida.DietaId ==
                    dietaId
                    &&
                    o.SeccionComida
                        .Comida.Dieta
                        .PacienteId ==
                    pacienteId
                    &&
                    o.SeccionComida
                        .Comida.Dieta
                        .Paciente
                        .NutricionistaId ==
                    nutricionistaId
                );


        if (opcion is null)
        {
            return Error<ItemOpcionComidaDto>(
                "Opción no encontrada.",
                TipoErrorDieta.NoEncontrado
            );
        }


        if (opcion.SeccionComida
                .Comida
                .Dieta
                .Estado ==
            EstadoDieta.Archivada)
        {
            return Error<ItemOpcionComidaDto>(
                "No se puede modificar una dieta archivada.",
                TipoErrorDieta.Validacion
            );
        }


        if (!Enum.IsDefined(
                dto.UnidadMedida))
        {
            return Error<ItemOpcionComidaDto>(
                "La unidad de medida no es válida.",
                TipoErrorDieta.Validacion
            );
        }


        /*
         * El alimento debe pertenecer
         * al nutricionista autenticado.
         */

        var alimento =
            await _context.Alimentos
                .AsNoTracking()
                .FirstOrDefaultAsync(a =>
                    a.Id ==
                    dto.AlimentoId
                    &&
                    a.NutricionistaId ==
                    nutricionistaId
                );


        if (alimento is null)
        {
            return Error<ItemOpcionComidaDto>(
                "Alimento no encontrado.",
                TipoErrorDieta.Validacion
            );
        }


        if (!alimento.Activo)
        {
            return Error<ItemOpcionComidaDto>(
                "No se puede utilizar un alimento inactivo.",
                TipoErrorDieta.Validacion
            );
        }


        // ======================================
        // VALIDAR UNIDAD BASE DEL ALIMENTO
        // ======================================

        if (dto.UnidadMedida !=
            alimento.UnidadBase)
        {
            return Error<ItemOpcionComidaDto>(
                $"La unidad del ítem debe ser '{alimento.UnidadBase}' para el alimento '{alimento.Nombre}'.",
                TipoErrorDieta.Validacion
            );
        }


        var item =
            new ItemOpcionComida
            {
                OpcionSeccionComidaId =
                    opcionId,

                AlimentoId =
                    alimento.Id,

                Cantidad =
                    dto.Cantidad,

                UnidadMedida =
                    dto.UnidadMedida,

                Indicaciones =
                    dto.Indicaciones?.Trim(),

                Orden =
                    dto.Orden
            };


        _context.ItemsOpcionesComidas
            .Add(item);


        await _context.SaveChangesAsync();


        return new ResultadoDieta<
            ItemOpcionComidaDto>
        {
            Exitoso = true,

            Datos =
                MapearItem(
                    item,
                    alimento
                ),

            TipoError =
                TipoErrorDieta.Ninguno
        };
    }


    // ==========================================
    // EDITAR ITEM
    // ==========================================
    public async Task<
        ResultadoDieta<ItemOpcionComidaDto>>
        EditarItemAsync(
            int nutricionistaId,
            int pacienteId,
            int dietaId,
            int comidaId,
            int seccionId,
            int opcionId,
            int itemId,
            EditarItemOpcionComidaDto dto)
    {
        var item =
            await _context.ItemsOpcionesComidas
                .Include(i =>
                    i.Alternativas
                )
                .Include(i =>
                    i.OpcionSeccionComida
                )
                    .ThenInclude(o =>
                        o.SeccionComida
                    )
                        .ThenInclude(s =>
                            s.Comida
                        )
                            .ThenInclude(c =>
                                c.Dieta
                            )
                                .ThenInclude(d =>
                                    d.Paciente
                                )
                .FirstOrDefaultAsync(i =>
                    i.Id ==
                    itemId
                    &&
                    i.OpcionSeccionComidaId ==
                    opcionId
                    &&
                    i.OpcionSeccionComida
                        .SeccionComidaId ==
                    seccionId
                    &&
                    i.OpcionSeccionComida
                        .SeccionComida
                        .ComidaId ==
                    comidaId
                    &&
                    i.OpcionSeccionComida
                        .SeccionComida
                        .Comida.DietaId ==
                    dietaId
                    &&
                    i.OpcionSeccionComida
                        .SeccionComida
                        .Comida.Dieta
                        .PacienteId ==
                    pacienteId
                    &&
                    i.OpcionSeccionComida
                        .SeccionComida
                        .Comida.Dieta
                        .Paciente
                        .NutricionistaId ==
                    nutricionistaId
                );


        if (item is null)
        {
            return Error<ItemOpcionComidaDto>(
                "Ítem no encontrado.",
                TipoErrorDieta.NoEncontrado
            );
        }


        if (item.OpcionSeccionComida
                .SeccionComida
                .Comida
                .Dieta
                .Estado ==
            EstadoDieta.Archivada)
        {
            return Error<ItemOpcionComidaDto>(
                "No se puede modificar una dieta archivada.",
                TipoErrorDieta.Validacion
            );
        }


        if (!Enum.IsDefined(
                dto.UnidadMedida))
        {
            return Error<ItemOpcionComidaDto>(
                "La unidad de medida no es válida.",
                TipoErrorDieta.Validacion
            );
        }


        var alimento =
            await _context.Alimentos
                .AsNoTracking()
                .FirstOrDefaultAsync(a =>
                    a.Id ==
                    dto.AlimentoId
                    &&
                    a.NutricionistaId ==
                    nutricionistaId
                );


        if (alimento is null)
        {
            return Error<ItemOpcionComidaDto>(
                "Alimento no encontrado.",
                TipoErrorDieta.Validacion
            );
        }


        if (!alimento.Activo)
        {
            return Error<ItemOpcionComidaDto>(
                "No se puede utilizar un alimento inactivo.",
                TipoErrorDieta.Validacion
            );
        }


        // ======================================
        // VALIDAR UNIDAD BASE DEL ALIMENTO
        // ======================================

        if (dto.UnidadMedida !=
            alimento.UnidadBase)
        {
            return Error<ItemOpcionComidaDto>(
                $"La unidad del ítem debe ser '{alimento.UnidadBase}' para el alimento '{alimento.Nombre}'.",
                TipoErrorDieta.Validacion
            );
        }


        item.AlimentoId =
            alimento.Id;

        item.Cantidad =
            dto.Cantidad;

        item.UnidadMedida =
            dto.UnidadMedida;

        item.Indicaciones =
            dto.Indicaciones?.Trim();

        item.Orden =
            dto.Orden;


        await _context.SaveChangesAsync();


        return new ResultadoDieta<
            ItemOpcionComidaDto>
        {
            Exitoso = true,

            Datos =
                MapearItem(
                    item,
                    alimento
                ),

            TipoError =
                TipoErrorDieta.Ninguno
        };
    }

    // ==========================================
    // ELIMINAR OPCIÓN
    // ==========================================

    public async Task<ResultadoDieta<bool>>
        EliminarOpcionAsync(
            int nutricionistaId,
            int pacienteId,
            int dietaId,
            int comidaId,
            int seccionId,
            int opcionId)
    {
        var opcion =
            await _context
                .OpcionesSeccionesComidas
                .Include(o =>
                    o.SeccionComida
                )
                    .ThenInclude(s =>
                        s.Comida
                    )
                        .ThenInclude(c =>
                            c.Dieta
                        )
                .FirstOrDefaultAsync(o =>
                    o.Id == opcionId
                    &&
                    o.SeccionComidaId == seccionId
                    &&
                    o.SeccionComida.ComidaId == comidaId
                    &&
                    o.SeccionComida.Comida.DietaId == dietaId
                    &&
                    o.SeccionComida.Comida.Dieta.PacienteId ==
                    pacienteId
                    &&
                    o.SeccionComida.Comida.Dieta
                        .Paciente.NutricionistaId ==
                    nutricionistaId
                );


        if (opcion is null)
        {
            return Error<bool>(
                "Opción no encontrada.",
                TipoErrorDieta.NoEncontrado
            );
        }


        if (opcion.SeccionComida
                .Comida
                .Dieta
                .Estado ==
            EstadoDieta.Archivada)
        {
            return Error<bool>(
                "No se puede modificar una dieta archivada.",
                TipoErrorDieta.Validacion
            );
        }


        _context.OpcionesSeccionesComidas
            .Remove(opcion);


        await _context.SaveChangesAsync();


        return new ResultadoDieta<bool>
        {
            Exitoso = true,
            Datos = true,
            TipoError = TipoErrorDieta.Ninguno
        };
    }

    // ==========================================
    // ELIMINAR ITEM
    // ==========================================

    public async Task<ResultadoDieta<bool>>
        EliminarItemAsync(
            int nutricionistaId,
            int pacienteId,
            int dietaId,
            int comidaId,
            int seccionId,
            int opcionId,
            int itemId)
    {
        var item =
            await _context.ItemsOpcionesComidas
                .Include(i =>
                    i.OpcionSeccionComida
                )
                    .ThenInclude(o =>
                        o.SeccionComida
                    )
                        .ThenInclude(s =>
                            s.Comida
                        )
                            .ThenInclude(c =>
                                c.Dieta
                            )
                .FirstOrDefaultAsync(i =>
                    i.Id == itemId
                    &&
                    i.OpcionSeccionComidaId ==
                    opcionId
                    &&
                    i.OpcionSeccionComida
                        .SeccionComidaId ==
                    seccionId
                    &&
                    i.OpcionSeccionComida
                        .SeccionComida
                        .ComidaId ==
                    comidaId
                    &&
                    i.OpcionSeccionComida
                        .SeccionComida
                        .Comida
                        .DietaId ==
                    dietaId
                    &&
                    i.OpcionSeccionComida
                        .SeccionComida
                        .Comida
                        .Dieta
                        .PacienteId ==
                    pacienteId
                    &&
                    i.OpcionSeccionComida
                        .SeccionComida
                        .Comida
                        .Dieta
                        .Paciente
                        .NutricionistaId ==
                    nutricionistaId
                );


        if (item is null)
        {
            return Error<bool>(
                "Ítem no encontrado.",
                TipoErrorDieta.NoEncontrado
            );
        }


        if (item.OpcionSeccionComida
                .SeccionComida
                .Comida
                .Dieta
                .Estado ==
            EstadoDieta.Archivada)
        {
            return Error<bool>(
                "No se puede modificar una dieta archivada.",
                TipoErrorDieta.Validacion
            );
        }


        _context.ItemsOpcionesComidas
            .Remove(item);


        await _context.SaveChangesAsync();


        return new ResultadoDieta<bool>
        {
            Exitoso = true,
            Datos = true,
            TipoError = TipoErrorDieta.Ninguno
        };
    }



    // ==========================================
    // HELPER PARA SECCIÓN
    // ==========================================

    private async Task<SeccionComida?>
        ObtenerSeccionModificableAsync(
            int nutricionistaId,
            int pacienteId,
            int dietaId,
            int comidaId,
            int seccionId)
    {
        return await _context.SeccionesComidas
            .Include(s =>
                s.Comida
            )
                .ThenInclude(c =>
                    c.Dieta
                )
                    .ThenInclude(d =>
                        d.Paciente
                    )
            .FirstOrDefaultAsync(s =>
                s.Id ==
                seccionId
                &&
                s.ComidaId ==
                comidaId
                &&
                s.Comida.DietaId ==
                dietaId
                &&
                s.Comida.Dieta
                    .PacienteId ==
                pacienteId
                &&
                s.Comida.Dieta
                    .Paciente
                    .NutricionistaId ==
                nutricionistaId
            );
    }


    // ==========================================
    // MAPPERS
    // ==========================================

    private static OpcionSeccionComidaDto
    MapearOpcion(
        OpcionSeccionComida opcion,
        List<ItemOpcionComidaDto> items)
    {
        var resultadoTotales =
            CalculadoraTotalesNutricionales
                .CalcularOpcion(
                    opcion.Items
                );


        return new OpcionSeccionComidaDto
        {
            Id =
                opcion.Id,

            SeccionComidaId =
                opcion.SeccionComidaId,

            Nombre =
                opcion.Nombre,

            Orden =
                opcion.Orden,

            EsPredeterminada =
                opcion.EsPredeterminada,

            Observaciones =
                opcion.Observaciones,

            Items =
                items,

            Totales =
                MapearTotales(
                    resultadoTotales
                )
        };
    }
    private static TotalesNutricionalesDto
    MapearTotales(
        ResultadoTotalesNutricionales resultado)
    {
        return new TotalesNutricionalesDto
        {
            Calorias =
                resultado.Calorias,

            Proteinas =
                resultado.Proteinas,

            Carbohidratos =
                resultado.Carbohidratos,

            Grasas =
                resultado.Grasas,

            CantidadItems =
                resultado.CantidadItems,

            ItemsSinCalculo =
                resultado.ItemsSinCalculo,

            SeccionesSinOpcionPredeterminada =
                resultado.SeccionesSinOpcionPredeterminada,

            EsCompleto =
                resultado.EsCompleto
        };
    }

    private static ItemOpcionComidaDto
        MapearItem(
            ItemOpcionComida item,
            Alimento? alimentoOverride = null)
    {
        var alimento =
            alimentoOverride
            ??
            item.Alimento;


        return new ItemOpcionComidaDto
        {
            Id =
                item.Id,

            AlimentoId =
                item.AlimentoId,

            Alimento =
                alimento?.Nombre
                ??
                string.Empty,

            Cantidad =
                item.Cantidad,

            UnidadMedida =
                item.UnidadMedida
                    .ToString(),

            Indicaciones =
                item.Indicaciones,

            Orden =
                item.Orden,

            CantidadAlternativas =
                item.Alternativas?
                    .Count(a =>
                        a.Activa
                    )
                ??
                0,

            Nutricion =
                alimento is null
                    ? null
                    : CalcularNutricion(
                        item.Cantidad,
                        item.UnidadMedida,
                        alimento
                    )
        };
    }


    // ==========================================
    // CÁLCULO NUTRICIONAL
    // ==========================================

    private static NutricionCalculadaDto?
        CalcularNutricion(
            decimal cantidad,
            UnidadMedida unidadMedida,
            Alimento alimento)
    {
        /*
         * Solo podemos aplicar proporcionalidad
         * si el item está expresado en la misma
         * unidad que la información nutricional
         * base del alimento.
         *
         * Ejemplo:
         *
         * Alimento:
         * 100 gramos = 350 kcal
         *
         * Item:
         * 200 gramos
         *
         * factor:
         * 200 / 100 = 2
         *
         * kcal:
         * 350 * 2 = 700
         */

        if (unidadMedida !=
            alimento.UnidadBase)
        {
            return null;
        }


        var resultado =
            CalculadoraNutricional
                .Calcular(
                    cantidad,
                    alimento.CantidadBase,
                    alimento.Calorias,
                    alimento.Proteinas,
                    alimento.Carbohidratos,
                    alimento.Grasas
                );


        return new NutricionCalculadaDto
        {
            Calorias =
                resultado.Calorias,

            Proteinas =
                resultado.Proteinas,

            Carbohidratos =
                resultado.Carbohidratos,

            Grasas =
                resultado.Grasas
        };
    }


    // ==========================================
    // ERROR
    // ==========================================

    private static ResultadoDieta<T>
        Error<T>(
            string mensaje,
            TipoErrorDieta tipo)
    {
        return new ResultadoDieta<T>
        {
            Exitoso =
                false,

            Error =
                mensaje,

            TipoError =
                tipo
        };
    }
}