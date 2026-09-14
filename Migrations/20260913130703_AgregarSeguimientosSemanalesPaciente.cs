using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace NutriApi.Migrations
{
    /// <inheritdoc />
    public partial class AgregarSeguimientosSemanalesPaciente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SeguimientosSemanalesPacientes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PacienteId = table.Column<int>(type: "integer", nullable: false),
                    FechaInicioSemana = table.Column<DateOnly>(type: "date", nullable: false),
                    FechaFinSemana = table.Column<DateOnly>(type: "date", nullable: false),
                    FechaRespuesta = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PesoActual = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: false),
                    Adherencia = table.Column<int>(type: "integer", nullable: false),
                    Descanso = table.Column<int>(type: "integer", nullable: false),
                    Digestiones = table.Column<int>(type: "integer", nullable: false),
                    DetalleDigestiones = table.Column<string>(type: "text", nullable: true),
                    RendimientoEntrenamientos = table.Column<int>(type: "integer", nullable: false),
                    CumplimientoHidratacion = table.Column<int>(type: "integer", nullable: false),
                    RegularidadIntestinal = table.Column<int>(type: "integer", nullable: false),
                    TuvoMolestiaFisica = table.Column<bool>(type: "boolean", nullable: false),
                    DetalleMolestiaFisica = table.Column<string>(type: "text", nullable: true),
                    SatisfaccionComunicacion = table.Column<int>(type: "integer", nullable: false),
                    RevisionNutricionista = table.Column<string>(type: "text", nullable: true),
                    FechaRevisionNutricionista = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeguimientosSemanalesPacientes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SeguimientosSemanalesPacientes_AspNetUsers_PacienteId",
                        column: x => x.PacienteId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SeguimientosSemanalesPacientes_PacienteId_FechaInicioSemana",
                table: "SeguimientosSemanalesPacientes",
                columns: new[] { "PacienteId", "FechaInicioSemana" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SeguimientosSemanalesPacientes");
        }
    }
}
