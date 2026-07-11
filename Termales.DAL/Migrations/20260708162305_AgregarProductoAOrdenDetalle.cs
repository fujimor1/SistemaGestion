using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Termales.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AgregarProductoAOrdenDetalle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "item_menu_id",
                schema: "comedor",
                table: "orden_detalles",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "producto_id",
                schema: "comedor",
                table: "orden_detalles",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_orden_detalles_producto_id",
                schema: "comedor",
                table: "orden_detalles",
                column: "producto_id");

            migrationBuilder.AddCheckConstraint(
                name: "CK_orden_detalles_item_o_producto",
                schema: "comedor",
                table: "orden_detalles",
                sql: "(item_menu_id IS NOT NULL AND producto_id IS NULL) OR (item_menu_id IS NULL AND producto_id IS NOT NULL)");

            migrationBuilder.AddForeignKey(
                name: "FK_orden_detalles_productos_producto_id",
                schema: "comedor",
                table: "orden_detalles",
                column: "producto_id",
                principalSchema: "tienda",
                principalTable: "productos",
                principalColumn: "producto_id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_orden_detalles_productos_producto_id",
                schema: "comedor",
                table: "orden_detalles");

            migrationBuilder.DropIndex(
                name: "IX_orden_detalles_producto_id",
                schema: "comedor",
                table: "orden_detalles");

            migrationBuilder.DropCheckConstraint(
                name: "CK_orden_detalles_item_o_producto",
                schema: "comedor",
                table: "orden_detalles");

            migrationBuilder.DropColumn(
                name: "producto_id",
                schema: "comedor",
                table: "orden_detalles");

            migrationBuilder.AlterColumn<int>(
                name: "item_menu_id",
                schema: "comedor",
                table: "orden_detalles",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);
        }
    }
}
