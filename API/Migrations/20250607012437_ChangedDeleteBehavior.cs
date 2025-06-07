using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class ChangedDeleteBehavior : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Productos_Kioscos_KioscoId",
                table: "Productos");

            migrationBuilder.DropForeignKey(
                name: "FK_Ventas_Kioscos_KioscoId",
                table: "Ventas");

            migrationBuilder.AddForeignKey(
                name: "FK_Productos_Kioscos_KioscoId",
                table: "Productos",
                column: "KioscoId",
                principalTable: "Kioscos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Ventas_Kioscos_KioscoId",
                table: "Ventas",
                column: "KioscoId",
                principalTable: "Kioscos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Productos_Kioscos_KioscoId",
                table: "Productos");

            migrationBuilder.DropForeignKey(
                name: "FK_Ventas_Kioscos_KioscoId",
                table: "Ventas");

            migrationBuilder.AddForeignKey(
                name: "FK_Productos_Kioscos_KioscoId",
                table: "Productos",
                column: "KioscoId",
                principalTable: "Kioscos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Ventas_Kioscos_KioscoId",
                table: "Ventas",
                column: "KioscoId",
                principalTable: "Kioscos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
