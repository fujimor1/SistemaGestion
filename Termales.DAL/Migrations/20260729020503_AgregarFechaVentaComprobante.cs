using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Termales.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AgregarFechaVentaComprobante : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "fecha_venta",
                schema: "public",
                table: "comprobantes",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            // Backfill: por defecto la venta ocurrió el mismo día en que se emitió el
            // documento — salvo los comprobantes que nacieron de un canje NV -> BI/FI,
            // donde el documento nuevo se emitió otro día pero el dinero ya había
            // entrado a caja el día de la Nota de Venta original.
            migrationBuilder.Sql(@"
                UPDATE comprobantes SET fecha_venta = fecha_emision;

                UPDATE comprobantes c
                SET fecha_venta = o.fecha_emision
                FROM comprobantes o
                WHERE c.comprobante_origen_id = o.comprobante_id
                  AND o.""TipoComprobante"" = 'NV';
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "fecha_venta",
                schema: "public",
                table: "comprobantes");
        }
    }
}
