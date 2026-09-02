
using NutriApi.Models.dieta;
using NutriApp.Enums;
using NutriApp.Models.Usuarios;
using System;
using System.Collections.Generic;

namespace NutriApp.Models.Dietas;

public class Dieta
{
    public int Id { get; set; }

    public int PacienteId { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string? Descripcion { get; set; }

    public int Version { get; set; } = 1;

    public DateOnly FechaInicio { get; set; }

    public DateOnly? FechaFin { get; set; }

    public EstadoDieta Estado { get; set; } = EstadoDieta.Borrador;

    public string? ObservacionesGenerales { get; set; }

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public DateTime? FechaActualizacion { get; set; }


    // Navegación

    public Paciente Paciente { get; set; } = null!;

    public ICollection<Comida> Comidas { get; set; } = [];

    public HidratacionDieta? Hidratacion { get; set; }

    public SuplementacionDieta? Suplementacion { get; set; }
}