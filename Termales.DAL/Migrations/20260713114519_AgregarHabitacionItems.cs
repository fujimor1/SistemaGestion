using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Termales.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AgregarHabitacionItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "habitacion_items",
                schema: "public",
                columns: table => new
                {
                    habitacion_item_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    habitacion_id = table.Column<int>(type: "integer", nullable: false),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    cantidad = table.Column<int>(type: "integer", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_habitacion_items", x => x.habitacion_item_id);
                    table.ForeignKey(
                        name: "FK_habitacion_items_habitaciones_habitacion_id",
                        column: x => x.habitacion_id,
                        principalSchema: "public",
                        principalTable: "habitaciones",
                        principalColumn: "habitacion_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_habitacion_items_habitacion_id",
                schema: "public",
                table: "habitacion_items",
                column: "habitacion_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "habitacion_items",
                schema: "public");
        }
    }
}
