using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Termales.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AgregarCompraIdEnEgresoCajaChica : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "compra_id",
                schema: "caja",
                table: "egresos_caja_chica",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_egresos_caja_chica_compra_id",
                schema: "caja",
                table: "egresos_caja_chica",
                column: "compra_id");

            migrationBuilder.AddForeignKey(
                name: "FK_egresos_caja_chica_compras_compra_id",
                schema: "caja",
                table: "egresos_caja_chica",
                column: "compra_id",
                principalSchema: "compras",
                principalTable: "compras",
                principalColumn: "compra_id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_egresos_caja_chica_compras_compra_id",
                schema: "caja",
                table: "egresos_caja_chica");

            migrationBuilder.DropIndex(
                name: "IX_egresos_caja_chica_compra_id",
                schema: "caja",
                table: "egresos_caja_chica");

            migrationBuilder.DropColumn(
                name: "compra_id",
                schema: "caja",
                table: "egresos_caja_chica");
        }
    }
}
