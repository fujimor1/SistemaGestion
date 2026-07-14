using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Termales.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AgregarComprobanteSunatYSeries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "comprobante_series",
                schema: "public",
                columns: table => new
                {
                    serie = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    tipo_comprobante = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    ultimo_numero = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_comprobante_series", x => x.serie);
                });

            migrationBuilder.CreateTable(
                name: "comprobantes_sunat",
                schema: "public",
                columns: table => new
                {
                    comprobante_id = table.Column<int>(type: "integer", nullable: false),
                    xml_firmado = table.Column<string>(type: "text", nullable: false),
                    hash_digest_value = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    cdr_xml = table.Column<string>(type: "text", nullable: true),
                    cdr_codigo_respuesta = table.Column<int>(type: "integer", nullable: true),
                    cdr_descripcion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    observaciones_sunat = table.Column<string>(type: "text", nullable: true),
                    estado = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    intentos_envio = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    fecha_limite_envio = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    fecha_envio_sunat = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ticket_resumen = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_comprobantes_sunat", x => x.comprobante_id);
                    table.ForeignKey(
                        name: "FK_comprobantes_sunat_comprobantes_comprobante_id",
                        column: x => x.comprobante_id,
                        principalSchema: "public",
                        principalTable: "comprobantes",
                        principalColumn: "comprobante_id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Siembra comprobante_series desde los datos existentes — imprescindible para que el
            // primer SiguienteNumeroAsync() no reinicie la numeración en 1 y choque con el índice
            // único (serie, numero) ya existente sobre comprobantes.
            migrationBuilder.Sql(@"
                INSERT INTO public.comprobante_series (serie, tipo_comprobante, ultimo_numero)
                SELECT serie, ""TipoComprobante"", MAX(numero)
                FROM public.comprobantes
                GROUP BY serie, ""TipoComprobante""
                ON CONFLICT (serie) DO NOTHING;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "comprobante_series",
                schema: "public");

            migrationBuilder.DropTable(
                name: "comprobantes_sunat",
                schema: "public");
        }
    }
}
