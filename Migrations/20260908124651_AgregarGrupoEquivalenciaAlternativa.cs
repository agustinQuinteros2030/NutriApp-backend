using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NutriApi.Migrations
{
    /// <inheritdoc />
    public partial class AgregarGrupoEquivalenciaAlternativa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GrupoEquivalenciaId",
                table: "AlternativasItemsComidas",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_AlternativasItemsComidas_GrupoEquivalenciaId",
                table: "AlternativasItemsComidas",
                column: "GrupoEquivalenciaId");

            migrationBuilder.AddForeignKey(
                name: "FK_AlternativasItemsComidas_GruposEquivalencias_GrupoEquivalen~",
                table: "AlternativasItemsComidas",
                column: "GrupoEquivalenciaId",
                principalTable: "GruposEquivalencias",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AlternativasItemsComidas_GruposEquivalencias_GrupoEquivalen~",
                table: "AlternativasItemsComidas");

            migrationBuilder.DropIndex(
                name: "IX_AlternativasItemsComidas_GrupoEquivalenciaId",
                table: "AlternativasItemsComidas");

            migrationBuilder.DropColumn(
                name: "GrupoEquivalenciaId",
                table: "AlternativasItemsComidas");
        }
    }
}
