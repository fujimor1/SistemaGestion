using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Termales.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AgregarFiadoAComprobante : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "cliente_id",
                schema: "public",
                table: "comprobantes",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "cobrado",
                schema: "public",
                table: "comprobantes",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "fecha_cobro",
                schema: "public",
                table: "comprobantes",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "metodo_pago",
                schema: "public",
                table: "comprobantes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_comprobantes_cliente_id",
                schema: "public",
                table: "comprobantes",
                column: "cliente_id");

            migrationBuilder.AddForeignKey(
                name: "FK_comprobantes_clientes_cliente_id",
                schema: "public",
                table: "comprobantes",
                column: "cliente_id",
                principalSchema: "public",
                principalTable: "clientes",
                principalColumn: "cliente_id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_comprobantes_clientes_cliente_id",
                schema: "public",
                table: "comprobantes");

            migrationBuilder.DropIndex(
                name: "IX_comprobantes_cliente_id",
                schema: "public",
                table: "comprobantes");

            migrationBuilder.DropColumn(
                name: "cliente_id",
                schema: "public",
                table: "comprobantes");

            migrationBuilder.DropColumn(
                name: "cobrado",
                schema: "public",
                table: "comprobantes");

            migrationBuilder.DropColumn(
                name: "fecha_cobro",
                schema: "public",
                table: "comprobantes");

            migrationBuilder.DropColumn(
                name: "metodo_pago",
                schema: "public",
                table: "comprobantes");
        }
    }
}
