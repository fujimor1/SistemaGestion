using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Termales.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AgregarDesgloseSistemaCierreCaja : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "efectivo_sistema",
                schema: "caja",
                table: "cierres_caja",
                type: "numeric(12,2)",
                precision: 12,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "yape_sistema",
                schema: "caja",
                table: "cierres_caja",
                type: "numeric(12,2)",
                precision: 12,
                scale: 2,
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "efectivo_sistema",
                schema: "caja",
                table: "cierres_caja");

            migrationBuilder.DropColumn(
                name: "yape_sistema",
                schema: "caja",
                table: "cierres_caja");
        }
    }
}
