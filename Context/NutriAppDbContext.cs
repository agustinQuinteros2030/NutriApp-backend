
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

using NutriApp.Models.Alimentos;
using NutriApp.Models.Dietas;
using NutriApp.Models.Notificaciones;
using NutriApp.Models.Pacientes;
using NutriApp.Models.Pagos;
using NutriApp.Models.Seguimiento;
using NutriApp.Models.Usuarios;

namespace NutriApp.Data;

public class NutriAppDbContext
    : IdentityDbContext<UsuarioAplicacion, IdentityRole<int>, int>
{
    public NutriAppDbContext(DbContextOptions<NutriAppDbContext> options)
        : base(options)
    {
    }


    // =========================
    // USUARIOS
    // =========================

    public DbSet<Nutricionista> Nutricionistas { get; set; }

    public DbSet<Paciente> Pacientes { get; set; }


    // =========================
    // PACIENTES
    // =========================

    public DbSet<PerfilPaciente> PerfilesPacientes { get; set; }

    public DbSet<NotaPaciente> NotasPacientes { get; set; }


    // =========================
    // ALIMENTOS
    // =========================

    public DbSet<CategoriaAlimento> CategoriasAlimentos { get; set; }

    public DbSet<Alimento> Alimentos { get; set; }

    public DbSet<GrupoEquivalencia> GruposEquivalencias { get; set; }

    public DbSet<EquivalenciaAlimento> EquivalenciasAlimentos { get; set; }


    // =========================
    // DIETAS
    // =========================

    public DbSet<Dieta> Dietas { get; set; }

    public DbSet<Comida> Comidas { get; set; }

    public DbSet<SeccionComida> SeccionesComidas { get; set; }

    public DbSet<OpcionSeccionComida> OpcionesSeccionesComidas { get; set; }

    public DbSet<ItemOpcionComida> ItemsOpcionesComidas { get; set; }

    public DbSet<AlternativaItemComida> AlternativasItemsComidas { get; set; }

    public DbSet<HidratacionDieta> HidratacionesDietas { get; set; }

    public DbSet<SuplementacionDieta> SuplementacionesDietas { get; set; }

    public DbSet<ItemSuplementacion> ItemsSuplementacion { get; set; }

    public DbSet<PagoPaciente> PagosPacientes { get; set; }

    // =========================
    // SEGUIMIENTO
    // =========================

    public DbSet<RegistroDiarioPaciente>
        RegistrosDiariosPacientes
    { get; set; }


    public DbSet<SeguimientoSemanalPaciente>
    SeguimientosSemanalesPacientes
    { get; set; }




    public DbSet<Notificacion> Notificaciones
    {
        get;
        set;
    }

    public DbSet<Turno> Turnos { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // MUY IMPORTANTE porque usamos Identity
        base.OnModelCreating(modelBuilder);


        ConfigurarUsuarios(modelBuilder);

        ConfigurarPacientes(modelBuilder);

        ConfigurarAlimentos(modelBuilder);

        ConfigurarDietas(modelBuilder);

        ConfigurarDecimales(modelBuilder);

        ConfigurarSeguimiento(modelBuilder);

        ConfigurarNotificaciones(modelBuilder);
        ConfigurarTurnos(modelBuilder);
    }

    private void ConfigurarNotificaciones(ModelBuilder modelBuilder)
    {
        // ==========================================
        // NOTIFICACIONES
        // ==========================================

        modelBuilder.Entity<Notificacion>(
            entity =>
            {
                entity.HasKey(n =>
                    n.Id
                );


                // ======================================
                // USUARIO
                // ======================================

                entity.HasOne(n =>
                        n.Usuario
                    )
                    .WithMany()
                    .HasForeignKey(n =>
                        n.UsuarioId
                    )
                    .OnDelete(
                        DeleteBehavior.Cascade
                    );


                // ======================================
                // CONTENIDO
                // ======================================

                entity.Property(n =>
                        n.Titulo
                    )
                    .IsRequired()
                    .HasMaxLength(150);


                entity.Property(n =>
                        n.Mensaje
                    )
                    .IsRequired()
                    .HasMaxLength(500);


                entity.Property(n =>
                        n.RecursoTipo
                    )
                    .HasMaxLength(100);


                // ======================================
                // ENUM
                // ======================================

                entity.Property(n =>
                        n.Tipo
                    )
                    .IsRequired();


                // ======================================
                // ESTADO
                // ======================================

                entity.Property(n =>
                        n.Leida
                    )
                    .IsRequired()
                    .HasDefaultValue(false);


                entity.Property(n =>
                        n.FechaCreacion
                    )
                    .IsRequired();


                // ======================================
                // ÍNDICES
                // ======================================

                /*
                 * Consulta principal:
                 *
                 * "dame las notificaciones de este
                 * usuario ordenadas por fecha".
                 */

                entity.HasIndex(n =>
                    new
                    {
                        n.UsuarioId,
                        n.FechaCreacion
                    });


                /*
                 * Muy utilizado para:
                 *
                 * "¿cuántas notificaciones no leídas
                 * tiene este usuario?"
                 */

                entity.HasIndex(n =>
                    new
                    {
                        n.UsuarioId,
                        n.Leida
                    });
            }
        );

    }


    // ==========================================================
    // USUARIOS
    // ==========================================================

    private static void ConfigurarUsuarios(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UsuarioAplicacion>()
            .HasDiscriminator<string>("TipoUsuario")
            .HasValue<UsuarioAplicacion>("Usuario")
            .HasValue<Nutricionista>("Nutricionista")
            .HasValue<Paciente>("Paciente");


        // Nutricionista 1 ---- N Pacientes

        modelBuilder.Entity<Paciente>()
            .HasOne(p => p.Nutricionista)
            .WithMany(n => n.Pacientes)
            .HasForeignKey(p => p.NutricionistaId)
            .OnDelete(DeleteBehavior.Restrict);
    }


    // ==========================================================
    // PACIENTES
    // ==========================================================

    private static void ConfigurarPacientes(ModelBuilder modelBuilder)
    {
        // Paciente 1 ---- 1 PerfilPaciente

        modelBuilder.Entity<PerfilPaciente>()
            .HasOne(pp => pp.Paciente)
            .WithOne(p => p.Perfil)
            .HasForeignKey<PerfilPaciente>(pp => pp.PacienteId)
            .OnDelete(DeleteBehavior.Cascade);


        modelBuilder.Entity<PerfilPaciente>()
            .HasIndex(pp => pp.PacienteId)
            .IsUnique();


        // Paciente 1 ---- N Notas

        modelBuilder.Entity<NotaPaciente>()
            .HasOne(n => n.Paciente)
            .WithMany(p => p.Notas)
            .HasForeignKey(n => n.PacienteId)
            .OnDelete(DeleteBehavior.Restrict);


        // Nutricionista 1 ---- N Notas

        modelBuilder.Entity<NotaPaciente>()
            .HasOne(n => n.Nutricionista)
            .WithMany(n => n.NotasPacientes)
            .HasForeignKey(n => n.NutricionistaId)
            .OnDelete(DeleteBehavior.Restrict);


        modelBuilder.Entity<PagoPaciente>()
    .HasOne(p =>
        p.Paciente
    )
    .WithMany(p =>
        p.Pagos
    )
    .HasForeignKey(p =>
        p.PacienteId
    )
    .OnDelete(
        DeleteBehavior.Restrict
    );


        modelBuilder.Entity<PagoPaciente>()
            .HasIndex(p =>
                new
                {
                    p.PacienteId,
                    p.ProximoVencimiento
                }
            );
    }


    // ==========================================================
    // ALIMENTOS
    // ==========================================================

    private static void ConfigurarAlimentos(ModelBuilder modelBuilder)
    {
        // Nutricionista 1 ---- N Categorias

        modelBuilder.Entity<CategoriaAlimento>()
            .HasOne(c => c.Nutricionista)
            .WithMany(n => n.CategoriasAlimentos)
            .HasForeignKey(c => c.NutricionistaId)
            .OnDelete(DeleteBehavior.Restrict);


        // Categoria 1 ---- N Alimentos

        modelBuilder.Entity<Alimento>()
            .HasOne(a => a.Categoria)
            .WithMany(c => c.Alimentos)
            .HasForeignKey(a => a.CategoriaAlimentoId)
            .OnDelete(DeleteBehavior.Restrict);


        // Nutricionista 1 ---- N Alimentos

        modelBuilder.Entity<Alimento>()
            .HasOne(a => a.Nutricionista)
            .WithMany(n => n.Alimentos)
            .HasForeignKey(a => a.NutricionistaId)
            .OnDelete(DeleteBehavior.Restrict);


        // Nutricionista 1 ---- N GruposEquivalencia

        modelBuilder.Entity<GrupoEquivalencia>()
            .HasOne(g => g.Nutricionista)
            .WithMany(n => n.GruposEquivalencias)
            .HasForeignKey(g => g.NutricionistaId)
            .OnDelete(DeleteBehavior.Restrict);


        // GrupoEquivalencia 1 ---- N Equivalencias

        modelBuilder.Entity<EquivalenciaAlimento>()
            .HasOne(e => e.GrupoEquivalencia)
            .WithMany(g => g.Equivalencias)
            .HasForeignKey(e => e.GrupoEquivalenciaId)
            .OnDelete(DeleteBehavior.Cascade);


        // Alimento 1 ---- N Equivalencias

        modelBuilder.Entity<EquivalenciaAlimento>()
            .HasOne(e => e.Alimento)
            .WithMany(a => a.Equivalencias)
            .HasForeignKey(e => e.AlimentoId)
            .OnDelete(DeleteBehavior.Restrict);


        // El mismo alimento no puede estar repetido
        // dentro del mismo grupo de equivalencia.

        modelBuilder.Entity<EquivalenciaAlimento>()
            .HasIndex(e => new
            {
                e.GrupoEquivalenciaId,
                e.AlimentoId
            })
            .IsUnique();


        // Evita categorías repetidas para un nutricionista

        modelBuilder.Entity<CategoriaAlimento>()
            .HasIndex(c => new
            {
                c.NutricionistaId,
                c.Nombre
            })
            .IsUnique();


        // Evita alimentos repetidos para un nutricionista

        modelBuilder.Entity<Alimento>()
            .HasIndex(a => new
            {
                a.NutricionistaId,
                a.Nombre
            })
            .IsUnique();
    }


    // ==========================================================
    // DIETAS
    // ==========================================================

    private static void ConfigurarDietas(ModelBuilder modelBuilder)
    {
        // Paciente 1 ---- N Dietas

        modelBuilder.Entity<Dieta>()
            .HasOne(d => d.Paciente)
            .WithMany(p => p.Dietas)
            .HasForeignKey(d => d.PacienteId)
            .OnDelete(DeleteBehavior.Restrict);


        // Dieta 1 ---- N Comidas

        modelBuilder.Entity<Comida>()
            .HasOne(c => c.Dieta)
            .WithMany(d => d.Comidas)
            .HasForeignKey(c => c.DietaId)
            .OnDelete(DeleteBehavior.Cascade);


        // Comida 1 ---- N Secciones

        modelBuilder.Entity<SeccionComida>()
            .HasOne(s => s.Comida)
            .WithMany(c => c.Secciones)
            .HasForeignKey(s => s.ComidaId)
            .OnDelete(DeleteBehavior.Cascade);


        // Seccion 1 ---- N Opciones

        modelBuilder.Entity<OpcionSeccionComida>()
            .HasOne(o => o.SeccionComida)
            .WithMany(s => s.Opciones)
            .HasForeignKey(o => o.SeccionComidaId)
            .OnDelete(DeleteBehavior.Cascade);


        // Opcion 1 ---- N Items

        modelBuilder.Entity<ItemOpcionComida>()
            .HasOne(i => i.OpcionSeccionComida)
            .WithMany(o => o.Items)
            .HasForeignKey(i => i.OpcionSeccionComidaId)
            .OnDelete(DeleteBehavior.Cascade);


        // Alimento 1 ---- N Items

        modelBuilder.Entity<ItemOpcionComida>()
            .HasOne(i => i.Alimento)
            .WithMany(a => a.ItemsComida)
            .HasForeignKey(i => i.AlimentoId)
            .OnDelete(DeleteBehavior.Restrict);


        // Item 1 ---- N Alternativas

        modelBuilder.Entity<AlternativaItemComida>()
            .HasOne(a => a.ItemOpcionComida)
            .WithMany(i => i.Alternativas)
            .HasForeignKey(a => a.ItemOpcionComidaId)
            .OnDelete(DeleteBehavior.Cascade);


        // Alimento 1 ---- N Alternativas

        modelBuilder.Entity<AlternativaItemComida>()
            .HasOne(a => a.Alimento)
            .WithMany(a => a.Alternativas)
            .HasForeignKey(a => a.AlimentoId)
            .OnDelete(DeleteBehavior.Restrict);


        modelBuilder.Entity<AlternativaItemComida>()
    .HasOne(a => a.GrupoEquivalencia)
    .WithMany()
    .HasForeignKey(a => a.GrupoEquivalenciaId)
    .OnDelete(DeleteBehavior.Restrict);


        // No podemos agregar dos veces la misma alternativa
        // al mismo item.

        modelBuilder.Entity<AlternativaItemComida>()
            .HasIndex(a => new
            {
                a.ItemOpcionComidaId,
                a.AlimentoId
            })
            .IsUnique();


        // Dieta 1 ---- 0..1 Hidratacion

        modelBuilder.Entity<HidratacionDieta>()
            .HasOne(h => h.Dieta)
            .WithOne(d => d.Hidratacion)
            .HasForeignKey<HidratacionDieta>(h => h.DietaId)
            .OnDelete(DeleteBehavior.Cascade);


        modelBuilder.Entity<HidratacionDieta>()
            .HasIndex(h => h.DietaId)
            .IsUnique();


        // Dieta 1 ---- 0..1 Suplementacion

        modelBuilder.Entity<SuplementacionDieta>()
            .HasOne(s => s.Dieta)
            .WithOne(d => d.Suplementacion)
            .HasForeignKey<SuplementacionDieta>(s => s.DietaId)
            .OnDelete(DeleteBehavior.Cascade);


        modelBuilder.Entity<SuplementacionDieta>()
            .HasIndex(s => s.DietaId)
            .IsUnique();


        // Suplementacion 1 ---- N Items

        modelBuilder.Entity<ItemSuplementacion>()
            .HasOne(i => i.SuplementacionDieta)
            .WithMany(s => s.Items)
            .HasForeignKey(i => i.SuplementacionDietaId)
            .OnDelete(DeleteBehavior.Cascade);
    }


    // ==========================================================
    // PRECISION DECIMAL
    // ==========================================================

    private static void ConfigurarDecimales(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PerfilPaciente>()
            .Property(p => p.PesoInicial)
            .HasPrecision(6, 2);


        modelBuilder.Entity<PerfilPaciente>()
            .Property(p => p.Altura)
            .HasPrecision(5, 2);


        modelBuilder.Entity<Alimento>()
            .Property(a => a.CantidadBase)
            .HasPrecision(10, 2);


        modelBuilder.Entity<Alimento>()
            .Property(a => a.Calorias)
            .HasPrecision(10, 2);


        modelBuilder.Entity<Alimento>()
            .Property(a => a.Proteinas)
            .HasPrecision(10, 2);


        modelBuilder.Entity<Alimento>()
            .Property(a => a.Carbohidratos)
            .HasPrecision(10, 2);


        modelBuilder.Entity<Alimento>()
            .Property(a => a.Grasas)
            .HasPrecision(10, 2);


        modelBuilder.Entity<EquivalenciaAlimento>()
            .Property(e => e.CantidadEquivalente)
            .HasPrecision(10, 2);


        modelBuilder.Entity<ItemOpcionComida>()
            .Property(i => i.Cantidad)
            .HasPrecision(10, 2);


        modelBuilder.Entity<ItemSuplementacion>()
            .Property(i => i.Cantidad)
            .HasPrecision(10, 2);


        modelBuilder.Entity<PagoPaciente>()
        .Property(p => p.Monto)
        .HasPrecision(18, 2);


        modelBuilder
    .Entity<SeguimientoSemanalPaciente>()
    .Property(s =>
        s.PesoActual
    )
    .HasPrecision(
        6,
        2
    );
    }


    private static void ConfigurarSeguimiento(
    ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<RegistroDiarioPaciente>()
            .HasOne(r =>
                r.Paciente
            )
            .WithMany(p =>
                p.RegistrosDiarios
            )
            .HasForeignKey(r =>
                r.PacienteId
            )
            .OnDelete(
                DeleteBehavior.Restrict
            );


        /*
         * Un paciente solamente puede tener
         * un registro por fecha.
         */

        modelBuilder
            .Entity<RegistroDiarioPaciente>()
            .HasIndex(r =>
                new
                {
                    r.PacienteId,
                    r.Fecha
                }
            )
            .IsUnique();

        // ==========================================
        // SEGUIMIENTO SEMANAL
        // ==========================================

        modelBuilder
            .Entity<SeguimientoSemanalPaciente>()
            .HasOne(s =>
                s.Paciente
            )
            .WithMany(p =>
                p.SeguimientosSemanales
            )
            .HasForeignKey(s =>
                s.PacienteId
            )
            .OnDelete(
                DeleteBehavior.Restrict
            );


        /*
         * Un paciente solamente puede completar
         * un seguimiento para cada semana.
         *
         * FechaInicioSemana funciona como
         * identificador lógico de la semana.
         */

        modelBuilder
            .Entity<SeguimientoSemanalPaciente>()
            .HasIndex(s =>
                new
                {
                    s.PacienteId,
                    s.FechaInicioSemana
                }
            )
            .IsUnique();
    }

    private static void ConfigurarTurnos(
    ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Turno>(
            entity =>
            {
                // ======================================
                // PK
                // ======================================

                entity.HasKey(t =>
                    t.Id
                );


                // ======================================
                // PACIENTE
                // ======================================

                entity.HasOne(t =>
                        t.Paciente
                    )
                    .WithMany()
                    .HasForeignKey(t =>
                        t.PacienteId
                    )
                    .OnDelete(
                        DeleteBehavior.Restrict
                    );


                // ======================================
                // FECHA
                // ======================================

                entity.Property(t =>
                        t.FechaHora
                    )
                    .IsRequired();


                entity.Property(t =>
                        t.FechaCreacion
                    )
                    .IsRequired();


                // ======================================
                // ENUMS
                // ======================================

                entity.Property(t =>
                        t.Modalidad
                    )
                    .IsRequired();


                entity.Property(t =>
                        t.Estado
                    )
                    .IsRequired();


                // ======================================
                // TEXTOS
                // ======================================

                entity.Property(t =>
                        t.Lugar
                    )
                    .HasMaxLength(200);


                entity.Property(t =>
                        t.LinkReunion
                    )
                    .HasMaxLength(500);


                entity.Property(t =>
                        t.Motivo
                    )
                    .HasMaxLength(250);


                entity.Property(t =>
                        t.Observaciones
                    )
                    .HasMaxLength(1000);


                // ======================================
                // ÍNDICES
                // ======================================

                /*
                 * Consultaremos frecuentemente:
                 *
                 * turnos de un paciente
                 * ordenados por fecha.
                 */

                entity.HasIndex(t =>
                    new
                    {
                        t.PacienteId,
                        t.FechaHora
                    });


                /*
                 * También consultaremos:
                 *
                 * próximos turnos programados.
                 */

                entity.HasIndex(t =>
                    new
                    {
                        t.Estado,
                        t.FechaHora
                    });
            }
        );
    }




}
