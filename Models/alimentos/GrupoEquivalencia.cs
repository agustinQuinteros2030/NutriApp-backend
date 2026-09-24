using NutriApp.Enums.Alimentos;
using NutriApp.Models.Usuarios;

namespace NutriApp.Models.Alimentos;

public class GrupoEquivalencia
{
    public int Id { get; set; }

    public int NutricionistaId { get; set; }

    public string Nombre { get; set; } =
        string.Empty;

    public string? Descripcion { get; set; }


    // ==========================================
    // CRITERIO DE CONVERSIÓN
    // ==========================================

    /*
     * Define qué nutriente se debe mantener
     * al convertir entre alimentos.
     *
     * Ejemplos:
     *
     * Arroz -> Papa
     * criterio = Carbohidratos
     *
     * Pollo -> Carne
     * criterio = Proteinas
     *
     * Aceite -> Palta
     * criterio = Grasas
     */

    public CriterioEquivalencia Criterio { get; set; }


    public bool Activo { get; set; } =
        true;

    public DateTime FechaCreacion { get; set; } =
        DateTime.UtcNow;


    // Navegación

    public Nutricionista Nutricionista { get; set; } =
        null!;

    public ICollection<EquivalenciaAlimento>
        Equivalencias
    { get; set; } = [];
}