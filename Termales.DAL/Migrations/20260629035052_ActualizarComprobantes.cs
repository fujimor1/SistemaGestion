using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Termales.DAL.Migrations
{
    /// <inheritdoc />
    public partial class ActualizarComprobantes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Cajero",
                schema: "public",
                table: "comprobantes",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClienteRazonSocial",
                schema: "public",
                table: "comprobantes",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClienteRuc",
                schema: "public",
                table: "comprobantes",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Estado",
                schema: "public",
                table: "comprobantes",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "Impuesto",
                schema: "public",
                table: "comprobantes",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Local",
                schema: "public",
                table: "comprobantes",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Moneda",
                schema: "public",
                table: "comprobantes",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TipoComprobante",
                schema: "public",
                table: "comprobantes",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "TotalGravada",
                schema: "public",
                table: "comprobantes",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Cajero",
                schema: "public",
                table: "comprobantes");

            migrationBuilder.DropColumn(
                name: "ClienteRazonSocial",
                schema: "public",
                table: "comprobantes");

            migrationBuilder.DropColumn(
                name: "ClienteRuc",
                schema: "public",
                table: "comprobantes");

            migrationBuilder.DropColumn(
                name: "Estado",
                schema: "public",
                table: "comprobantes");

            migrationBuilder.DropColumn(
                name: "Impuesto",
                schema: "public",
                table: "comprobantes");

            migrationBuilder.DropColumn(
                name: "Local",
                schema: "public",
                table: "comprobantes");

            migrationBuilder.DropColumn(
                name: "Moneda",
                schema: "public",
                table: "comprobantes");

            migrationBuilder.DropColumn(
                name: "TipoComprobante",
                schema: "public",
                table: "comprobantes");

            migrationBuilder.DropColumn(
                name: "TotalGravada",
                schema: "public",
                table: "comprobantes");
        }
    }
}
