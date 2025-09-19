using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class AddSkuAndPreciosFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Productos_KioscoId",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "Precio",
                table: "Productos");

            migrationBuilder.AddColumn<decimal>(
                name: "PrecioCompra",
                table: "Productos",
                type: "numeric(18,2)",
                nullable: false);

            migrationBuilder.AddColumn<decimal>(
                name: "PrecioVenta",
                table: "Productos",
                type: "numeric(18,2)",
                nullable: false);

            migrationBuilder.AddColumn<string>(
                name: "Sku",
                table: "Productos",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Productos_KioscoId_Sku",
                table: "Productos",
                columns: new[] { "KioscoId", "Sku" });

            // Añadir restricciones CHECK para asegurar valores positivos
            migrationBuilder.Sql("ALTER TABLE \"Productos\" ADD CONSTRAINT CK_Productos_PrecioCompra_Positive CHECK (\"PrecioCompra\" > 0);");
            migrationBuilder.Sql("ALTER TABLE \"Productos\" ADD CONSTRAINT CK_Productos_PrecioVenta_Positive CHECK (\"PrecioVenta\" > 0);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Productos_KioscoId_Sku",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "PrecioCompra",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "PrecioVenta",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "Sku",
                table: "Productos");

            migrationBuilder.AddColumn<decimal>(
                name: "Precio",
                table: "Productos",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_Productos_KioscoId",
                table: "Productos",
                column: "KioscoId");

            migrationBuilder.Sql("ALTER TABLE \"Productos\" DROP CONSTRAINT IF EXISTS CK_Productos_PrecioCompra_Positive;");
            migrationBuilder.Sql("ALTER TABLE \"Productos\" DROP CONSTRAINT IF EXISTS CK_Productos_PrecioVenta_Positive;");
        }
    }
}
