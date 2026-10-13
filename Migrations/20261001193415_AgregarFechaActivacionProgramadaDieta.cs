using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NutriApi.Migrations
{
    /// <inheritdoc />
    public partial class AgregarFechaActivacionProgramadaDieta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "FechaActivacionProgramada",
                table: "Dietas",
                type: "date",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FechaActivacionProgramada",
                table: "Dietas");
        }
    }
}
