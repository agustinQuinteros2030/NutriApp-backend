namespace NutriApi.Services.CatalogoInicial;

public enum EstadoCargaCatalogo
{
    Importado,
    YaInicializado,
    CatalogoExistente,
}

public sealed record ResultadoCargaCatalogo(
    EstadoCargaCatalogo Estado,
    string Version,
    int Categorias,
    int Alimentos
);

public interface ICatalogoInicialService
{
    Task<ResultadoCargaCatalogo> InicializarAsync(
        int nutricionistaId,
        CancellationToken cancellationToken = default
    );
}
