using NutriApp.Enums.Alimentos;
using NutriApp.Models.Alimentos;

namespace NutriApi.Calculos;

public static class CalculadoraEquivalencias
{
    // ==========================================
    // NUEVO CÁLCULO AUTOMÁTICO
    // ==========================================

    /// <summary>
    /// Calcula automáticamente qué cantidad de un
    /// alimento destino aporta la misma cantidad
    /// del nutriente indicado que una cantidad
    /// determinada del alimento origen.
    ///
    /// Ejemplo:
    ///
    /// criterio = Carbohidratos
    ///
    /// arroz:
    /// 50 g base
    /// 39 g carbohidratos
    ///
    /// papa:
    /// 100 g base
    /// 20.5 g carbohidratos
    ///
    /// origen:
    /// 100 g arroz
    ///
    /// resultado:
    /// aproximadamente 380 g papa.
    /// </summary>
    public static decimal CalcularCantidadDestino(
        decimal cantidadOrigen,
        Alimento alimentoOrigen,
        Alimento alimentoDestino,
        CriterioEquivalencia criterio)
    {
        if (cantidadOrigen <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(cantidadOrigen),
                "La cantidad de origen debe ser mayor a cero."
            );
        }


        if (alimentoOrigen is null)
        {
            throw new ArgumentNullException(
                nameof(alimentoOrigen)
            );
        }


        if (alimentoDestino is null)
        {
            throw new ArgumentNullException(
                nameof(alimentoDestino)
            );
        }


        if (alimentoOrigen.CantidadBase <= 0)
        {
            throw new ArgumentException(
                "La cantidad base del alimento origen debe ser mayor a cero.",
                nameof(alimentoOrigen)
            );
        }


        if (alimentoDestino.CantidadBase <= 0)
        {
            throw new ArgumentException(
                "La cantidad base del alimento destino debe ser mayor a cero.",
                nameof(alimentoDestino)
            );
        }


        var nutrienteOrigen =
            ObtenerValorCriterio(
                alimentoOrigen,
                criterio
            );


        var nutrienteDestino =
            ObtenerValorCriterio(
                alimentoDestino,
                criterio
            );


        if (!nutrienteOrigen.HasValue ||
            nutrienteOrigen.Value <= 0)
        {
            throw new InvalidOperationException(
                $"El alimento origen no posee un valor válido para el criterio {criterio}."
            );
        }


        if (!nutrienteDestino.HasValue ||
            nutrienteDestino.Value <= 0)
        {
            throw new InvalidOperationException(
                $"El alimento destino no posee un valor válido para el criterio {criterio}."
            );
        }


        /*
         * PASO 1
         *
         * Calculamos cuánto nutriente aporta
         * realmente la cantidad indicada
         * del alimento origen.
         *
         * cantidadOrigen
         * ------------------- × nutrienteBase
         * cantidadBaseOrigen
         */

        var factorOrigen =
            cantidadOrigen /
            alimentoOrigen.CantidadBase;


        var nutrienteConsumido =
            nutrienteOrigen.Value *
            factorOrigen;


        /*
         * PASO 2
         *
         * Calculamos qué cantidad del alimento
         * destino necesitamos para obtener
         * ese mismo nutriente.
         *
         * nutrienteConsumido
         * --------------------- × cantidadBaseDestino
         * nutrienteBaseDestino
         */

        var cantidadDestino =
            nutrienteConsumido
            /
            nutrienteDestino.Value
            *
            alimentoDestino.CantidadBase;


        return Math.Round(
            cantidadDestino,
            2
        );
    }


    // ==========================================
    // OBTENER NUTRIENTE DEL CRITERIO
    // ==========================================

    public static decimal?
        ObtenerValorCriterio(
            Alimento alimento,
            CriterioEquivalencia criterio)
    {
        return criterio switch
        {
            CriterioEquivalencia.Carbohidratos =>
                alimento.Carbohidratos,

            CriterioEquivalencia.Proteinas =>
                alimento.Proteinas,

            CriterioEquivalencia.Grasas =>
                alimento.Grasas,

            CriterioEquivalencia.Calorias =>
                alimento.Calorias,

            _ =>
                throw new ArgumentOutOfRangeException(
                    nameof(criterio),
                    criterio,
                    "El criterio de equivalencia no es válido."
                )
        };
    }


    // ==========================================
    // MÉTODO ANTERIOR
    // ==========================================

    /*
     * TEMPORAL.
     *
     * Lo dejamos mientras migramos
     * EquivalenciaService y
     * AlternativaItemComidaService.
     *
     * Cuando ningún service lo utilice,
     * lo eliminamos.
     */

    public static decimal CalcularCantidadDestino(
        decimal cantidadOrigen,
        decimal cantidadEquivalenteOrigen,
        decimal cantidadEquivalenteDestino)
    {
        if (cantidadOrigen <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(cantidadOrigen),
                "La cantidad de origen debe ser mayor a cero."
            );
        }


        if (cantidadEquivalenteOrigen <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(cantidadEquivalenteOrigen),
                "La cantidad equivalente de origen debe ser mayor a cero."
            );
        }


        if (cantidadEquivalenteDestino <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(cantidadEquivalenteDestino),
                "La cantidad equivalente de destino debe ser mayor a cero."
            );
        }


        var factor =
            cantidadOrigen /
            cantidadEquivalenteOrigen;


        var cantidadDestino =
            cantidadEquivalenteDestino *
            factor;


        return Math.Round(
            cantidadDestino,
            2
        );
    }
}