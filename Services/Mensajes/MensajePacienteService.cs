using Microsoft.EntityFrameworkCore;
using NutriApi.DTOs.Mensajes;
using NutriApi.Services.Notificaciones;
using NutriApp.Data;
using NutriApp.Enums.Notificaciones;
using NutriApp.Models.Mensajes;

namespace NutriApi.Services.Mensajes;

public class MensajePacienteService : IMensajePacienteService
{
    private const string RecursoMensajePaciente = "MensajePaciente";

    private readonly NutriAppDbContext _context;

    private readonly INotificacionService _notificacionService;

    private readonly ILogger<MensajePacienteService> _logger;

    public MensajePacienteService(
        NutriAppDbContext context,
        INotificacionService notificacionService,
        ILogger<MensajePacienteService> logger
    )
    {
        _context = context;

        _notificacionService = notificacionService;

        _logger = logger;
    }

    // ==========================================
    // NUTRICIONISTA - CREAR
    // ==========================================

    public async Task<ResultadoMensajePaciente<MensajePacienteDto>> CrearAsync(
        int nutricionistaId,
        int pacienteId,
        CrearMensajePacienteDto dto
    )
    {
        var paciente = await _context
            .Pacientes.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == pacienteId && p.NutricionistaId == nutricionistaId);

        if (paciente is null)
        {
            return Error<MensajePacienteDto>(
                "Paciente no encontrado.",
                TipoErrorMensajePaciente.NoEncontrado
            );
        }

        var errorValidacion = ValidarContenido(dto.Titulo, dto.Contenido);

        if (errorValidacion is not null)
        {
            return Error<MensajePacienteDto>(errorValidacion, TipoErrorMensajePaciente.Validacion);
        }

        var mensaje = new MensajePaciente
        {
            NutricionistaId = nutricionistaId,

            PacienteId = pacienteId,

            Titulo = dto.Titulo.Trim(),

            Contenido = dto.Contenido.Trim(),

            FechaCreacion = DateTime.UtcNow,

            Leido = false,
        };

        _context.MensajesPacientes.Add(mensaje);

        await _context.SaveChangesAsync();

        // ======================================
        // NOTIFICACIÓN
        // ======================================

        try
        {
            await _notificacionService.CrearAsync(
                pacienteId,
                TipoNotificacion.NuevoMensajeNutricionista,
                "Nuevo mensaje de tu nutricionista",
                mensaje.Titulo,
                RecursoMensajePaciente,
                mensaje.Id
            );
        }
        catch (Exception ex)
        {
            /*
             * El mensaje ya fue persistido.
             *
             * Una falla de notificación no debe
             * provocar que el envío del mensaje
             * aparezca como fallido.
             */

            _logger.LogError(
                ex,
                "No se pudo crear la notificación del mensaje {MensajeId}.",
                mensaje.Id
            );
        }

        return Exito(Mapear(mensaje));
    }

    // ==========================================
    // NUTRICIONISTA - MENSAJES DEL PACIENTE
    // ==========================================

    public async Task<ResultadoMensajePaciente<List<MensajePacienteDto>>> ObtenerPacienteAsync(
        int nutricionistaId,
        int pacienteId
    )
    {
        var pertenece = await PacientePerteneceAsync(nutricionistaId, pacienteId);

        if (!pertenece)
        {
            return Error<List<MensajePacienteDto>>(
                "Paciente no encontrado.",
                TipoErrorMensajePaciente.NoEncontrado
            );
        }

        var mensajes = await _context
            .MensajesPacientes.AsNoTracking()
            .Where(m => m.PacienteId == pacienteId && m.NutricionistaId == nutricionistaId)
            .OrderByDescending(m => m.FechaCreacion)
            .ToListAsync();

        return Exito(mensajes.Select(Mapear).ToList());
    }

    // ==========================================
    // PACIENTE - MIS MENSAJES
    // ==========================================

    public async Task<ResultadoMensajePaciente<List<MensajePacienteDto>>> ObtenerPropiosAsync(
        int pacienteId
    )
    {
        var pacienteExiste = await _context
            .Pacientes.AsNoTracking()
            .AnyAsync(p => p.Id == pacienteId);

        if (!pacienteExiste)
        {
            return Error<List<MensajePacienteDto>>(
                "Paciente no encontrado.",
                TipoErrorMensajePaciente.NoEncontrado
            );
        }

        var mensajes = await _context
            .MensajesPacientes.AsNoTracking()
            .Where(m => m.PacienteId == pacienteId)
            .OrderByDescending(m => m.FechaCreacion)
            .ToListAsync();

        return Exito(mensajes.Select(Mapear).ToList());
    }

    // ==========================================
    // PACIENTE - DETALLE
    // ==========================================

    public async Task<ResultadoMensajePaciente<MensajePacienteDto>> ObtenerDetallePacienteAsync(
        int pacienteId,
        int mensajeId
    )
    {
        var mensaje = await _context
            .MensajesPacientes.AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == mensajeId && m.PacienteId == pacienteId);

        if (mensaje is null)
        {
            return Error<MensajePacienteDto>(
                "Mensaje no encontrado.",
                TipoErrorMensajePaciente.NoEncontrado
            );
        }

        return Exito(Mapear(mensaje));
    }

    // ==========================================
    // NUTRICIONISTA - EDITAR
    // ==========================================

    public async Task<ResultadoMensajePaciente<MensajePacienteDto>> EditarAsync(
        int nutricionistaId,
        int pacienteId,
        int mensajeId,
        EditarMensajePacienteDto dto
    )
    {
        var mensaje = await _context.MensajesPacientes.FirstOrDefaultAsync(m =>
            m.Id == mensajeId && m.PacienteId == pacienteId && m.NutricionistaId == nutricionistaId
        );

        if (mensaje is null)
        {
            return Error<MensajePacienteDto>(
                "Mensaje no encontrado.",
                TipoErrorMensajePaciente.NoEncontrado
            );
        }

        if (mensaje.Leido)
        {
            return Error<MensajePacienteDto>(
                "No se puede editar un mensaje que el paciente ya leyó.",
                TipoErrorMensajePaciente.Conflicto
            );
        }

        var errorValidacion = ValidarContenido(dto.Titulo, dto.Contenido);

        if (errorValidacion is not null)
        {
            return Error<MensajePacienteDto>(errorValidacion, TipoErrorMensajePaciente.Validacion);
        }

        mensaje.Titulo = dto.Titulo.Trim();

        mensaje.Contenido = dto.Contenido.Trim();

        mensaje.FechaActualizacion = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // ======================================
        // ACTUALIZAR NOTIFICACIÓN
        // ======================================

        try
        {
            await _notificacionService.ActualizarContenidoPorRecursoAsync(
                pacienteId,
                RecursoMensajePaciente,
                mensaje.Id,
                "Nuevo mensaje de tu nutricionista",
                mensaje.Titulo
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "No se pudo actualizar la notificación del mensaje {MensajeId}.",
                mensaje.Id
            );
        }

        return Exito(Mapear(mensaje));
    }

    // ==========================================
    // NUTRICIONISTA - ELIMINAR
    // ==========================================

    public async Task<ResultadoMensajePaciente<bool>> EliminarAsync(
        int nutricionistaId,
        int pacienteId,
        int mensajeId
    )
    {
        var mensaje = await _context.MensajesPacientes.FirstOrDefaultAsync(m =>
            m.Id == mensajeId && m.PacienteId == pacienteId && m.NutricionistaId == nutricionistaId
        );

        if (mensaje is null)
        {
            return Error<bool>("Mensaje no encontrado.", TipoErrorMensajePaciente.NoEncontrado);
        }

        if (mensaje.Leido)
        {
            return Error<bool>(
                "No se puede eliminar un mensaje que el paciente ya leyó.",
                TipoErrorMensajePaciente.Conflicto
            );
        }

        _context.MensajesPacientes.Remove(mensaje);

        await _context.SaveChangesAsync();

        // ======================================
        // ELIMINAR NOTIFICACIÓN
        // ======================================

        try
        {
            await _notificacionService.EliminarPorRecursoAsync(
                pacienteId,
                RecursoMensajePaciente,
                mensaje.Id
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "No se pudo eliminar la notificación del mensaje {MensajeId}.",
                mensaje.Id
            );
        }

        return Exito(true);
    }

    // ==========================================
    // PACIENTE - MARCAR COMO LEÍDO
    // ==========================================

    public async Task<ResultadoMensajePaciente<MensajePacienteDto>> MarcarComoLeidoAsync(
        int pacienteId,
        int mensajeId
    )
    {
        var mensaje = await _context.MensajesPacientes.FirstOrDefaultAsync(m =>
            m.Id == mensajeId && m.PacienteId == pacienteId
        );

        if (mensaje is null)
        {
            return Error<MensajePacienteDto>(
                "Mensaje no encontrado.",
                TipoErrorMensajePaciente.NoEncontrado
            );
        }

        if (!mensaje.Leido)
        {
            mensaje.Leido = true;

            mensaje.FechaLectura = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        // ======================================
        // SINCRONIZAR NOTIFICACIÓN
        // ======================================

        try
        {
            await _notificacionService.MarcarComoLeidaPorRecursoAsync(
                pacienteId,
                RecursoMensajePaciente,
                mensaje.Id
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "No se pudo marcar como leída la notificación del mensaje {MensajeId}.",
                mensaje.Id
            );
        }

        return Exito(Mapear(mensaje));
    }

    // ==========================================
    // OWNERSHIP
    // ==========================================

    private async Task<bool> PacientePerteneceAsync(int nutricionistaId, int pacienteId)
    {
        return await _context
            .Pacientes.AsNoTracking()
            .AnyAsync(p => p.Id == pacienteId && p.NutricionistaId == nutricionistaId);
    }

    // ==========================================
    // VALIDACIÓN
    // ==========================================

    private static string? ValidarContenido(string? titulo, string? contenido)
    {
        if (string.IsNullOrWhiteSpace(titulo))
        {
            return "El título es obligatorio.";
        }

        if (titulo.Trim().Length > 150)
        {
            return "El título no puede superar los 150 caracteres.";
        }

        if (string.IsNullOrWhiteSpace(contenido))
        {
            return "El contenido del mensaje es obligatorio.";
        }

        if (contenido.Trim().Length > 2000)
        {
            return "El mensaje no puede superar los 2000 caracteres.";
        }

        return null;
    }

    // ==========================================
    // MAPPER
    // ==========================================

    private static MensajePacienteDto Mapear(MensajePaciente mensaje)
    {
        return new MensajePacienteDto
        {
            Id = mensaje.Id,

            PacienteId = mensaje.PacienteId,

            Titulo = mensaje.Titulo,

            Contenido = mensaje.Contenido,

            FechaCreacion = mensaje.FechaCreacion,

            FechaActualizacion = mensaje.FechaActualizacion,

            Leido = mensaje.Leido,

            FechaLectura = mensaje.FechaLectura,
        };
    }

    // ==========================================
    // RESULTADOS
    // ==========================================

    private static ResultadoMensajePaciente<T> Exito<T>(T datos)
    {
        return new ResultadoMensajePaciente<T>
        {
            Exitoso = true,

            Datos = datos,

            TipoError = TipoErrorMensajePaciente.Ninguno,
        };
    }

    private static ResultadoMensajePaciente<T> Error<T>(
        string mensaje,
        TipoErrorMensajePaciente tipo
    )
    {
        return new ResultadoMensajePaciente<T>
        {
            Exitoso = false,

            Error = mensaje,

            TipoError = tipo,
        };
    }
}
