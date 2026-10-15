using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NutriApi.Migrations
{
    /// <inheritdoc />
    public partial class AgregarObjetivosNutricionales : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ObjetivoCalorias",
                table: "Dietas",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ObjetivoCarbohidratos",
                table: "Dietas",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ObjetivoGrasas",
                table: "Dietas",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ObjetivoProteinas",
                table: "Dietas",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ObjetivoCalorias",
                table: "Comidas",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ObjetivoCarbohidratos",
                table: "Comidas",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ObjetivoGrasas",
                table: "Comidas",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ObjetivoProteinas",
                table: "Comidas",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ObjetivoCalorias",
                table: "Dietas");

            migrationBuilder.DropColumn(
                name: "ObjetivoCarbohidratos",
                table: "Dietas");

            migrationBuilder.DropColumn(
                name: "ObjetivoGrasas",
                table: "Dietas");

            migrationBuilder.DropColumn(
                name: "ObjetivoProteinas",
                table: "Dietas");

            migrationBuilder.DropColumn(
                name: "ObjetivoCalorias",
                table: "Comidas");

            migrationBuilder.DropColumn(
                name: "ObjetivoCarbohidratos",
                table: "Comidas");

            migrationBuilder.DropColumn(
                name: "ObjetivoGrasas",
                table: "Comidas");

            migrationBuilder.DropColumn(
                name: "ObjetivoProteinas",
                table: "Comidas");
        }
    }
}
