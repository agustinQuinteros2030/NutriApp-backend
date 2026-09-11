using Microsoft.EntityFrameworkCore;

using NutriApi.DTOs.Dietas;

using NutriApp.Data;
using NutriApp.Enums;
using NutriApp.Models.Dietas;

namespace NutriApi.Services.Dietas;

public class ComplementoDietaService
    : IComplementoDietaService
{
    private readonly NutriAppDbContext _context;


    public ComplementoDietaService(
        NutriAppDbContext context)
    {
        _context = context;
    }


    // ==========================================
    // OBTENER HIDRATACIÓN
    // ==========================================

    public async Task<
        ResultadoDieta<HidratacionDietaDto?>>
        ObtenerHidratacionAsync(
            int nutricionistaId,
            int pacienteId,
            int dietaId)
    {
        var dietaExiste =
            await ExisteDietaAsync(
                nutricionistaId,
                pacienteId,
                dietaId
            );


        if (!dietaExiste)
        {
            return Error<HidratacionDietaDto?>(
                "Dieta no encontrada.",
                TipoErrorDieta.NoEncontrado
            );
        }


        var hidratacion =
            await _context.HidratacionesDietas
                .AsNoTracking()
                .FirstOrDefaultAsync(h =>
                    h.DietaId ==
                    dietaId
                );


        return new ResultadoDieta<
            HidratacionDietaDto?>
        {
            Exitoso = true,

            Datos =
                hidratacion is null
                    ? null
                    : MapearHidratacion(
                        hidratacion
                    ),

            TipoError =
                TipoErrorDieta.Ninguno
        };
    }


    // ==========================================
    // GUARDAR HIDRATACIÓN
    // ==========================================

    public async Task<
        ResultadoDieta<HidratacionDietaDto>>
        GuardarHidratacionAsync(
            int nutricionistaId,
            int pacienteId,
            int dietaId,
            GuardarHidratacionDietaDto dto)
    {
        var dieta =
            await ObtenerDietaAsync(
                nutricionistaId,
                pacienteId,
                dietaId
            );


        if (dieta is null)
        {
            return Error<HidratacionDietaDto>(
                "Dieta no encontrada.",
                TipoErrorDieta.NoEncontrado
            );
        }


        if (dieta.Estado ==
            EstadoDieta.Archivada)
        {
            return Error<HidratacionDietaDto>(
                "No se puede modificar una dieta archivada.",
                TipoErrorDieta.Validacion
            );
        }


        if (dto.MililitrosDiarios.HasValue &&
            dto.MililitrosDiarios.Value <= 0)
        {
            return Error<HidratacionDietaDto>(
                "Los mililitros diarios deben ser mayores a cero.",
                TipoErrorDieta.Validacion
            );
        }


        if (dto.VasosDiarios.HasValue &&
            dto.VasosDiarios.Value <= 0)
        {
            return Error<HidratacionDietaDto>(
                "Los vasos diarios deben ser mayores a cero.",
                TipoErrorDieta.Validacion
            );
        }


        var hidratacion =
            await _context.HidratacionesDietas
                .FirstOrDefaultAsync(h =>
                    h.DietaId ==
                    dietaId
                );


        if (hidratacion is null)
        {
            hidratacion =
                new HidratacionDieta
                {
                    DietaId =
                        dietaId,

                    MililitrosDiarios =
                        dto.MililitrosDiarios,

                    VasosDiarios =
                        dto.VasosDiarios,

                    Observaciones =
                        Limpiar(
                            dto.Observaciones
                        )
                };


            _context.HidratacionesDietas
                .Add(hidratacion);
        }
        else
        {
            hidratacion.MililitrosDiarios =
                dto.MililitrosDiarios;

            hidratacion.VasosDiarios =
                dto.VasosDiarios;

            hidratacion.Observaciones =
                Limpiar(
                    dto.Observaciones
                );
        }


        await _context.SaveChangesAsync();


        return new ResultadoDieta<
            HidratacionDietaDto>
        {
            Exitoso = true,

            Datos =
                MapearHidratacion(
                    hidratacion
                ),

            TipoError =
                TipoErrorDieta.Ninguno
        };
    }


    // ==========================================
    // OBTENER SUPLEMENTACIÓN
    // ==========================================

    public async Task<
        ResultadoDieta<SuplementacionDietaDto?>>
        ObtenerSuplementacionAsync(
            int nutricionistaId,
            int pacienteId,
            int dietaId)
    {
        var dietaExiste =
            await ExisteDietaAsync(
                nutricionistaId,
                pacienteId,
                dietaId
            );


        if (!dietaExiste)
        {
            return Error<SuplementacionDietaDto?>(
                "Dieta no encontrada.",
                TipoErrorDieta.NoEncontrado
            );
        }


        var suplementacion =
            await _context.SuplementacionesDietas
                .AsNoTracking()
                .Include(s =>
                    s.Items
                )
                .FirstOrDefaultAsync(s =>
                    s.DietaId ==
                    dietaId
                );


        return new ResultadoDieta<
            SuplementacionDietaDto?>
        {
            Exitoso = true,

            Datos =
                suplementacion is null
                    ? null
                    : MapearSuplementacion(
                        suplementacion
                    ),

            TipoError =
                TipoErrorDieta.Ninguno
        };
    }


    // ==========================================
    // GUARDAR SUPLEMENTACIÓN
    // ==========================================

    public async Task<
        ResultadoDieta<SuplementacionDietaDto>>
        GuardarSuplementacionAsync(
            int nutricionistaId,
            int pacienteId,
            int dietaId,
            GuardarSuplementacionDietaDto dto)
    {
        var dieta =
            await ObtenerDietaAsync(
                nutricionistaId,
                pacienteId,
                dietaId
            );


        if (dieta is null)
        {
            return Error<SuplementacionDietaDto>(
                "Dieta no encontrada.",
                TipoErrorDieta.NoEncontrado
            );
        }


        if (dieta.Estado ==
            EstadoDieta.Archivada)
        {
            return Error<SuplementacionDietaDto>(
                "No se puede modificar una dieta archivada.",
                TipoErrorDieta.Validacion
            );
        }


        var suplementacion =
            await _context.SuplementacionesDietas
                .Include(s =>
                    s.Items
                )
                .FirstOrDefaultAsync(s =>
                    s.DietaId ==
                    dietaId
                );


        if (suplementacion is null)
        {
            suplementacion =
                new SuplementacionDieta
                {
                    DietaId =
                        dietaId,

                    ObservacionesGenerales =
                        Limpiar(
                            dto.ObservacionesGenerales
                        )
                };


            _context.SuplementacionesDietas
                .Add(suplementacion);
        }
        else
        {
            suplementacion.ObservacionesGenerales =
                Limpiar(
                    dto.ObservacionesGenerales
                );
        }


        await _context.SaveChangesAsync();


        return new ResultadoDieta<
            SuplementacionDietaDto>
        {
            Exitoso = true,

            Datos =
                MapearSuplementacion(
                    suplementacion
                ),

            TipoError =
                TipoErrorDieta.Ninguno
        };
    }


    // ==========================================
    // AGREGAR ITEM SUPLEMENTACIÓN
    // ==========================================

    public async Task<
        ResultadoDieta<ItemSuplementacionDto>>
        AgregarItemSuplementacionAsync(
            int nutricionistaId,
            int pacienteId,
            int dietaId,
            CrearItemSuplementacionDto dto)
    {
        var dieta =
            await ObtenerDietaAsync(
                nutricionistaId,
                pacienteId,
                dietaId
            );


        if (dieta is null)
        {
            return Error<ItemSuplementacionDto>(
                "Dieta no encontrada.",
                TipoErrorDieta.NoEncontrado
            );
        }


        if (dieta.Estado ==
            EstadoDieta.Archivada)
        {
            return Error<ItemSuplementacionDto>(
                "No se puede modificar una dieta archivada.",
                TipoErrorDieta.Validacion
            );
        }


        var errorValidacion =
            ValidarItem(
                dto.Nombre,
                dto.Cantidad,
                dto.Orden
            );


        if (errorValidacion is not null)
        {
            return Error<ItemSuplementacionDto>(
                errorValidacion,
                TipoErrorDieta.Validacion
            );
        }


        /*
         * Si todavía no existe el contenedor
         * de suplementación, lo creamos.
         */

        var suplementacion =
            await _context.SuplementacionesDietas
                .FirstOrDefaultAsync(s =>
                    s.DietaId ==
                    dietaId
                );


        if (suplementacion is null)
        {
            suplementacion =
                new SuplementacionDieta
                {
                    DietaId =
                        dietaId
                };


            _context.SuplementacionesDietas
                .Add(suplementacion);


            await _context.SaveChangesAsync();
        }


        var item =
            new ItemSuplementacion
            {
                SuplementacionDietaId =
                    suplementacion.Id,

                Nombre =
                    dto.Nombre.Trim(),

                Cantidad =
                    dto.Cantidad,

                Unidad =
                    Limpiar(
                        dto.Unidad
                    ),

                Momento =
                    Limpiar(
                        dto.Momento
                    ),

                Indicaciones =
                    Limpiar(
                        dto.Indicaciones
                    ),

                Orden =
                    dto.Orden
            };


        _context.ItemsSuplementacion
            .Add(item);


        await _context.SaveChangesAsync();


        return new ResultadoDieta<
            ItemSuplementacionDto>
        {
            Exitoso = true,

            Datos =
                MapearItem(
                    item
                ),

            TipoError =
                TipoErrorDieta.Ninguno
        };
    }


    // ==========================================
    // EDITAR ITEM SUPLEMENTACIÓN
    // ==========================================

    public async Task<
        ResultadoDieta<ItemSuplementacionDto>>
        EditarItemSuplementacionAsync(
            int nutricionistaId,
            int pacienteId,
            int dietaId,
            int itemId,
            EditarItemSuplementacionDto dto)
    {
        var dieta =
            await ObtenerDietaAsync(
                nutricionistaId,
                pacienteId,
                dietaId
            );


        if (dieta is null)
        {
            return Error<ItemSuplementacionDto>(
                "Dieta no encontrada.",
                TipoErrorDieta.NoEncontrado
            );
        }


        if (dieta.Estado ==
            EstadoDieta.Archivada)
        {
            return Error<ItemSuplementacionDto>(
                "No se puede modificar una dieta archivada.",
                TipoErrorDieta.Validacion
            );
        }


        var errorValidacion =
            ValidarItem(
                dto.Nombre,
                dto.Cantidad,
                dto.Orden
            );


        if (errorValidacion is not null)
        {
            return Error<ItemSuplementacionDto>(
                errorValidacion,
                TipoErrorDieta.Validacion
            );
        }


        /*
         * La consulta exige que el item pertenezca
         * a la suplementación de ESTA dieta.
         */

        var item =
            await _context.ItemsSuplementacion
                .Include(i =>
                    i.SuplementacionDieta
                )
                .FirstOrDefaultAsync(i =>
                    i.Id ==
                    itemId
                    &&
                    i.SuplementacionDieta
                        .DietaId ==
                    dietaId
                );


        if (item is null)
        {
            return Error<ItemSuplementacionDto>(
                "Suplemento no encontrado.",
                TipoErrorDieta.NoEncontrado
            );
        }


        item.Nombre =
            dto.Nombre.Trim();

        item.Cantidad =
            dto.Cantidad;

        item.Unidad =
            Limpiar(
                dto.Unidad
            );

        item.Momento =
            Limpiar(
                dto.Momento
            );

        item.Indicaciones =
            Limpiar(
                dto.Indicaciones
            );

        item.Orden =
            dto.Orden;


        await _context.SaveChangesAsync();


        return new ResultadoDieta<
            ItemSuplementacionDto>
        {
            Exitoso = true,

            Datos =
                MapearItem(
                    item
                ),

            TipoError =
                TipoErrorDieta.Ninguno
        };
    }


    // ==========================================
    // ELIMINAR ITEM SUPLEMENTACIÓN
    // ==========================================

    public async Task<
        ResultadoDieta<bool>>
        EliminarItemSuplementacionAsync(
            int nutricionistaId,
            int pacienteId,
            int dietaId,
            int itemId)
    {
        var dieta =
            await ObtenerDietaAsync(
                nutricionistaId,
                pacienteId,
                dietaId
            );


        if (dieta is null)
        {
            return Error<bool>(
                "Dieta no encontrada.",
                TipoErrorDieta.NoEncontrado
            );
        }


        if (dieta.Estado ==
            EstadoDieta.Archivada)
        {
            return Error<bool>(
                "No se puede modificar una dieta archivada.",
                TipoErrorDieta.Validacion
            );
        }


        var item =
            await _context.ItemsSuplementacion
                .Include(i =>
                    i.SuplementacionDieta
                )
                .FirstOrDefaultAsync(i =>
                    i.Id ==
                    itemId
                    &&
                    i.SuplementacionDieta
                        .DietaId ==
                    dietaId
                );


        if (item is null)
        {
            return Error<bool>(
                "Suplemento no encontrado.",
                TipoErrorDieta.NoEncontrado
            );
        }


        _context.ItemsSuplementacion
            .Remove(item);


        await _context.SaveChangesAsync();


        return new ResultadoDieta<bool>
        {
            Exitoso = true,

            Datos = true,

            TipoError =
                TipoErrorDieta.Ninguno
        };
    }


    // ==========================================
    // OWNERSHIP DIETA
    // ==========================================

    private async Task<Dieta?>
        ObtenerDietaAsync(
            int nutricionistaId,
            int pacienteId,
            int dietaId)
    {
        return await _context.Dietas
            .FirstOrDefaultAsync(d =>
                d.Id ==
                dietaId
                &&
                d.PacienteId ==
                pacienteId
                &&
                d.Paciente
                    .NutricionistaId ==
                nutricionistaId
            );
    }


    private async Task<bool>
        ExisteDietaAsync(
            int nutricionistaId,
            int pacienteId,
            int dietaId)
    {
        return await _context.Dietas
            .AsNoTracking()
            .AnyAsync(d =>
                d.Id ==
                dietaId
                &&
                d.PacienteId ==
                pacienteId
                &&
                d.Paciente
                    .NutricionistaId ==
                nutricionistaId
            );
    }


    // ==========================================
    // VALIDACIÓN ITEM
    // ==========================================

    private static string?
        ValidarItem(
            string nombre,
            decimal? cantidad,
            int orden)
    {
        if (string.IsNullOrWhiteSpace(
            nombre))
        {
            return
                "El nombre del suplemento es obligatorio.";
        }


        if (cantidad.HasValue &&
            cantidad.Value <= 0)
        {
            return
                "La cantidad debe ser mayor a cero.";
        }


        if (orden < 1 ||
            orden > 100)
        {
            return
                "El orden debe estar entre 1 y 100.";
        }


        return null;
    }


    // ==========================================
    // MAPPERS
    // ==========================================

    private static HidratacionDietaDto
        MapearHidratacion(
            HidratacionDieta hidratacion)
    {
        return new HidratacionDietaDto
        {
            Id =
                hidratacion.Id,

            DietaId =
                hidratacion.DietaId,

            MililitrosDiarios =
                hidratacion.MililitrosDiarios,

            VasosDiarios =
                hidratacion.VasosDiarios,

            Observaciones =
                hidratacion.Observaciones
        };
    }


    private static SuplementacionDietaDto
        MapearSuplementacion(
            SuplementacionDieta suplementacion)
    {
        return new SuplementacionDietaDto
        {
            Id =
                suplementacion.Id,

            DietaId =
                suplementacion.DietaId,

            ObservacionesGenerales =
                suplementacion
                    .ObservacionesGenerales,

            Items =
                suplementacion.Items
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
        };
    }


    private static ItemSuplementacionDto
        MapearItem(
            ItemSuplementacion item)
    {
        return new ItemSuplementacionDto
        {
            Id =
                item.Id,

            SuplementacionDietaId =
                item.SuplementacionDietaId,

            Nombre =
                item.Nombre,

            Cantidad =
                item.Cantidad,

            Unidad =
                item.Unidad,

            Momento =
                item.Momento,

            Indicaciones =
                item.Indicaciones,

            Orden =
                item.Orden
        };
    }


    // ==========================================
    // STRING HELPER
    // ==========================================

    private static string?
        Limpiar(
            string? valor)
    {
        if (string.IsNullOrWhiteSpace(
            valor))
        {
            return null;
        }


        return valor.Trim();
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
            Exitoso = false,

            Error =
                mensaje,

            TipoError =
                tipo
        };
    }
}