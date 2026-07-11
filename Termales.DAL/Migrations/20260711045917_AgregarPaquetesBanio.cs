using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Termales.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AgregarPaquetesBanio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "paquetes_banio",
                schema: "public",
                columns: table => new
                {
                    paquete_banio_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    precio = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_paquetes_banio", x => x.paquete_banio_id);
                });

            migrationBuilder.CreateTable(
                name: "paquete_banio_tipo_servicios",
                schema: "public",
                columns: table => new
                {
                    paquete_banio_id = table.Column<int>(type: "integer", nullable: false),
                    tipo_servicio_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_paquete_banio_tipo_servicios", x => new { x.paquete_banio_id, x.tipo_servicio_id });
                    table.ForeignKey(
                        name: "FK_paquete_banio_tipo_servicios_paquetes_banio_paquete_banio_id",
                        column: x => x.paquete_banio_id,
                        principalSchema: "public",
                        principalTable: "paquetes_banio",
                        principalColumn: "paquete_banio_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_paquete_banio_tipo_servicios_tipo_servicios_tipo_servicio_id",
                        column: x => x.tipo_servicio_id,
                        principalSchema: "public",
                        principalTable: "tipo_servicios",
                        principalColumn: "tipo_servicio_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "tipo_servicios",
                keyColumn: "tipo_servicio_id",
                keyValue: 1,
                column: "precio_por_persona",
                value: 5.00m);

            migrationBuilder.UpdateData(
                schema: "public",
                table: "tipo_servicios",
                keyColumn: "tipo_servicio_id",
                keyValue: 2,
                column: "precio_por_persona",
                value: 5.00m);

            migrationBuilder.CreateIndex(
                name: "IX_paquete_banio_tipo_servicios_tipo_servicio_id",
                schema: "public",
                table: "paquete_banio_tipo_servicios",
                column: "tipo_servicio_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "paquete_banio_tipo_servicios",
                schema: "public");

            migrationBuilder.DropTable(
                name: "paquetes_banio",
                schema: "public");

            migrationBuilder.UpdateData(
                schema: "public",
                table: "tipo_servicios",
                keyColumn: "tipo_servicio_id",
                keyValue: 1,
                column: "precio_por_persona",
                value: 15.00m);

            migrationBuilder.UpdateData(
                schema: "public",
                table: "tipo_servicios",
                keyColumn: "tipo_servicio_id",
                keyValue: 2,
                column: "precio_por_persona",
                value: 25.00m);
        }
    }
}
