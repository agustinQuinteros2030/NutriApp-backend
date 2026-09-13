
using Microsoft.EntityFrameworkCore;

using NutriApi.DTOs.PerfilPaciente;

using NutriApp.Data;

namespace NutriApi.Services.MiPerfil;

public class PerfilPacienteService
    : IPerfilPacienteService
{
  

 private readonly NutriAppDbContext _context;


    public PerfilPacienteService(
        NutriAppDbContext context)
    {
        _context = context;
    }


    public async Task<MiPerfilPacienteDto?>
        ObtenerMiPerfilAsync(
            int pacienteId)
    {
        return await _context.Pacientes
            .AsNoTracking()
            .Where(p =>
                p.Id == pacienteId
            )
            .Select(p =>
                new MiPerfilPacienteDto
                {
                    Id =
                        p.Id,

                    Nombre =
                        p.Nombre,

                    Apellido =
                        p.Apellido,

                    Email =
                        p.Email
                        ?? string.Empty,

                    Telefono =
                        p.PhoneNumber,

                    FechaNacimiento =
                        p.FechaNacimiento,

                    ObjetivoNutricional =
                        p.Perfil == null
                            ? null
                            : p.Perfil
                                .ObjetivoNutricional
                                .ToString(),

                    TipoActividad =
                        p.Perfil == null
                            ? null
                            : p.Perfil
                                .TipoActividad
                                .ToString(),

                    PesoInicial =
                        p.Perfil == null
                            ? null
                            : p.Perfil
                                .PesoInicial,

                    Altura =
                        p.Perfil == null
                            ? null
                            : p.Perfil
                                .Altura,

                    FechaInicio =
                        p.Perfil == null
                            ? null
                            : p.Perfil
                                .FechaInicio,

                    ActividadDescripcion =
                        p.Perfil == null
                            ? null
                            : p.Perfil
                                .ActividadDescripcion
                }
            )
            .FirstOrDefaultAsync();
    }
}