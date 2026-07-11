using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Termales.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AgregarRecetaInsumo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "receta_insumos",
                schema: "comedor",
                columns: table => new
                {
                    receta_insumo_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    item_menu_id = table.Column<int>(type: "integer", nullable: false),
                    insumo_id = table.Column<int>(type: "integer", nullable: false),
                    cantidad = table.Column<decimal>(type: "numeric(12,3)", precision: 12, scale: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_receta_insumos", x => x.receta_insumo_id);
                    table.ForeignKey(
                        name: "FK_receta_insumos_insumos_insumo_id",
                        column: x => x.insumo_id,
                        principalSchema: "inventario",
                        principalTable: "insumos",
                        principalColumn: "insumo_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_receta_insumos_items_menu_item_menu_id",
                        column: x => x.item_menu_id,
                        principalSchema: "comedor",
                        principalTable: "items_menu",
                        principalColumn: "item_menu_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_receta_insumos_insumo_id",
                schema: "comedor",
                table: "receta_insumos",
                column: "insumo_id");

            migrationBuilder.CreateIndex(
                name: "IX_receta_insumos_item_menu_id",
                schema: "comedor",
                table: "receta_insumos",
                column: "item_menu_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "receta_insumos",
                schema: "comedor");
        }
    }
}
