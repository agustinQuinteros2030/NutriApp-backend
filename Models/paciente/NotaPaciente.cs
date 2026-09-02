using NutriApp.Models.Usuarios;
using System;

namespace NutriApp.Models.Pacientes;

public class NotaPaciente
{
    public int Id { get; set; }

    public int PacienteId { get; set; }

    public int NutricionistaId { get; set; }

    public string Contenido { get; set; } = string.Empty;

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public DateTime? FechaActualizacion { get; set; }


    // Navegaciones

    public Paciente Paciente { get; set; } = null!;

    public Nutricionista Nutricionista { get; set; } = null!;
}