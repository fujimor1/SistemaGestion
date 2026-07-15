using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Termales.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AgregarMontoEfectivoMixto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "monto_efectivo_mixto",
                schema: "public",
                table: "comprobantes",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "monto_efectivo_mixto",
                schema: "public",
                table: "comprobantes");
        }
    }
}
