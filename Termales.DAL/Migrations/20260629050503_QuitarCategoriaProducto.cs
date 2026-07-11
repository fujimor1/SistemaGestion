using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Termales.DAL.Migrations
{
    /// <inheritdoc />
    public partial class QuitarCategoriaProducto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "categoria",
                schema: "tienda",
                table: "productos");

            migrationBuilder.AlterColumn<string>(
                name: "descripcion",
                schema: "tienda",
                table: "productos",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "----",
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "descripcion",
                schema: "tienda",
                table: "productos",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldDefaultValue: "----");

            migrationBuilder.AddColumn<string>(
                name: "categoria",
                schema: "tienda",
                table: "productos",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);
        }
    }
}
