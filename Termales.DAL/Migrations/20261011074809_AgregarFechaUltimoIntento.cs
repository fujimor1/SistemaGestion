using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Termales.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AgregarFechaUltimoIntento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "fecha_ultimo_intento",
                schema: "public",
                table: "comprobantes_sunat",
                type: "timestamp without time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "fecha_ultimo_intento",
                schema: "public",
                table: "comprobantes_sunat");
        }
    }
}
