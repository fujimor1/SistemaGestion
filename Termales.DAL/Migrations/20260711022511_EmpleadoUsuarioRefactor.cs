using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Termales.DAL.Migrations
{
    /// <inheritdoc />
    public partial class EmpleadoUsuarioRefactor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_usuarios_empleados_empleado_id",
                schema: "seguridad",
                table: "usuarios");

            migrationBuilder.DropIndex(
                name: "IX_empleados_email",
                schema: "public",
                table: "empleados");

            migrationBuilder.DropColumn(
                name: "apellido",
                schema: "seguridad",
                table: "usuarios");

            migrationBuilder.DropColumn(
                name: "nombre",
                schema: "seguridad",
                table: "usuarios");

            migrationBuilder.DropColumn(
                name: "cargo",
                schema: "public",
                table: "empleados");

            migrationBuilder.DropColumn(
                name: "email",
                schema: "public",
                table: "empleados");

            migrationBuilder.DropColumn(
                name: "fecha_contrato",
                schema: "public",
                table: "empleados");

            migrationBuilder.DropColumn(
                name: "password_hash",
                schema: "public",
                table: "empleados");

            migrationBuilder.DropColumn(
                name: "telefono",
                schema: "public",
                table: "empleados");

            migrationBuilder.EnsureSchema(
                name: "compras");

            // Backfill: cualquier usuario sembrado antes de esta migración (ej. el admin
            // inicial) pudo quedar con empleado_id nulo, ya que antes el vínculo era
            // opcional. Se le crea/asigna un Empleado placeholder "Sistema Administrador"
            // (dni '00000000') antes de forzar la columna a NOT NULL más abajo.
            migrationBuilder.Sql(@"
                INSERT INTO public.empleados (nombres, apellidos, dni, activo)
                SELECT 'Sistema', 'Administrador', '00000000', true
                WHERE NOT EXISTS (SELECT 1 FROM public.empleados WHERE dni = '00000000');

                UPDATE seguridad.usuarios
                SET empleado_id = (SELECT empleado_id FROM public.empleados WHERE dni = '00000000')
                WHERE empleado_id IS NULL;
            ");

            migrationBuilder.AlterColumn<int>(
                name: "empleado_id",
                schema: "seguridad",
                table: "usuarios",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "compra_id",
                schema: "inventario",
                table: "entradas_producto",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "compra_id",
                schema: "inventario",
                table: "entradas_insumo",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "proveedores",
                schema: "compras",
                columns: table => new
                {
                    proveedor_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ruc = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: false),
                    razon_social = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    nombre_comercial = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    direccion = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    telefono = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true),
                    email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_proveedores", x => x.proveedor_id);
                });

            migrationBuilder.CreateTable(
                name: "compras",
                schema: "compras",
                columns: table => new
                {
                    compra_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    proveedor_id = table.Column<int>(type: "integer", nullable: false),
                    tipo_comprobante = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    serie = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    numero = table.Column<int>(type: "integer", nullable: false),
                    fecha_emision = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    forma_pago = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    fecha_vencimiento = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    moneda = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "PEN"),
                    total_gravada = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    igv = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    total = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    observaciones = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true),
                    registrado_por = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    fecha_registro = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "now()"),
                    fecha_pago = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    egreso_caja_chica_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compras", x => x.compra_id);
                    table.ForeignKey(
                        name: "FK_compras_egresos_caja_chica_egreso_caja_chica_id",
                        column: x => x.egreso_caja_chica_id,
                        principalSchema: "caja",
                        principalTable: "egresos_caja_chica",
                        principalColumn: "egreso_caja_chica_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_compras_proveedores_proveedor_id",
                        column: x => x.proveedor_id,
                        principalSchema: "compras",
                        principalTable: "proveedores",
                        principalColumn: "proveedor_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "detalle_compras",
                schema: "compras",
                columns: table => new
                {
                    detalle_compra_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    compra_id = table.Column<int>(type: "integer", nullable: false),
                    tipo_item = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    insumo_id = table.Column<int>(type: "integer", nullable: true),
                    producto_id = table.Column<int>(type: "integer", nullable: true),
                    cantidad = table.Column<decimal>(type: "numeric(12,3)", precision: 12, scale: 3, nullable: false),
                    precio_unitario = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    total = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_detalle_compras", x => x.detalle_compra_id);
                    table.CheckConstraint("CK_detalle_compras_insumo_o_producto", "(insumo_id IS NOT NULL AND producto_id IS NULL) OR (insumo_id IS NULL AND producto_id IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_detalle_compras_compras_compra_id",
                        column: x => x.compra_id,
                        principalSchema: "compras",
                        principalTable: "compras",
                        principalColumn: "compra_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_detalle_compras_insumos_insumo_id",
                        column: x => x.insumo_id,
                        principalSchema: "inventario",
                        principalTable: "insumos",
                        principalColumn: "insumo_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_detalle_compras_productos_producto_id",
                        column: x => x.producto_id,
                        principalSchema: "tienda",
                        principalTable: "productos",
                        principalColumn: "producto_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_entradas_producto_compra_id",
                schema: "inventario",
                table: "entradas_producto",
                column: "compra_id");

            migrationBuilder.CreateIndex(
                name: "IX_entradas_insumo_compra_id",
                schema: "inventario",
                table: "entradas_insumo",
                column: "compra_id");

            migrationBuilder.CreateIndex(
                name: "IX_compras_egreso_caja_chica_id",
                schema: "compras",
                table: "compras",
                column: "egreso_caja_chica_id");

            migrationBuilder.CreateIndex(
                name: "IX_compras_proveedor_id_serie_numero",
                schema: "compras",
                table: "compras",
                columns: new[] { "proveedor_id", "serie", "numero" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_detalle_compras_compra_id",
                schema: "compras",
                table: "detalle_compras",
                column: "compra_id");

            migrationBuilder.CreateIndex(
                name: "IX_detalle_compras_insumo_id",
                schema: "compras",
                table: "detalle_compras",
                column: "insumo_id");

            migrationBuilder.CreateIndex(
                name: "IX_detalle_compras_producto_id",
                schema: "compras",
                table: "detalle_compras",
                column: "producto_id");

            migrationBuilder.CreateIndex(
                name: "IX_proveedores_ruc",
                schema: "compras",
                table: "proveedores",
                column: "ruc",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_entradas_insumo_compras_compra_id",
                schema: "inventario",
                table: "entradas_insumo",
                column: "compra_id",
                principalSchema: "compras",
                principalTable: "compras",
                principalColumn: "compra_id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_entradas_producto_compras_compra_id",
                schema: "inventario",
                table: "entradas_producto",
                column: "compra_id",
                principalSchema: "compras",
                principalTable: "compras",
                principalColumn: "compra_id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_usuarios_empleados_empleado_id",
                schema: "seguridad",
                table: "usuarios",
                column: "empleado_id",
                principalSchema: "public",
                principalTable: "empleados",
                principalColumn: "empleado_id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_entradas_insumo_compras_compra_id",
                schema: "inventario",
                table: "entradas_insumo");

            migrationBuilder.DropForeignKey(
                name: "FK_entradas_producto_compras_compra_id",
                schema: "inventario",
                table: "entradas_producto");

            migrationBuilder.DropForeignKey(
                name: "FK_usuarios_empleados_empleado_id",
                schema: "seguridad",
                table: "usuarios");

            migrationBuilder.DropTable(
                name: "detalle_compras",
                schema: "compras");

            migrationBuilder.DropTable(
                name: "compras",
                schema: "compras");

            migrationBuilder.DropTable(
                name: "proveedores",
                schema: "compras");

            migrationBuilder.DropIndex(
                name: "IX_entradas_producto_compra_id",
                schema: "inventario",
                table: "entradas_producto");

            migrationBuilder.DropIndex(
                name: "IX_entradas_insumo_compra_id",
                schema: "inventario",
                table: "entradas_insumo");

            migrationBuilder.DropColumn(
                name: "compra_id",
                schema: "inventario",
                table: "entradas_producto");

            migrationBuilder.DropColumn(
                name: "compra_id",
                schema: "inventario",
                table: "entradas_insumo");

            migrationBuilder.AlterColumn<int>(
                name: "empleado_id",
                schema: "seguridad",
                table: "usuarios",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<string>(
                name: "apellido",
                schema: "seguridad",
                table: "usuarios",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "nombre",
                schema: "seguridad",
                table: "usuarios",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "cargo",
                schema: "public",
                table: "empleados",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "email",
                schema: "public",
                table: "empleados",
                type: "character varying(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "fecha_contrato",
                schema: "public",
                table: "empleados",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "password_hash",
                schema: "public",
                table: "empleados",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "telefono",
                schema: "public",
                table: "empleados",
                type: "character varying(15)",
                maxLength: 15,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_empleados_email",
                schema: "public",
                table: "empleados",
                column: "email",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_usuarios_empleados_empleado_id",
                schema: "seguridad",
                table: "usuarios",
                column: "empleado_id",
                principalSchema: "public",
                principalTable: "empleados",
                principalColumn: "empleado_id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
