using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Termales.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AgregarMotivoSalidaInsumo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "motivo",
                schema: "inventario",
                table: "salidas_insumo",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "registrado_por",
                schema: "inventario",
                table: "salidas_insumo",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "motivo",
                schema: "inventario",
                table: "salidas_insumo");

            migrationBuilder.DropColumn(
                name: "registrado_por",
                schema: "inventario",
                table: "salidas_insumo");
        }
    }
}
