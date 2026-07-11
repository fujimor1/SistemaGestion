using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Termales.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AgregarModuloCaja : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "caja");

            migrationBuilder.CreateTable(
                name: "aperturas_caja",
                schema: "caja",
                columns: table => new
                {
                    apertura_caja_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    fecha = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    monto_inicial = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    responsable = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    observaciones = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    fecha_registro = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_aperturas_caja", x => x.apertura_caja_id);
                });

            migrationBuilder.CreateTable(
                name: "cierres_caja",
                schema: "caja",
                columns: table => new
                {
                    cierre_caja_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    fecha = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    total_sistema = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    efectivo_fisico = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    yape_fisico = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    transferencia_fisico = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    total_egresos = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    monto_apertura = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    diferencia = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    observaciones = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    encargado_cierre = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    fecha_registro = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cierres_caja", x => x.cierre_caja_id);
                });

            migrationBuilder.CreateTable(
                name: "egresos_caja_chica",
                schema: "caja",
                columns: table => new
                {
                    egreso_caja_chica_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    fecha = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "now()"),
                    concepto = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    monto = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    responsable = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    tipo_documento = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    numero_documento = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    registrado_por = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    observaciones = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_egresos_caja_chica", x => x.egreso_caja_chica_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_aperturas_caja_fecha",
                schema: "caja",
                table: "aperturas_caja",
                column: "fecha");

            migrationBuilder.CreateIndex(
                name: "IX_cierres_caja_fecha",
                schema: "caja",
                table: "cierres_caja",
                column: "fecha",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_egresos_caja_chica_fecha",
                schema: "caja",
                table: "egresos_caja_chica",
                column: "fecha");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "aperturas_caja",
                schema: "caja");

            migrationBuilder.DropTable(
                name: "cierres_caja",
                schema: "caja");

            migrationBuilder.DropTable(
                name: "egresos_caja_chica",
                schema: "caja");
        }
    }
}
