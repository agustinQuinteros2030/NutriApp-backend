using NutriApp.Enums;
using NutriApp.Models.Usuarios;
using System;

namespace NutriApp.Models.Pacientes;

public class PerfilPaciente
{
    public int Id { get; set; }

    public int PacienteId { get; set; }

    public ObjetivoNutricional ObjetivoNutricional { get; set; }

    public TipoActividad TipoActividad { get; set; }

    public decimal? PesoInicial { get; set; }

    public decimal? Altura { get; set; }

    public DateOnly? FechaInicio { get; set; }

    public string? ActividadDescripcion { get; set; }

    public string? ObservacionesGenerales { get; set; }

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public DateTime? FechaActualizacion { get; set; }


    // Navegación

    public Paciente Paciente { get; set; } = null!;
}