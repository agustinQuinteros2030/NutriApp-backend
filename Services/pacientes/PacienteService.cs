using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using NutriApi.DTOs.Pacientes;

using NutriApp.Data;
using NutriApp.Models.Pacientes;
using NutriApp.Models.Usuarios;

namespace NutriApi.Services.Pacientes;

public class PacienteService : IPacienteService
{
    private readonly NutriAppDbContext _context;

    private readonly UserManager<UsuarioAplicacion> _userManager;


    public PacienteService(
        NutriAppDbContext context,
        UserManager<UsuarioAplicacion> userManager)
    {
        _context = context;
        _userManager = userManager;
    }


    // ==========================================
    // CREAR
    // ==========================================

    public async Task<ResultadoPaciente<PacienteDetalleDto>>
        CrearAsync(
            int nutricionistaId,
            CrearPacienteDto dto)
    {
        var email =
            dto.Email.Trim().ToLowerInvariant();


        var usuarioExistente =
            await _userManager.FindByEmailAsync(email);


        if (usuarioExistente is not null)
        {
            return new ResultadoPaciente<PacienteDetalleDto>
            {
                Exitoso = false,
                Error = "Ya existe un usuario con ese email.",
                TipoError = TipoErrorPaciente.Validacion
            };
        }


        var paciente = new Paciente
        {
            Nombre = dto.Nombre.Trim(),

            Apellido = dto.Apellido.Trim(),

            Email = email,

            UserName = email,

            PhoneNumber = dto.Telefono?.Trim(),

            FechaNacimiento = dto.FechaNacimiento,

            NutricionistaId = nutricionistaId,

            Activo = true,

            FechaCreacion = DateTime.UtcNow
        };


        // Se crea como usuario Identity,
        // pero todavía SIN contraseña.

        var resultadoCreacion =
            await _userManager.CreateAsync(paciente);


        if (!resultadoCreacion.Succeeded)
        {
            return new ResultadoPaciente<PacienteDetalleDto>
            {
                Exitoso = false,

                Error = string.Join(
                    " | ",
                    resultadoCreacion.Errors
                        .Select(e => e.Description)
                ),

                TipoError = TipoErrorPaciente.Validacion
            };
        }


        var resultadoRol =
            await _userManager.AddToRoleAsync(
                paciente,
                Roles.Paciente
            );


        if (!resultadoRol.Succeeded)
        {
            await _userManager.DeleteAsync(paciente);

            return new ResultadoPaciente<PacienteDetalleDto>
            {
                Exitoso = false,
                Error = "No se pudo asignar el rol Paciente.",
                TipoError = TipoErrorPaciente.ErrorInterno
            };
        }


        var perfil = new PerfilPaciente
        {
            PacienteId = paciente.Id,

            ObjetivoNutricional =
                dto.ObjetivoNutricional,

            TipoActividad =
                dto.TipoActividad,

            PesoInicial =
                dto.PesoInicial,

            Altura =
                dto.Altura,

            FechaInicio =
                dto.FechaInicio ?? DateOnly.FromDateTime(DateTime.UtcNow),

            ActividadDescripcion =
                dto.ActividadDescripcion?.Trim(),

            ObservacionesGenerales =
                dto.ObservacionesGenerales?.Trim(),

            FechaCreacion =
                DateTime.UtcNow
        };


        try
        {
            _context.PerfilesPacientes.Add(perfil);

            await _context.SaveChangesAsync();
        }
        catch
        {
            // Si falla el perfil, no dejamos
            // un usuario paciente a medias.

            await _userManager.DeleteAsync(paciente);

            throw;
        }


        return new ResultadoPaciente<PacienteDetalleDto>
        {
            Exitoso = true,

            TipoError = TipoErrorPaciente.Ninguno,

            Datos = MapearDetalle(
                paciente,
                perfil
            )
        };
    }


    // ==========================================
    // LISTAR
    // ==========================================

    public async Task<List<PacienteListadoDto>>
        ObtenerTodosAsync(
            int nutricionistaId,
            string? buscar)
    {
        var query =
            _context.Pacientes
                .AsNoTracking()
                .Where(p =>
                    p.NutricionistaId ==
                    nutricionistaId
                );


        if (!string.IsNullOrWhiteSpace(buscar))
        {
            var termino = buscar.Trim();

            query = query.Where(p =>
                EF.Functions.ILike(
                    p.Nombre,
                    $"%{termino}%"
                )
                ||
                EF.Functions.ILike(
                    p.Apellido,
                    $"%{termino}%"
                )
                ||
                (
                    p.Email != null &&
                    EF.Functions.ILike(
                        p.Email,
                        $"%{termino}%"
                    )
                )
            );
        }


        return await query
            .OrderBy(p => p.Apellido)
            .ThenBy(p => p.Nombre)
            .Select(p => new PacienteListadoDto
            {
                Id = p.Id,

                Nombre = p.Nombre,

                Apellido = p.Apellido,

                Email = p.Email ?? string.Empty,

                Telefono = p.PhoneNumber,

                Activo = p.Activo,

                ObjetivoNutricional =
                    p.Perfil != null
                        ? p.Perfil.ObjetivoNutricional.ToString()
                        : string.Empty
            })
            .ToListAsync();
    }


    // ==========================================
    // DETALLE
    // ==========================================

    public async Task<ResultadoPaciente<PacienteDetalleDto>>
        ObtenerPorIdAsync(
            int nutricionistaId,
            int pacienteId)
    {
        var paciente =
            await _context.Pacientes
                .AsNoTracking()
                .Include(p => p.Perfil)
                .FirstOrDefaultAsync(p =>
                    p.Id == pacienteId
                    &&
                    p.NutricionistaId ==
                    nutricionistaId
                );


        if (paciente is null)
        {
            return NoEncontrado();
        }


        return new ResultadoPaciente<PacienteDetalleDto>
        {
            Exitoso = true,

            Datos =
                MapearDetalle(
                    paciente,
                    paciente.Perfil
                ),

            TipoError =
                TipoErrorPaciente.Ninguno
        };
    }


    // ==========================================
    // EDITAR
    // ==========================================

    public async Task<ResultadoPaciente<PacienteDetalleDto>>
        EditarAsync(
            int nutricionistaId,
            int pacienteId,
            EditarPacienteDto dto)
    {
        var paciente =
            await _context.Pacientes
                .Include(p => p.Perfil)
                .FirstOrDefaultAsync(p =>
                    p.Id == pacienteId
                    &&
                    p.NutricionistaId ==
                    nutricionistaId
                );


        if (paciente is null)
        {
            return NoEncontrado();
        }


        var email =
            dto.Email.Trim().ToLowerInvariant();


        if (!string.Equals(
                paciente.Email,
                email,
                StringComparison.OrdinalIgnoreCase))
        {
            var usuarioConEmail =
                await _userManager
                    .FindByEmailAsync(email);


            if (usuarioConEmail is not null &&
                usuarioConEmail.Id != paciente.Id)
            {
                return new ResultadoPaciente<PacienteDetalleDto>
                {
                    Exitoso = false,
                    Error = "Ya existe un usuario con ese email.",
                    TipoError = TipoErrorPaciente.Validacion
                };
            }


            var resultadoEmail =
                await _userManager
                    .SetEmailAsync(
                        paciente,
                        email
                    );


            if (!resultadoEmail.Succeeded)
            {
                return new ResultadoPaciente<PacienteDetalleDto>
                {
                    Exitoso = false,
                    Error = "No se pudo actualizar el email.",
                    TipoError = TipoErrorPaciente.Validacion
                };
            }


            var resultadoUsername =
                await _userManager
                    .SetUserNameAsync(
                        paciente,
                        email
                    );


            if (!resultadoUsername.Succeeded)
            {
                return new ResultadoPaciente<PacienteDetalleDto>
                {
                    Exitoso = false,
                    Error = "No se pudo actualizar el usuario.",
                    TipoError = TipoErrorPaciente.Validacion
                };
            }
        }


        paciente.Nombre =
            dto.Nombre.Trim();

        paciente.Apellido =
            dto.Apellido.Trim();

        paciente.PhoneNumber =
            dto.Telefono?.Trim();

        paciente.FechaNacimiento =
            dto.FechaNacimiento;


        if (paciente.Perfil is null)
        {
            paciente.Perfil = new PerfilPaciente
            {
                PacienteId = paciente.Id,
                FechaCreacion = DateTime.UtcNow
            };
        }


        paciente.Perfil.ObjetivoNutricional =
            dto.ObjetivoNutricional;

        paciente.Perfil.TipoActividad =
            dto.TipoActividad;

        paciente.Perfil.PesoInicial =
            dto.PesoInicial;

        paciente.Perfil.Altura =
            dto.Altura;

        paciente.Perfil.FechaInicio =
            dto.FechaInicio;

        paciente.Perfil.ActividadDescripcion =
            dto.ActividadDescripcion?.Trim();

        paciente.Perfil.ObservacionesGenerales =
            dto.ObservacionesGenerales?.Trim();

        paciente.Perfil.FechaActualizacion =
            DateTime.UtcNow;


        await _context.SaveChangesAsync();


        return new ResultadoPaciente<PacienteDetalleDto>
        {
            Exitoso = true,

            Datos =
                MapearDetalle(
                    paciente,
                    paciente.Perfil
                ),

            TipoError =
                TipoErrorPaciente.Ninguno
        };
    }


    // ==========================================
    // ACTIVAR / DESACTIVAR
    // ==========================================

    public async Task<ResultadoPaciente<bool>>
        CambiarEstadoAsync(
            int nutricionistaId,
            int pacienteId,
            bool activo)
    {
        var paciente =
            await _context.Pacientes
                .FirstOrDefaultAsync(p =>
                    p.Id == pacienteId
                    &&
                    p.NutricionistaId ==
                    nutricionistaId
                );


        if (paciente is null)
        {
            return new ResultadoPaciente<bool>
            {
                Exitoso = false,
                Error = "Paciente no encontrado.",
                TipoError =
                    TipoErrorPaciente.NoEncontrado
            };
        }


        paciente.Activo = activo;

        await _context.SaveChangesAsync();


        return new ResultadoPaciente<bool>
        {
            Exitoso = true,
            Datos = true,
            TipoError = TipoErrorPaciente.Ninguno
        };
    }


    // ==========================================
    // HELPERS
    // ==========================================

    private static ResultadoPaciente<PacienteDetalleDto>
        NoEncontrado()
    {
        return new ResultadoPaciente<PacienteDetalleDto>
        {
            Exitoso = false,

            Error = "Paciente no encontrado.",

            TipoError =
                TipoErrorPaciente.NoEncontrado
        };
    }


    private static PacienteDetalleDto MapearDetalle(
        Paciente paciente,
        PerfilPaciente? perfil)
    {
        return new PacienteDetalleDto
        {
            Id = paciente.Id,

            Nombre = paciente.Nombre,

            Apellido = paciente.Apellido,

            Email =
                paciente.Email ?? string.Empty,

            Telefono =
                paciente.PhoneNumber,

            FechaNacimiento =
                paciente.FechaNacimiento,

            Activo =
                paciente.Activo,

            ObjetivoNutricional =
                perfil?.ObjetivoNutricional
                    .ToString()
                ?? string.Empty,

            TipoActividad =
                perfil?.TipoActividad
                    .ToString()
                ?? string.Empty,

            PesoInicial =
                perfil?.PesoInicial,

            Altura =
                perfil?.Altura,

            FechaInicio =
                perfil?.FechaInicio,

            ActividadDescripcion =
                perfil?.ActividadDescripcion,

            ObservacionesGenerales =
                perfil?.ObservacionesGenerales,

            FechaCreacion =
                paciente.FechaCreacion
        };
    }
}