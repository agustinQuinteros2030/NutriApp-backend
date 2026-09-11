namespace NutriApi.DTOs.Dietas;

public class TotalesNutricionalesDto
{
    public decimal? Calorias { get; set; }

    public decimal? Proteinas { get; set; }

    public decimal? Carbohidratos { get; set; }

    public decimal? Grasas { get; set; }


    public int CantidadItems { get; set; }

    public int ItemsSinCalculo { get; set; }

    public int SeccionesSinOpcionPredeterminada { get; set; }


    public bool EsCompleto { get; set; }
}