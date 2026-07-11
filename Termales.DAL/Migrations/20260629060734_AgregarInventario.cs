using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Termales.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AgregarInventario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "inventario");

            migrationBuilder.AddColumn<decimal>(
                name: "precio_compra",
                schema: "tienda",
                table: "productos",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "entradas_producto",
                schema: "inventario",
                columns: table => new
                {
                    entrada_producto_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    producto_id = table.Column<int>(type: "integer", nullable: false),
                    cantidad = table.Column<int>(type: "integer", nullable: false),
                    precio_unitario = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    total = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    fecha = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    observacion = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_entradas_producto", x => x.entrada_producto_id);
                    table.ForeignKey(
                        name: "FK_entradas_producto_productos_producto_id",
                        column: x => x.producto_id,
                        principalSchema: "tienda",
                        principalTable: "productos",
                        principalColumn: "producto_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "insumos",
                schema: "inventario",
                columns: table => new
                {
                    insumo_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    tipo_ambiente = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    tipo_articulo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "insumo"),
                    unidad = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    stock_actual = table.Column<decimal>(type: "numeric(12,3)", precision: 12, scale: 3, nullable: false, defaultValue: 0m),
                    precio_referencia = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false, defaultValue: 0m),
                    activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    fecha_registro = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_insumos", x => x.insumo_id);
                });

            migrationBuilder.CreateTable(
                name: "entradas_insumo",
                schema: "inventario",
                columns: table => new
                {
                    entrada_insumo_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    insumo_id = table.Column<int>(type: "integer", nullable: false),
                    cantidad = table.Column<decimal>(type: "numeric(12,3)", precision: 12, scale: 3, nullable: false),
                    precio_unitario = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    total = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    fecha = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    observacion = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_entradas_insumo", x => x.entrada_insumo_id);
                    table.ForeignKey(
                        name: "FK_entradas_insumo_insumos_insumo_id",
                        column: x => x.insumo_id,
                        principalSchema: "inventario",
                        principalTable: "insumos",
                        principalColumn: "insumo_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_entradas_insumo_insumo_id",
                schema: "inventario",
                table: "entradas_insumo",
                column: "insumo_id");

            migrationBuilder.CreateIndex(
                name: "IX_entradas_producto_producto_id",
                schema: "inventario",
                table: "entradas_producto",
                column: "producto_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "entradas_insumo",
                schema: "inventario");

            migrationBuilder.DropTable(
                name: "entradas_producto",
                schema: "inventario");

            migrationBuilder.DropTable(
                name: "insumos",
                schema: "inventario");

            migrationBuilder.DropColumn(
                name: "precio_compra",
                schema: "tienda",
                table: "productos");
        }
    }
}
