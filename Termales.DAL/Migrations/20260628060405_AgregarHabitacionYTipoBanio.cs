using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Termales.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AgregarHabitacionYTipoBanio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "comedor");

            migrationBuilder.AddColumn<int>(
                name: "tipo_banio",
                schema: "public",
                table: "piscinas",
                type: "integer",
                nullable: false,
                defaultValue: 2);

            migrationBuilder.CreateTable(
                name: "categorias_menu",
                schema: "comedor",
                columns: table => new
                {
                    categoria_menu_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categorias_menu", x => x.categoria_menu_id);
                });

            migrationBuilder.CreateTable(
                name: "habitaciones",
                schema: "public",
                columns: table => new
                {
                    habitacion_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    descripcion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    capacidad = table.Column<int>(type: "integer", nullable: false),
                    ocupado = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_habitaciones", x => x.habitacion_id);
                });

            migrationBuilder.CreateTable(
                name: "mesas",
                schema: "comedor",
                columns: table => new
                {
                    mesa_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    numero = table.Column<int>(type: "integer", nullable: false),
                    capacidad = table.Column<int>(type: "integer", nullable: false),
                    estado = table.Column<int>(type: "integer", nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mesas", x => x.mesa_id);
                });

            migrationBuilder.CreateTable(
                name: "items_menu",
                schema: "comedor",
                columns: table => new
                {
                    item_menu_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    categoria_menu_id = table.Column<int>(type: "integer", nullable: false),
                    nombre = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    descripcion = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    precio = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_items_menu", x => x.item_menu_id);
                    table.ForeignKey(
                        name: "FK_items_menu_categorias_menu_categoria_menu_id",
                        column: x => x.categoria_menu_id,
                        principalSchema: "comedor",
                        principalTable: "categorias_menu",
                        principalColumn: "categoria_menu_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ordenes",
                schema: "comedor",
                columns: table => new
                {
                    orden_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    mesa_id = table.Column<int>(type: "integer", nullable: false),
                    usuario_id = table.Column<int>(type: "integer", nullable: false),
                    estado = table.Column<int>(type: "integer", nullable: false),
                    total = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    observaciones = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    fecha_apertura = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    fecha_cierre = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ordenes", x => x.orden_id);
                    table.ForeignKey(
                        name: "FK_ordenes_mesas_mesa_id",
                        column: x => x.mesa_id,
                        principalSchema: "comedor",
                        principalTable: "mesas",
                        principalColumn: "mesa_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ordenes_usuarios_usuario_id",
                        column: x => x.usuario_id,
                        principalSchema: "seguridad",
                        principalTable: "usuarios",
                        principalColumn: "usuario_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "orden_detalles",
                schema: "comedor",
                columns: table => new
                {
                    orden_detalle_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    orden_id = table.Column<int>(type: "integer", nullable: false),
                    item_menu_id = table.Column<int>(type: "integer", nullable: false),
                    cantidad = table.Column<int>(type: "integer", nullable: false),
                    precio_unitario = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    estado = table.Column<int>(type: "integer", nullable: false),
                    observaciones = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_orden_detalles", x => x.orden_detalle_id);
                    table.ForeignKey(
                        name: "FK_orden_detalles_items_menu_item_menu_id",
                        column: x => x.item_menu_id,
                        principalSchema: "comedor",
                        principalTable: "items_menu",
                        principalColumn: "item_menu_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_orden_detalles_ordenes_orden_id",
                        column: x => x.orden_id,
                        principalSchema: "comedor",
                        principalTable: "ordenes",
                        principalColumn: "orden_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_items_menu_categoria_menu_id",
                schema: "comedor",
                table: "items_menu",
                column: "categoria_menu_id");

            migrationBuilder.CreateIndex(
                name: "IX_mesas_numero",
                schema: "comedor",
                table: "mesas",
                column: "numero",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_orden_detalles_item_menu_id",
                schema: "comedor",
                table: "orden_detalles",
                column: "item_menu_id");

            migrationBuilder.CreateIndex(
                name: "IX_orden_detalles_orden_id",
                schema: "comedor",
                table: "orden_detalles",
                column: "orden_id");

            migrationBuilder.CreateIndex(
                name: "IX_ordenes_mesa_id",
                schema: "comedor",
                table: "ordenes",
                column: "mesa_id");

            migrationBuilder.CreateIndex(
                name: "IX_ordenes_usuario_id",
                schema: "comedor",
                table: "ordenes",
                column: "usuario_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "habitaciones",
                schema: "public");

            migrationBuilder.DropTable(
                name: "orden_detalles",
                schema: "comedor");

            migrationBuilder.DropTable(
                name: "items_menu",
                schema: "comedor");

            migrationBuilder.DropTable(
                name: "ordenes",
                schema: "comedor");

            migrationBuilder.DropTable(
                name: "categorias_menu",
                schema: "comedor");

            migrationBuilder.DropTable(
                name: "mesas",
                schema: "comedor");

            migrationBuilder.DropColumn(
                name: "tipo_banio",
                schema: "public",
                table: "piscinas");
        }
    }
}
