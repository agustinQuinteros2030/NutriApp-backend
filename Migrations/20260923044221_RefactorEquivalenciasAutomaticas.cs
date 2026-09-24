using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NutriApi.Migrations
{
    /// <inheritdoc />
    public partial class RefactorEquivalenciasAutomaticas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CantidadEquivalente",
                table: "EquivalenciasAlimentos");

            migrationBuilder.DropColumn(
                name: "UnidadMedida",
                table: "EquivalenciasAlimentos");

            migrationBuilder.AddColumn<int>(
                name: "Criterio",
                table: "GruposEquivalencias",
                type: "integer",
                nullable: false,
                defaultValue: 1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Criterio",
                table: "GruposEquivalencias");

            migrationBuilder.AddColumn<decimal>(
                name: "CantidadEquivalente",
                table: "EquivalenciasAlimentos",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "UnidadMedida",
                table: "EquivalenciasAlimentos",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
