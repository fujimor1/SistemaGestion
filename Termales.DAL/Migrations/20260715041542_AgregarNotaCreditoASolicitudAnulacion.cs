using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Termales.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AgregarNotaCreditoASolicitudAnulacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "nota_credito_comprobante_id",
                schema: "public",
                table: "solicitudes_anulacion",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_solicitudes_anulacion_nota_credito_comprobante_id",
                schema: "public",
                table: "solicitudes_anulacion",
                column: "nota_credito_comprobante_id");

            migrationBuilder.AddForeignKey(
                name: "FK_solicitudes_anulacion_comprobantes_nota_credito_comprobante~",
                schema: "public",
                table: "solicitudes_anulacion",
                column: "nota_credito_comprobante_id",
                principalSchema: "public",
                principalTable: "comprobantes",
                principalColumn: "comprobante_id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_solicitudes_anulacion_comprobantes_nota_credito_comprobante~",
                schema: "public",
                table: "solicitudes_anulacion");

            migrationBuilder.DropIndex(
                name: "IX_solicitudes_anulacion_nota_credito_comprobante_id",
                schema: "public",
                table: "solicitudes_anulacion");

            migrationBuilder.DropColumn(
                name: "nota_credito_comprobante_id",
                schema: "public",
                table: "solicitudes_anulacion");
        }
    }
}
