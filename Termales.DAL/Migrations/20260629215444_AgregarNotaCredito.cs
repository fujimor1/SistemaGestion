using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Termales.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AgregarNotaCredito : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "comprobante_origen_id",
                schema: "public",
                table: "comprobantes",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_comprobantes_comprobante_origen_id",
                schema: "public",
                table: "comprobantes",
                column: "comprobante_origen_id");

            migrationBuilder.AddForeignKey(
                name: "FK_comprobantes_comprobantes_comprobante_origen_id",
                schema: "public",
                table: "comprobantes",
                column: "comprobante_origen_id",
                principalSchema: "public",
                principalTable: "comprobantes",
                principalColumn: "comprobante_id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_comprobantes_comprobantes_comprobante_origen_id",
                schema: "public",
                table: "comprobantes");

            migrationBuilder.DropIndex(
                name: "IX_comprobantes_comprobante_origen_id",
                schema: "public",
                table: "comprobantes");

            migrationBuilder.DropColumn(
                name: "comprobante_origen_id",
                schema: "public",
                table: "comprobantes");
        }
    }
}
