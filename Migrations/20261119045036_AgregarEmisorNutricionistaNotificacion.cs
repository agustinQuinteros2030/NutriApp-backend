using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NutriApi.Migrations
{
    /// <inheritdoc />
    public partial class AgregarEmisorNutricionistaNotificacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EmisorNutricionistaId",
                table: "Notificaciones",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notificaciones_EmisorNutricionistaId",
                table: "Notificaciones",
                column: "EmisorNutricionistaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Notificaciones_AspNetUsers_EmisorNutricionistaId",
                table: "Notificaciones",
                column: "EmisorNutricionistaId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notificaciones_AspNetUsers_EmisorNutricionistaId",
                table: "Notificaciones");

            migrationBuilder.DropIndex(
                name: "IX_Notificaciones_EmisorNutricionistaId",
                table: "Notificaciones");

            migrationBuilder.DropColumn(
                name: "EmisorNutricionistaId",
                table: "Notificaciones");
        }
    }
}
