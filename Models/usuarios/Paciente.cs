using NutriApp.Models.Dietas;
using NutriApp.Models.Pacientes;
using NutriApp.Models.Pagos;
using NutriApp.Models.Seguimiento;

namespace NutriApp.Models.Usuarios;

public class Paciente : UsuarioAplicacion
{
    public int NutricionistaId { get; set; }

    public DateOnly? FechaNacimiento { get; set; }


    // Navegaciones

    public Nutricionista Nutricionista { get; set; } = null!;

    public PerfilPaciente? Perfil { get; set; }

    public ICollection<Dieta> Dietas { get; set; } = [];

    public ICollection<NotaPaciente> Notas { get; set; } = [];

    public ICollection<PagoPaciente> Pagos { get; set; } = [];

    public ICollection<RegistroDiarioPaciente>
    RegistrosDiarios
    { get; set; } = [];

    public ICollection<SeguimientoSemanalPaciente>
    SeguimientosSemanales
    { get; set; } = [];
}