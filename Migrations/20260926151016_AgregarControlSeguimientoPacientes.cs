using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace NutriApi.Migrations
{
    /// <inheritdoc />
    public partial class AgregarControlSeguimientoPacientes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ControlesSeguimientoPacientes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PacienteId = table.Column<int>(type: "integer", nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    FrecuenciaDias = table.Column<int>(type: "integer", nullable: false),
                    UltimoSeguimiento = table.Column<DateOnly>(type: "date", nullable: true),
                    ProximoSeguimiento = table.Column<DateOnly>(type: "date", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ControlesSeguimientoPacientes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ControlesSeguimientoPacientes_AspNetUsers_PacienteId",
                        column: x => x.PacienteId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ControlesSeguimientoPacientes_Activo_ProximoSeguimiento",
                table: "ControlesSeguimientoPacientes",
                columns: new[] { "Activo", "ProximoSeguimiento" });

            migrationBuilder.CreateIndex(
                name: "IX_ControlesSeguimientoPacientes_PacienteId",
                table: "ControlesSeguimientoPacientes",
                column: "PacienteId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ControlesSeguimientoPacientes");
        }
    }
}
