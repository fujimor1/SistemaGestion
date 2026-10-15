using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Termales.DAL.Migrations
{
    /// <inheritdoc />
    public partial class ClienteEmpresaRuc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Agregar tipo_documento con default 'DNI' para los registros existentes
            migrationBuilder.AddColumn<string>(
                name: "tipo_documento",
                table: "clientes",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "DNI");

            // 2. Agregar numero_documento copiando el valor actual de dni
            migrationBuilder.AddColumn<string>(
                name: "numero_documento",
                table: "clientes",
                type: "character varying(11)",
                maxLength: 11,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("UPDATE clientes SET numero_documento = dni WHERE numero_documento = ''");

            // 3. Agregar razon_social (nullable)
            migrationBuilder.AddColumn<string>(
                name: "razon_social",
                table: "clientes",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            // 4. Eliminar el índice único antiguo de dni y crear uno nuevo en numero_documento
            migrationBuilder.DropIndex(
                name: "IX_clientes_dni",
                table: "clientes");

            migrationBuilder.CreateIndex(
                name: "IX_clientes_numero_documento",
                table: "clientes",
                column: "numero_documento",
                unique: true);

            // 5. Hacer nombres y apellidos nullable
            migrationBuilder.AlterColumn<string>(
                name: "nombres",
                table: "clientes",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "apellidos",
                table: "clientes",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_clientes_numero_documento",
                table: "clientes");

            migrationBuilder.DropColumn(name: "tipo_documento", table: "clientes");
            migrationBuilder.DropColumn(name: "numero_documento", table: "clientes");
            migrationBuilder.DropColumn(name: "razon_social", table: "clientes");

            migrationBuilder.CreateIndex(
                name: "IX_clientes_dni",
                table: "clientes",
                column: "dni",
                unique: true);

            migrationBuilder.AlterColumn<string>(
                name: "nombres",
                table: "clientes",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "apellidos",
                table: "clientes",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);
        }
    }
}
