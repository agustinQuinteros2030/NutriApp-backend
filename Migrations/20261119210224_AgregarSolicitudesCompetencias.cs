using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace NutriApi.Migrations
{
    /// <inheritdoc />
    public partial class AgregarSolicitudesCompetencias : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SolicitudesCompetencias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PacienteId = table.Column<int>(type: "integer", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Disciplina = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FechaInicio = table.Column<DateOnly>(type: "date", nullable: false),
                    FechaFin = table.Column<DateOnly>(type: "date", nullable: true),
                    Lugar = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ComentarioPaciente = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Estado = table.Column<int>(type: "integer", nullable: false),
                    MotivoRechazo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CompetenciaId = table.Column<int>(type: "integer", nullable: true),
                    FechaSolicitud = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaResolucion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolicitudesCompetencias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SolicitudesCompetencias_AspNetUsers_PacienteId",
                        column: x => x.PacienteId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SolicitudesCompetencias_Competencias_CompetenciaId",
                        column: x => x.CompetenciaId,
                        principalTable: "Competencias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesCompetencias_CompetenciaId",
                table: "SolicitudesCompetencias",
                column: "CompetenciaId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesCompetencias_Estado_FechaSolicitud",
                table: "SolicitudesCompetencias",
                columns: new[] { "Estado", "FechaSolicitud" });

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesCompetencias_PacienteId_Estado_FechaInicio",
                table: "SolicitudesCompetencias",
                columns: new[] { "PacienteId", "Estado", "FechaInicio" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SolicitudesCompetencias");
        }
    }
}
