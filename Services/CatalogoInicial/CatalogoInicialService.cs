using System.Data;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using NutriApp.Data;
using NutriApp.Enums;
using NutriApp.Enums.Alimentos;
using NutriApp.Models.Alimentos;
using NutriApp.Models.Catalogo;

namespace NutriApi.Services.CatalogoInicial;

public sealed class CatalogoInicialService : ICatalogoInicialService
{
    private readonly NutriAppDbContext _context;
    private readonly IHostEnvironment _environment;

    public CatalogoInicialService(NutriAppDbContext context, IHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    // ==========================================
    // INICIALIZAR CATÁLOGO
    // ==========================================

    public async Task<ResultadoCargaCatalogo> InicializarAsync(
        int nutricionistaId,
        CancellationToken cancellationToken = default
    )
    {
        if (nutricionistaId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(nutricionistaId));
        }

        // ======================================
        // LEER CATÁLOGO JSON
        // ======================================

        var ruta = Path.Combine(
            _environment.ContentRootPath,
            "DatosIniciales",
            "catalogo_inicial_v1.json"
        );

        var contenido = await File.ReadAllTextAsync(ruta, cancellationToken);

        var catalogo =
            JsonSerializer.Deserialize<ArchivoCatalogo>(
                contenido,
                new JsonSerializerOptions(JsonSerializerDefaults.Web)
            ) ?? throw new InvalidOperationException("El catálogo inicial está vacío.");

        ValidarArchivo(catalogo);

        // ======================================
        // LEER GRUPOS DE EQUIVALENCIA
        // ======================================

        var rutaGrupos = Path.Combine(
            _environment.ContentRootPath,
            "DatosIniciales",
            "grupos_equivalencias_borrador_v1.json"
        );

        var contenidoGrupos = await File.ReadAllTextAsync(rutaGrupos, cancellationToken);

        var propuesta =
            JsonSerializer.Deserialize<ArchivoGrupos>(
                contenidoGrupos,
                new JsonSerializerOptions(JsonSerializerDefaults.Web)
            ) ?? throw new InvalidOperationException("Archivo de grupos vacío.");

        ValidarGrupos(propuesta, catalogo);

        // ======================================
        // ESTRATEGIA DE EJECUCIÓN
        // ======================================

        var estrategia = _context.Database.CreateExecutionStrategy();

        return await estrategia.ExecuteAsync(async () =>
        {
            _context.ChangeTracker.Clear();

            await using var transaccion = await _context.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken
            );

            // ==================================
            // VERIFICAR CARGA ANTERIOR
            // ==================================

            var yaInicializado = await _context
                .Set<CatalogoInicializacion>()
                .AsNoTracking()
                .AnyAsync(m => m.NutricionistaId == nutricionistaId, cancellationToken);

            if (yaInicializado)
            {
                return new ResultadoCargaCatalogo(
                    EstadoCargaCatalogo.YaInicializado,
                    catalogo.Version,
                    0,
                    0
                );
            }

            // ==================================
            // VERIFICAR NUTRICIONISTA
            // ==================================

            var existeNutricionista = await _context
                .Nutricionistas.AsNoTracking()
                .AnyAsync(n => n.Id == nutricionistaId, cancellationToken);

            if (!existeNutricionista)
            {
                throw new InvalidOperationException("Nutricionista no encontrado.");
            }

            // ==================================
            // NO SOBRESCRIBIR CATÁLOGOS
            // ==================================

            var hayDatos =
                await _context
                    .CategoriasAlimentos.AsNoTracking()
                    .AnyAsync(c => c.NutricionistaId == nutricionistaId, cancellationToken)
                || await _context
                    .Alimentos.AsNoTracking()
                    .AnyAsync(a => a.NutricionistaId == nutricionistaId, cancellationToken)
                || await _context
                    .GruposEquivalencias.AsNoTracking()
                    .AnyAsync(g => g.NutricionistaId == nutricionistaId, cancellationToken);

            if (hayDatos)
            {
                return new ResultadoCargaCatalogo(
                    EstadoCargaCatalogo.CatalogoExistente,
                    catalogo.Version,
                    0,
                    0
                );
            }

            // ==================================
            // CREAR CATEGORÍAS
            // ==================================

            var categorias = catalogo.Categorias.ToDictionary(
                n => n,
                n => new CategoriaAlimento
                {
                    NutricionistaId = nutricionistaId,
                    Nombre = n,
                    Activa = true,
                },
                StringComparer.OrdinalIgnoreCase
            );

            _context.CategoriasAlimentos.AddRange(categorias.Values);

            // ==================================
            // CREAR ALIMENTOS
            // ==================================

            var alimentosPorNombre = new Dictionary<string, Alimento>(
                StringComparer.OrdinalIgnoreCase
            );

            foreach (var entrada in catalogo.Alimentos)
            {
                var unidad = Enum.Parse<UnidadMedida>(entrada.UnidadBase);

                var alimentoNuevo = new Alimento
                {
                    NutricionistaId = nutricionistaId,

                    Categoria = categorias[entrada.Categoria],

                    Nombre = entrada.Nombre,

                    CantidadBase = entrada.CantidadBase,

                    UnidadBase = unidad,

                    Calorias = entrada.Calorias,

                    Proteinas = entrada.Proteinas,

                    Carbohidratos = entrada.Carbohidratos,

                    Grasas = entrada.Grasas,

                    Activo = true,

                    FechaCreacion = DateTime.UtcNow,
                };

                _context.Alimentos.Add(alimentoNuevo);

                alimentosPorNombre.Add(entrada.Nombre, alimentoNuevo);
            }

            // ==================================
            // CREAR GRUPOS Y EQUIVALENCIAS
            // ==================================

            foreach (var grupoDatos in propuesta.Grupos)
            {
                var grupo = new GrupoEquivalencia
                {
                    NutricionistaId = nutricionistaId,

                    Nombre = grupoDatos.Nombre,

                    Descripcion = grupoDatos.Observaciones,

                    Criterio = Enum.Parse<CriterioEquivalencia>(grupoDatos.Criterio),

                    Activo = grupoDatos.Aprobado,

                    FechaCreacion = DateTime.UtcNow,
                };

                foreach (var nombreAlimento in grupoDatos.Alimentos)
                {
                    grupo.Equivalencias.Add(
                        new EquivalenciaAlimento
                        {
                            Alimento = alimentosPorNombre[nombreAlimento],

                            Activa = grupoDatos.Aprobado,
                        }
                    );
                }

                _context.GruposEquivalencias.Add(grupo);
            }

            // ==================================
            // REGISTRAR INICIALIZACIÓN
            // ==================================

            _context
                .Set<CatalogoInicializacion>()
                .Add(
                    new CatalogoInicializacion
                    {
                        NutricionistaId = nutricionistaId,

                        VersionCatalogo = catalogo.Version,

                        FechaInicializacion = DateTime.UtcNow,
                    }
                );

            // ==================================
            // PERSISTENCIA
            // ==================================

            await _context.SaveChangesAsync(cancellationToken);

            await transaccion.CommitAsync(cancellationToken);

            return new ResultadoCargaCatalogo(
                EstadoCargaCatalogo.Importado,
                catalogo.Version,
                categorias.Count,
                catalogo.Alimentos.Count
            );
        });
    }

    // ==========================================
    // VALIDAR ARCHIVO DE ALIMENTOS
    // ==========================================

    private static void ValidarArchivo(ArchivoCatalogo archivo)
    {
        if (
            string.IsNullOrWhiteSpace(archivo.Version)
            || archivo.Categorias is null
            || archivo.Categorias.Count == 0
            || archivo.Alimentos is null
            || archivo.Alimentos.Count == 0
        )
        {
            throw new InvalidOperationException("Catálogo inicial incompleto.");
        }

        if (
            archivo.Categorias.Any(string.IsNullOrWhiteSpace)
            || archivo.Categorias.Distinct(StringComparer.OrdinalIgnoreCase).Count()
                != archivo.Categorias.Count
        )
        {
            throw new InvalidOperationException("Categorías vacías o duplicadas.");
        }

        var nombres = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var categorias = archivo.Categorias.ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var a in archivo.Alimentos)
        {
            if (
                string.IsNullOrWhiteSpace(a.Nombre)
                || !nombres.Add(a.Nombre)
                || !categorias.Contains(a.Categoria)
                || a.CantidadBase <= 0m
                || a.Calorias < 0m
                || a.Carbohidratos < 0m
                || a.Proteinas < 0m
                || a.Grasas < 0m
                || !Enum.TryParse<UnidadMedida>(a.UnidadBase, out var unidad)
                || !Enum.IsDefined(unidad)
            )
            {
                throw new InvalidOperationException($"Entrada inválida o duplicada: {a.Nombre}");
            }
        }
    }

    // ==========================================
    // VALIDAR GRUPOS
    // ==========================================

    private static void ValidarGrupos(ArchivoGrupos propuesta, ArchivoCatalogo catalogo)
    {
        if (
            propuesta.Version != catalogo.Version
            || propuesta.Grupos is null
            || propuesta.Grupos.Count == 0
        )
        {
            throw new InvalidOperationException("Los grupos no coinciden con el catálogo.");
        }

        var alimentos = catalogo.Alimentos.ToDictionary(
            x => x.Nombre,
            StringComparer.OrdinalIgnoreCase
        );

        var nombresGrupos = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var grupo in propuesta.Grupos)
        {
            if (
                string.IsNullOrWhiteSpace(grupo.Nombre)
                || !nombresGrupos.Add(grupo.Nombre)
                || grupo.Alimentos is null
                || grupo.Alimentos.Count < 2
                || !Enum.TryParse<CriterioEquivalencia>(grupo.Criterio, false, out var criterio)
                || !Enum.IsDefined(criterio)
            )
            {
                throw new InvalidOperationException($"Grupo inválido: {grupo.Nombre}");
            }

            var usados = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var nombre in grupo.Alimentos)
            {
                if (!usados.Add(nombre) || !alimentos.TryGetValue(nombre, out var alimento))
                {
                    throw new InvalidOperationException(
                        $"Alimento inválido en {grupo.Nombre}: {nombre}"
                    );
                }

                var nutriente = criterio switch
                {
                    CriterioEquivalencia.Carbohidratos => alimento.Carbohidratos,

                    CriterioEquivalencia.Proteinas => alimento.Proteinas,

                    CriterioEquivalencia.Grasas => alimento.Grasas,

                    CriterioEquivalencia.Calorias => alimento.Calorias,

                    _ => 0m,
                };

                if (nutriente <= 0m)
                {
                    throw new InvalidOperationException(
                        $"Criterio sin aporte nutricional: {nombre}"
                    );
                }
            }
        }
    }

    // ==========================================
    // MODELOS DE DESERIALIZACIÓN
    // ==========================================

    public sealed class ArchivoGrupos
    {
        public string Version { get; set; } = string.Empty;

        public List<GrupoInicial> Grupos { get; set; } = [];
    }

    public sealed class GrupoInicial
    {
        public string Nombre { get; set; } = string.Empty;

        public string Criterio { get; set; } = string.Empty;

        public bool Aprobado { get; set; }

        public string Observaciones { get; set; } = string.Empty;

        public List<string> Alimentos { get; set; } = [];
    }

    public sealed class ArchivoCatalogo
    {
        public string Version { get; set; } = string.Empty;

        public List<string> Categorias { get; set; } = [];

        public List<AlimentoInicial> Alimentos { get; set; } = [];
    }

    public sealed class AlimentoInicial
    {
        public string Categoria { get; set; } = string.Empty;

        public string Nombre { get; set; } = string.Empty;

        public decimal CantidadBase { get; set; }

        public string UnidadBase { get; set; } = string.Empty;

        public decimal Calorias { get; set; }

        public decimal Carbohidratos { get; set; }

        public decimal Proteinas { get; set; }

        public decimal Grasas { get; set; }
    }
}
