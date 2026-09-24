using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NutriApi.Migrations
{
    /// <inheritdoc />
    public partial class SimplificarRegistroDiario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdherenciaPorcentaje",
                table: "RegistrosDiariosPacientes");

            migrationBuilder.DropColumn(
                name: "Energia",
                table: "RegistrosDiariosPacientes");

            migrationBuilder.DropColumn(
                name: "Hambre",
                table: "RegistrosDiariosPacientes");

            migrationBuilder.DropColumn(
                name: "Entreno",
                table: "RegistrosDiariosPacientes");

            migrationBuilder.AddColumn<bool>(
                name: "CumplioPlan",
                table: "RegistrosDiariosPacientes",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CaderaCm",
                table: "RegistrosDiariosPacientes",
                type: "numeric(6,2)",
                precision: 6,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CinturaCm",
                table: "RegistrosDiariosPacientes",
                type: "numeric(6,2)",
                precision: 6,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CuelloCm",
                table: "RegistrosDiariosPacientes",
                type: "numeric(6,2)",
                precision: 6,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "GemeloCm",
                table: "RegistrosDiariosPacientes",
                type: "numeric(6,2)",
                precision: 6,
                scale: 2,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CaderaCm",
                table: "RegistrosDiariosPacientes");

            migrationBuilder.DropColumn(
                name: "CinturaCm",
                table: "RegistrosDiariosPacientes");

            migrationBuilder.DropColumn(
                name: "CuelloCm",
                table: "RegistrosDiariosPacientes");

            migrationBuilder.DropColumn(
                name: "GemeloCm",
                table: "RegistrosDiariosPacientes");

            migrationBuilder.DropColumn(
                name: "CumplioPlan",
                table: "RegistrosDiariosPacientes");

            migrationBuilder.AddColumn<bool>(
                name: "Entreno",
                table: "RegistrosDiariosPacientes",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AdherenciaPorcentaje",
                table: "RegistrosDiariosPacientes",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Energia",
                table: "RegistrosDiariosPacientes",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Hambre",
                table: "RegistrosDiariosPacientes",
                type: "integer",
                nullable: true);
        }
    }
}
