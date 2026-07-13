using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Termales.DAL.Migrations
{
    /// <inheritdoc />
    public partial class UnirMesasYPedidosLlevar : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "mesa_id",
                schema: "comedor",
                table: "ordenes",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<string>(
                name: "tipo_entrega",
                schema: "comedor",
                table: "ordenes",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "comedor");

            migrationBuilder.AddColumn<int>(
                name: "mesa_principal_id",
                schema: "comedor",
                table: "mesas",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_mesas_mesa_principal_id",
                schema: "comedor",
                table: "mesas",
                column: "mesa_principal_id");

            migrationBuilder.AddForeignKey(
                name: "FK_mesas_mesas_mesa_principal_id",
                schema: "comedor",
                table: "mesas",
                column: "mesa_principal_id",
                principalSchema: "comedor",
                principalTable: "mesas",
                principalColumn: "mesa_id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_mesas_mesas_mesa_principal_id",
                schema: "comedor",
                table: "mesas");

            migrationBuilder.DropIndex(
                name: "IX_mesas_mesa_principal_id",
                schema: "comedor",
                table: "mesas");

            migrationBuilder.DropColumn(
                name: "tipo_entrega",
                schema: "comedor",
                table: "ordenes");

            migrationBuilder.DropColumn(
                name: "mesa_principal_id",
                schema: "comedor",
                table: "mesas");

            migrationBuilder.AlterColumn<int>(
                name: "mesa_id",
                schema: "comedor",
                table: "ordenes",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);
        }
    }
}
