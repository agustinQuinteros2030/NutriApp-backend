using NutriApp.Models.Alimentos;
using NutriApp.Models.Pacientes;
using NutriApp.Models.Usuarios;

public class Nutricionista : UsuarioAplicacion
{
    public string? Matricula { get; set; }

    public ICollection<Paciente> Pacientes { get; set; } = [];

    public ICollection<CategoriaAlimento> CategoriasAlimentos { get; set; } = [];

    public ICollection<Alimento> Alimentos { get; set; } = [];

    public ICollection<GrupoEquivalencia> GruposEquivalencias { get; set; } = [];

    public ICollection<NotaPaciente> NotasPacientes { get; set; } = [];
}