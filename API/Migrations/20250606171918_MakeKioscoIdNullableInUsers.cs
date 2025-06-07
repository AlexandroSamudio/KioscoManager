using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class MakeKioscoIdNullableInUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "KioscoId",
                table: "AspNetUsers",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DO $$
                BEGIN
                  IF EXISTS (SELECT 1 FROM "AspNetUsers" WHERE "KioscoId" IS NULL) THEN
                    RAISE EXCEPTION 'No se puede revertir la migración porque hay usuarios con KioscoId nulo.';
                  END IF;
                END $$;
                """
            );

            migrationBuilder.AlterColumn<int>(
                name: "KioscoId",
                table: "AspNetUsers",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);
        }
    }
}
