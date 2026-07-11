using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Termales.DAL.Migrations
{
    /// <inheritdoc />
    public partial class FlujoAprobacionAnulacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "solicitudes_anulacion",
                schema: "public",
                columns: table => new
                {
                    solicitud_anulacion_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    comprobante_id = table.Column<int>(type: "integer", nullable: false),
                    motivo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    solicitado_por = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    estado_anterior_comprobante = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    fecha_solicitud = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    resuelto_por = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    fecha_resolucion = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    motivo_rechazo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_solicitudes_anulacion", x => x.solicitud_anulacion_id);
                    table.ForeignKey(
                        name: "FK_solicitudes_anulacion_comprobantes_comprobante_id",
                        column: x => x.comprobante_id,
                        principalSchema: "public",
                        principalTable: "comprobantes",
                        principalColumn: "comprobante_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_solicitudes_anulacion_comprobante_id",
                schema: "public",
                table: "solicitudes_anulacion",
                column: "comprobante_id");

            migrationBuilder.CreateIndex(
                name: "IX_solicitudes_anulacion_estado",
                schema: "public",
                table: "solicitudes_anulacion",
                column: "estado");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "solicitudes_anulacion",
                schema: "public");
        }
    }
}
