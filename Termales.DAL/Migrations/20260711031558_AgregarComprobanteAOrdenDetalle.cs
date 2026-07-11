using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Termales.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AgregarComprobanteAOrdenDetalle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "comprobante_id",
                schema: "comedor",
                table: "orden_detalles",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_orden_detalles_comprobante_id",
                schema: "comedor",
                table: "orden_detalles",
                column: "comprobante_id");

            migrationBuilder.AddForeignKey(
                name: "FK_orden_detalles_comprobantes_comprobante_id",
                schema: "comedor",
                table: "orden_detalles",
                column: "comprobante_id",
                principalSchema: "public",
                principalTable: "comprobantes",
                principalColumn: "comprobante_id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_orden_detalles_comprobantes_comprobante_id",
                schema: "comedor",
                table: "orden_detalles");

            migrationBuilder.DropIndex(
                name: "IX_orden_detalles_comprobante_id",
                schema: "comedor",
                table: "orden_detalles");

            migrationBuilder.DropColumn(
                name: "comprobante_id",
                schema: "comedor",
                table: "orden_detalles");
        }
    }
}
