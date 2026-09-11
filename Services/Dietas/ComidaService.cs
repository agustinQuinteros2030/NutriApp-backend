using Microsoft.EntityFrameworkCore;

using NutriApi.Calculos;
using NutriApi.DTOs.Dietas;

using NutriApp.Data;
using NutriApp.Enums;
using NutriApp.Models.Dietas;

namespace NutriApi.Services.Dietas;

public class ComidaService : IComidaService
{
    private readonly NutriAppDbContext _context;


    public ComidaService(
        NutriAppDbContext context)
    {
        _context = context;
    }


    // ==========================================
    // CREAR COMIDA
    // ==========================================

    public async Task<ResultadoDieta<ComidaDetalleDto>>
        CrearComidaAsync(
            int nutricionistaId,
            int pacienteId,
            int dietaId,
            CrearComidaDto dto)
    {
        var dieta =
            await _context.Dietas
                .FirstOrDefaultAsync(d =>
                    d.Id == dietaId
                    &&
                    d.PacienteId == pacienteId
                    &&
                    d.Paciente.NutricionistaId ==
                    nutricionistaId
                );


        if (dieta is null)
        {
            return Error<ComidaDetalleDto>(
                "Dieta no encontrada.",
                TipoErrorDieta.NoEncontrado
            );
        }


        if (dieta.Estado ==
            EstadoDieta.Archivada)
        {
            return Error<ComidaDetalleDto>(
                "No se puede modificar una dieta archivada.",
                TipoErrorDieta.Validacion
            );
        }


        if (!Enum.IsDefined(dto.Tipo))
        {
            return Error<ComidaDetalleDto>(
                "El tipo de comida no es válido.",
                TipoErrorDieta.Validacion
            );
        }


        var comida =
            new Comida
            {
                DietaId =
                    dietaId,

                Nombre =
                    dto.Nombre.Trim(),

                Tipo =
                    dto.Tipo,

                Orden =
                    dto.Orden,

                Observaciones =
                    dto.Observaciones?.Trim()
            };


        _context.Comidas
            .Add(comida);


        await _context.SaveChangesAsync();


        return new ResultadoDieta<ComidaDetalleDto>
        {
            Exitoso = true,

            Datos =
                MapearComida(
                    comida,
                    new List<SeccionComidaDto>()
                ),

            TipoError =
                TipoErrorDieta.Ninguno
        };
    }


    // ==========================================
    // LISTAR COMIDAS
    // ==========================================

    public async Task<
        ResultadoDieta<List<ComidaListadoDto>>>
        ObtenerComidasAsync(
            int nutricionistaId,
            int pacienteId,
            int dietaId)
    {
        var dietaExiste =
            await _context.Dietas
                .AsNoTracking()
                .AnyAsync(d =>
                    d.Id == dietaId
                    &&
                    d.PacienteId == pacienteId
                    &&
                    d.Paciente.NutricionistaId ==
                    nutricionistaId
                );


        if (!dietaExiste)
        {
            return Error<List<ComidaListadoDto>>(
                "Dieta no encontrada.",
                TipoErrorDieta.NoEncontrado
            );
        }


        var comidas =
            await _context.Comidas
                .AsNoTracking()
                .Where(c =>
                    c.DietaId == dietaId
                )
                .OrderBy(c =>
                    c.Orden
                )
                .ThenBy(c =>
                    c.Id
                )
                .Select(c =>
                    new ComidaListadoDto
                    {
                        Id =
                            c.Id,

                        Nombre =
                            c.Nombre,

                        Tipo =
                            c.Tipo.ToString(),

                        Orden =
                            c.Orden,

                        CantidadSecciones =
                            c.Secciones.Count
                    }
                )
                .ToListAsync();


        return new ResultadoDieta<
            List<ComidaListadoDto>>
        {
            Exitoso = true,

            Datos =
                comidas,

            TipoError =
                TipoErrorDieta.Ninguno
        };
    }


    // ==========================================
    // DETALLE COMIDA
    // ==========================================

    public async Task<
        ResultadoDieta<ComidaDetalleDto>>
        ObtenerComidaAsync(
            int nutricionistaId,
            int pacienteId,
            int dietaId,
            int comidaId)
    {
        /*
         * Para calcular los totales necesitamos:
         *
         * Comida
         *   -> Secciones
         *      -> Opciones
         *         -> Items
         *            -> Alimento
         *
         * El cálculo se realiza después de obtener
         * las entidades, no dentro del SELECT SQL.
         */

        var comida =
            await _context.Comidas
                .AsNoTracking()

                .Include(c =>
                    c.Secciones
                )
                    .ThenInclude(s =>
                        s.Opciones
                    )
                        .ThenInclude(o =>
                            o.Items
                        )
                            .ThenInclude(i =>
                                i.Alimento
                            )

                .Where(c =>
                    c.Id == comidaId
                    &&
                    c.DietaId == dietaId
                    &&
                    c.Dieta.PacienteId ==
                    pacienteId
                    &&
                    c.Dieta.Paciente
                        .NutricionistaId ==
                    nutricionistaId
                )

                .AsSplitQuery()

                .FirstOrDefaultAsync();


        if (comida is null)
        {
            return Error<ComidaDetalleDto>(
                "Comida no encontrada.",
                TipoErrorDieta.NoEncontrado
            );
        }


        var secciones =
            comida.Secciones
                .OrderBy(s =>
                    s.Orden
                )
                .ThenBy(s =>
                    s.Id
                )
                .Select(s =>
                    MapearSeccion(s)
                )
                .ToList();


        return new ResultadoDieta<ComidaDetalleDto>
        {
            Exitoso = true,

            Datos =
                MapearComida(
                    comida,
                    secciones
                ),

            TipoError =
                TipoErrorDieta.Ninguno
        };
    }


    // ==========================================
    // EDITAR COMIDA
    // ==========================================

    public async Task<
        ResultadoDieta<ComidaDetalleDto>>
        EditarComidaAsync(
            int nutricionistaId,
            int pacienteId,
            int dietaId,
            int comidaId,
            EditarComidaDto dto)
    {
        var comida =
            await _context.Comidas

                .Include(c =>
                    c.Dieta
                )

                .Include(c =>
                    c.Secciones
                )
                    .ThenInclude(s =>
                        s.Opciones
                    )
                        .ThenInclude(o =>
                            o.Items
                        )
                            .ThenInclude(i =>
                                i.Alimento
                            )

                .AsSplitQuery()

                .FirstOrDefaultAsync(c =>
                    c.Id == comidaId
                    &&
                    c.DietaId == dietaId
                    &&
                    c.Dieta.PacienteId ==
                    pacienteId
                    &&
                    c.Dieta.Paciente
                        .NutricionistaId ==
                    nutricionistaId
                );


        if (comida is null)
        {
            return Error<ComidaDetalleDto>(
                "Comida no encontrada.",
                TipoErrorDieta.NoEncontrado
            );
        }


        if (comida.Dieta.Estado ==
            EstadoDieta.Archivada)
        {
            return Error<ComidaDetalleDto>(
                "No se puede modificar una dieta archivada.",
                TipoErrorDieta.Validacion
            );
        }


        if (!Enum.IsDefined(dto.Tipo))
        {
            return Error<ComidaDetalleDto>(
                "El tipo de comida no es válido.",
                TipoErrorDieta.Validacion
            );
        }


        comida.Nombre =
            dto.Nombre.Trim();

        comida.Tipo =
            dto.Tipo;

        comida.Orden =
            dto.Orden;

        comida.Observaciones =
            dto.Observaciones?.Trim();


        await _context.SaveChangesAsync();


        var secciones =
            comida.Secciones
                .OrderBy(s =>
                    s.Orden
                )
                .ThenBy(s =>
                    s.Id
                )
                .Select(s =>
                    MapearSeccion(s)
                )
                .ToList();


        return new ResultadoDieta<ComidaDetalleDto>
        {
            Exitoso = true,

            Datos =
                MapearComida(
                    comida,
                    secciones
                ),

            TipoError =
                TipoErrorDieta.Ninguno
        };
    }


    // ==========================================
    // CREAR SECCIÓN
    // ==========================================

    public async Task<
        ResultadoDieta<SeccionComidaDto>>
        CrearSeccionAsync(
            int nutricionistaId,
            int pacienteId,
            int dietaId,
            int comidaId,
            CrearSeccionComidaDto dto)
    {
        var comida =
            await _context.Comidas
                .Include(c =>
                    c.Dieta
                )
                .FirstOrDefaultAsync(c =>
                    c.Id == comidaId
                    &&
                    c.DietaId == dietaId
                    &&
                    c.Dieta.PacienteId ==
                    pacienteId
                    &&
                    c.Dieta.Paciente
                        .NutricionistaId ==
                    nutricionistaId
                );


        if (comida is null)
        {
            return Error<SeccionComidaDto>(
                "Comida no encontrada.",
                TipoErrorDieta.NoEncontrado
            );
        }


        if (comida.Dieta.Estado ==
            EstadoDieta.Archivada)
        {
            return Error<SeccionComidaDto>(
                "No se puede modificar una dieta archivada.",
                TipoErrorDieta.Validacion
            );
        }


        if (!Enum.IsDefined(dto.Tipo))
        {
            return Error<SeccionComidaDto>(
                "El tipo de sección no es válido.",
                TipoErrorDieta.Validacion
            );
        }


        var seccion =
            new SeccionComida
            {
                ComidaId =
                    comidaId,

                Nombre =
                    dto.Nombre.Trim(),

                Tipo =
                    dto.Tipo,

                Orden =
                    dto.Orden,

                Observaciones =
                    dto.Observaciones?.Trim()
            };


        _context.SeccionesComidas
            .Add(seccion);


        await _context.SaveChangesAsync();


        return new ResultadoDieta<SeccionComidaDto>
        {
            Exitoso = true,

            Datos =
                MapearSeccion(
                    seccion
                ),

            TipoError =
                TipoErrorDieta.Ninguno
        };
    }


    // ==========================================
    // LISTAR SECCIONES
    // ==========================================

    public async Task<
        ResultadoDieta<List<SeccionComidaDto>>>
        ObtenerSeccionesAsync(
            int nutricionistaId,
            int pacienteId,
            int dietaId,
            int comidaId)
    {
        var comidaExiste =
            await _context.Comidas
                .AsNoTracking()
                .AnyAsync(c =>
                    c.Id == comidaId
                    &&
                    c.DietaId == dietaId
                    &&
                    c.Dieta.PacienteId ==
                    pacienteId
                    &&
                    c.Dieta.Paciente
                        .NutricionistaId ==
                    nutricionistaId
                );


        if (!comidaExiste)
        {
            return Error<
                List<SeccionComidaDto>>(
                "Comida no encontrada.",
                TipoErrorDieta.NoEncontrado
            );
        }


        var secciones =
            await _context.SeccionesComidas
                .AsNoTracking()
                .Where(s =>
                    s.ComidaId == comidaId
                )
                .OrderBy(s =>
                    s.Orden
                )
                .ThenBy(s =>
                    s.Id
                )
                .Select(s =>
                    new SeccionComidaDto
                    {
                        Id =
                            s.Id,

                        Nombre =
                            s.Nombre,

                        Tipo =
                            s.Tipo.ToString(),

                        Orden =
                            s.Orden,

                        Observaciones =
                            s.Observaciones,

                        CantidadOpciones =
                            s.Opciones.Count
                    }
                )
                .ToListAsync();


        return new ResultadoDieta<
            List<SeccionComidaDto>>
        {
            Exitoso = true,

            Datos =
                secciones,

            TipoError =
                TipoErrorDieta.Ninguno
        };
    }


    // ==========================================
    // EDITAR SECCIÓN
    // ==========================================

    public async Task<
        ResultadoDieta<SeccionComidaDto>>
        EditarSeccionAsync(
            int nutricionistaId,
            int pacienteId,
            int dietaId,
            int comidaId,
            int seccionId,
            EditarSeccionComidaDto dto)
    {
        var seccion =
            await _context.SeccionesComidas
                .Include(s =>
                    s.Comida
                )
                    .ThenInclude(c =>
                        c.Dieta
                    )
                .Include(s =>
                    s.Opciones
                )
                .FirstOrDefaultAsync(s =>
                    s.Id == seccionId
                    &&
                    s.ComidaId == comidaId
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


        if (seccion is null)
        {
            return Error<SeccionComidaDto>(
                "Sección no encontrada.",
                TipoErrorDieta.NoEncontrado
            );
        }


        if (seccion.Comida.Dieta.Estado ==
            EstadoDieta.Archivada)
        {
            return Error<SeccionComidaDto>(
                "No se puede modificar una dieta archivada.",
                TipoErrorDieta.Validacion
            );
        }


        if (!Enum.IsDefined(dto.Tipo))
        {
            return Error<SeccionComidaDto>(
                "El tipo de sección no es válido.",
                TipoErrorDieta.Validacion
            );
        }


        seccion.Nombre =
            dto.Nombre.Trim();

        seccion.Tipo =
            dto.Tipo;

        seccion.Orden =
            dto.Orden;

        seccion.Observaciones =
            dto.Observaciones?.Trim();


        await _context.SaveChangesAsync();


        return new ResultadoDieta<SeccionComidaDto>
        {
            Exitoso = true,

            Datos =
                MapearSeccion(
                    seccion
                ),

            TipoError =
                TipoErrorDieta.Ninguno
        };
    }


    // ==========================================
    // MAPPERS
    // ==========================================

    private static ComidaDetalleDto
        MapearComida(
            Comida comida,
            List<SeccionComidaDto> secciones)
    {
        /*
         * Para el total de la comida:
         *
         * - recorremos todas las secciones
         * - de cada sección tomamos solamente
         *   la opción predeterminada
         * - sumamos sus items
         */

        var resultadoTotales =
            CalculadoraTotalesNutricionales
                .CalcularSecciones(
                    comida.Secciones
                );


        return new ComidaDetalleDto
        {
            Id =
                comida.Id,

            DietaId =
                comida.DietaId,

            Nombre =
                comida.Nombre,

            Tipo =
                comida.Tipo.ToString(),

            Orden =
                comida.Orden,

            Observaciones =
                comida.Observaciones,

            Secciones =
                secciones,

            Totales =
                MapearTotales(
                    resultadoTotales
                )
        };
    }


    private static SeccionComidaDto
        MapearSeccion(
            SeccionComida seccion)
    {
        return new SeccionComidaDto
        {
            Id =
                seccion.Id,

            Nombre =
                seccion.Nombre,

            Tipo =
                seccion.Tipo.ToString(),

            Orden =
                seccion.Orden,

            Observaciones =
                seccion.Observaciones,

            CantidadOpciones =
                seccion.Opciones?.Count
                ??
                0
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
                resultado
                    .SeccionesSinOpcionPredeterminada,

            EsCompleto =
                resultado.EsCompleto
        };
    }


    // ==========================================
    // ERROR GENÉRICO
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