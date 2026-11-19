using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using NutriApi.DTOs.Competencias;
using NutriApp.Data;
using NutriApp.Models.Competencias;

namespace NutriApi.Services.Competencias;

public class CompetenciaService : ICompetenciaService
{
    private readonly NutriAppDbContext _context;

    private static readonly Expression<Func<Competencia, CompetenciaDto>> ProyeccionProfesional =
        c => new CompetenciaDto
        {
            Id = c.Id,
            PacienteId = c.PacienteId,
            PacienteNombre = c.Paciente.Nombre + " " + c.Paciente.Apellido,
            Nombre = c.Nombre,
            Disciplina = c.Disciplina,
            FechaInicio = c.FechaInicio,
            FechaFin = c.FechaFin,
            Lugar = c.Lugar,
            Observaciones = c.Observaciones,
            Estado = c.Estado,
            FechaCreacion = c.FechaCreacion,
            FechaActualizacion = c.FechaActualizacion,
        };

    private static readonly Expression<
        Func<Competencia, CompetenciaPacienteDto>
    > ProyeccionPaciente = c => new CompetenciaPacienteDto
    {
        Id = c.Id,
        Nombre = c.Nombre,
        Disciplina = c.Disciplina,
        FechaInicio = c.FechaInicio,
        FechaFin = c.FechaFin,
        Lugar = c.Lugar,
        Estado = c.Estado,
    };

    public CompetenciaService(NutriAppDbContext context)
    {
        _context = context;
    }

    // ==========================================
    // CREAR
    // ==========================================

    public async Task<ResultadoCompetencia<CompetenciaDto>> CrearAsync(
        int nutricionistaId,
        int pacienteId,
        CrearCompetenciaDto dto
    )
    {
        var paciente = await _context
            .Pacientes.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == pacienteId && p.NutricionistaId == nutricionistaId);

        if (paciente is null)
        {
            return Error<CompetenciaDto>(
                "Paciente no encontrado.",
                TipoErrorCompetencia.NoEncontrado
            );
        }

        var error = ValidarDatos(
            dto.Nombre,
            dto.Disciplina,
            dto.FechaInicio,
            dto.FechaFin,
            dto.Lugar,
            dto.Observaciones
        );

        if (error is not null)
        {
            return Error<CompetenciaDto>(error, TipoErrorCompetencia.Validacion);
        }

        var competencia = new Competencia
        {
            PacienteId = pacienteId,

            Nombre = dto.Nombre.Trim(),

            Disciplina = dto.Disciplina.Trim(),

            FechaInicio = dto.FechaInicio,

            FechaFin = dto.FechaFin,

            Lugar = LimpiarOpcional(dto.Lugar),

            Observaciones = LimpiarOpcional(dto.Observaciones),

            Estado = EstadoCompetencia.Programada,

            FechaCreacion = DateTime.UtcNow,
        };

        _context.Competencias.Add(competencia);

        await _context.SaveChangesAsync();

        return Exito(MapearProfesional(competencia, paciente.Nombre, paciente.Apellido));
    }

    // ==========================================
    // EDITAR
    // ==========================================

    public async Task<ResultadoCompetencia<CompetenciaDto>> EditarAsync(
        int nutricionistaId,
        int pacienteId,
        int competenciaId,
        EditarCompetenciaDto dto
    )
    {
        var competencia = await BuscarCompetenciaProfesionalAsync(
            nutricionistaId,
            pacienteId,
            competenciaId
        );

        if (competencia is null)
        {
            return Error<CompetenciaDto>(
                "Competencia no encontrada.",
                TipoErrorCompetencia.NoEncontrado
            );
        }

        if (competencia.Estado != EstadoCompetencia.Programada)
        {
            return Error<CompetenciaDto>(
                "Solo se pueden editar competencias programadas.",
                TipoErrorCompetencia.Conflicto
            );
        }

        var error = ValidarDatos(
            dto.Nombre,
            dto.Disciplina,
            dto.FechaInicio,
            dto.FechaFin,
            dto.Lugar,
            dto.Observaciones
        );

        if (error is not null)
        {
            return Error<CompetenciaDto>(error, TipoErrorCompetencia.Validacion);
        }

        competencia.Nombre = dto.Nombre.Trim();

        competencia.Disciplina = dto.Disciplina.Trim();

        competencia.FechaInicio = dto.FechaInicio;

        competencia.FechaFin = dto.FechaFin;

        competencia.Lugar = LimpiarOpcional(dto.Lugar);

        competencia.Observaciones = LimpiarOpcional(dto.Observaciones);

        competencia.FechaActualizacion = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Exito(
            MapearProfesional(
                competencia,
                competencia.Paciente.Nombre,
                competencia.Paciente.Apellido
            )
        );
    }

    // ==========================================
    // CANCELAR
    // ==========================================

    public async Task<ResultadoCompetencia<CompetenciaDto>> CancelarAsync(
        int nutricionistaId,
        int pacienteId,
        int competenciaId
    )
    {
        return await CambiarEstadoAsync(
            nutricionistaId,
            pacienteId,
            competenciaId,
            EstadoCompetencia.Cancelada
        );
    }

    // ==========================================
    // MARCAR COMO REALIZADA
    // ==========================================

    public async Task<ResultadoCompetencia<CompetenciaDto>> MarcarRealizadaAsync(
        int nutricionistaId,
        int pacienteId,
        int competenciaId
    )
    {
        return await CambiarEstadoAsync(
            nutricionistaId,
            pacienteId,
            competenciaId,
            EstadoCompetencia.Realizada
        );
    }

    // ==========================================
    // NUTRICIONISTA - COMPETENCIAS DEL PACIENTE
    // ==========================================

    public async Task<ResultadoCompetencia<List<CompetenciaDto>>> ObtenerPacienteAsync(
        int nutricionistaId,
        int pacienteId
    )
    {
        var pertenece = await _context
            .Pacientes.AsNoTracking()
            .AnyAsync(p => p.Id == pacienteId && p.NutricionistaId == nutricionistaId);

        if (!pertenece)
        {
            return Error<List<CompetenciaDto>>(
                "Paciente no encontrado.",
                TipoErrorCompetencia.NoEncontrado
            );
        }

        var competencias = await _context
            .Competencias.AsNoTracking()
            .Where(c => c.PacienteId == pacienteId)
            .OrderBy(c => c.FechaInicio)
            .ThenBy(c => c.Id)
            .Select(ProyeccionProfesional)
            .ToListAsync();

        return Exito(competencias);
    }

    // ==========================================
    // NUTRICIONISTA - DETALLE
    // ==========================================

    public async Task<ResultadoCompetencia<CompetenciaDto>> ObtenerDetalleAsync(
        int nutricionistaId,
        int pacienteId,
        int competenciaId
    )
    {
        var competencia = await _context
            .Competencias.AsNoTracking()
            .Where(c =>
                c.Id == competenciaId
                && c.PacienteId == pacienteId
                && c.Paciente.NutricionistaId == nutricionistaId
            )
            .Select(ProyeccionProfesional)
            .FirstOrDefaultAsync();

        return competencia is null
            ? Error<CompetenciaDto>("Competencia no encontrada.", TipoErrorCompetencia.NoEncontrado)
            : Exito(competencia);
    }

    // ==========================================
    // PACIENTE - MIS COMPETENCIAS
    // ==========================================

    public async Task<ResultadoCompetencia<List<CompetenciaPacienteDto>>> ObtenerPropiosAsync(
        int pacienteId
    )
    {
        var existe = await _context.Pacientes.AsNoTracking().AnyAsync(p => p.Id == pacienteId);

        if (!existe)
        {
            return Error<List<CompetenciaPacienteDto>>(
                "Paciente no encontrado.",
                TipoErrorCompetencia.NoEncontrado
            );
        }

        var competencias = await _context
            .Competencias.AsNoTracking()
            .Where(c => c.PacienteId == pacienteId)
            .OrderBy(c => c.FechaInicio)
            .ThenBy(c => c.Id)
            .Select(ProyeccionPaciente)
            .ToListAsync();

        return Exito(competencias);
    }

    // ==========================================
    // CAMBIAR ESTADO
    // ==========================================

    private async Task<ResultadoCompetencia<CompetenciaDto>> CambiarEstadoAsync(
        int nutricionistaId,
        int pacienteId,
        int competenciaId,
        EstadoCompetencia nuevoEstado
    )
    {
        var competencia = await BuscarCompetenciaProfesionalAsync(
            nutricionistaId,
            pacienteId,
            competenciaId
        );

        if (competencia is null)
        {
            return Error<CompetenciaDto>(
                "Competencia no encontrada.",
                TipoErrorCompetencia.NoEncontrado
            );
        }

        if (competencia.Estado != EstadoCompetencia.Programada)
        {
            return Error<CompetenciaDto>(
                "Solo puede cambiarse el estado de competencias programadas.",
                TipoErrorCompetencia.Conflicto
            );
        }

        competencia.Estado = nuevoEstado;

        competencia.FechaActualizacion = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Exito(
            MapearProfesional(
                competencia,
                competencia.Paciente.Nombre,
                competencia.Paciente.Apellido
            )
        );
    }

    // ==========================================
    // BUSCAR COMPETENCIA AUTORIZADA
    // ==========================================

    private Task<Competencia?> BuscarCompetenciaProfesionalAsync(
        int nutricionistaId,
        int pacienteId,
        int competenciaId
    )
    {
        return _context
            .Competencias.Include(c => c.Paciente)
            .FirstOrDefaultAsync(c =>
                c.Id == competenciaId
                && c.PacienteId == pacienteId
                && c.Paciente.NutricionistaId == nutricionistaId
            );
    }

    // ==========================================
    // VALIDAR DATOS
    // ==========================================

    private static string? ValidarDatos(
        string? nombre,
        string? disciplina,
        DateOnly fechaInicio,
        DateOnly? fechaFin,
        string? lugar,
        string? observaciones
    )
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            return "El nombre de la competencia es obligatorio.";
        }

        if (nombre.Trim().Length > 150)
        {
            return "El nombre de la competencia no puede superar los 150 caracteres.";
        }

        if (string.IsNullOrWhiteSpace(disciplina))
        {
            return "La disciplina es obligatoria.";
        }

        if (disciplina.Trim().Length > 100)
        {
            return "La disciplina no puede superar los 100 caracteres.";
        }

        if (fechaInicio == default)
        {
            return "La fecha de inicio es obligatoria.";
        }

        if (fechaFin.HasValue && fechaFin.Value < fechaInicio)
        {
            return "La fecha de fin no puede ser anterior a la fecha de inicio.";
        }

        if (lugar?.Trim().Length > 200)
        {
            return "El lugar no puede superar los 200 caracteres.";
        }

        if (observaciones?.Trim().Length > 1000)
        {
            return "Las observaciones no pueden superar los 1000 caracteres.";
        }

        return null;
    }

    // ==========================================
    // LIMPIAR TEXTO OPCIONAL
    // ==========================================

    private static string? LimpiarOpcional(string? valor)
    {
        return string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
    }

    // ==========================================
    // MAPPER PROFESIONAL
    // ==========================================

    private static CompetenciaDto MapearProfesional(
        Competencia competencia,
        string nombrePaciente,
        string apellidoPaciente
    )
    {
        return new CompetenciaDto
        {
            Id = competencia.Id,

            PacienteId = competencia.PacienteId,

            PacienteNombre = nombrePaciente + " " + apellidoPaciente,

            Nombre = competencia.Nombre,

            Disciplina = competencia.Disciplina,

            FechaInicio = competencia.FechaInicio,

            FechaFin = competencia.FechaFin,

            Lugar = competencia.Lugar,

            Observaciones = competencia.Observaciones,

            Estado = competencia.Estado,

            FechaCreacion = competencia.FechaCreacion,

            FechaActualizacion = competencia.FechaActualizacion,
        };
    }

    // ==========================================
    // RESULTADOS
    // ==========================================

    private static ResultadoCompetencia<T> Exito<T>(T datos)
    {
        return new ResultadoCompetencia<T>
        {
            Exitoso = true,
            Datos = datos,
            TipoError = TipoErrorCompetencia.Ninguno,
        };
    }

    private static ResultadoCompetencia<T> Error<T>(string mensaje, TipoErrorCompetencia tipo)
    {
        return new ResultadoCompetencia<T>
        {
            Exitoso = false,
            Error = mensaje,
            TipoError = tipo,
        };
    }
}
