using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Termales.DAL.Migrations
{
    /// <inheritdoc />
    public partial class FijarDefaultMetodoPago : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Filas existentes creadas antes de que el default se corrigiera pudieron
            // quedar con metodo_pago=0, que no corresponde a ningún valor del enum
            // (Efectivo=1 es el mínimo válido) — se normalizan a Efectivo.
            migrationBuilder.Sql("UPDATE public.comprobantes SET metodo_pago = 1 WHERE metodo_pago = 0;");

            migrationBuilder.AlterColumn<int>(
                name: "metodo_pago",
                schema: "public",
                table: "comprobantes",
                type: "integer",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "integer");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "metodo_pago",
                schema: "public",
                table: "comprobantes",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 1);
        }
    }
}
