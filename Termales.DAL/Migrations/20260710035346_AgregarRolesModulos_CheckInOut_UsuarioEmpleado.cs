using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Termales.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AgregarRolesModulos_CheckInOut_UsuarioEmpleado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "empleado_id",
                schema: "seguridad",
                table: "usuarios",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "fecha_check_in",
                schema: "public",
                table: "habitaciones",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "fecha_check_out",
                schema: "public",
                table: "habitaciones",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_empleado_id",
                schema: "seguridad",
                table: "usuarios",
                column: "empleado_id",
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

            // Redefinición de roles como módulos del negocio:
            //  - Supervisor: acceso total (todos los módulos).
            //  - Administrador: Baños Termales, Habitaciones, Tienda, Caja,
            //    Inventario, y Comedor (solo mesas/categorías/menú).
            //  - Recepcionista: Baños Termales y Habitaciones (check-in/check-out).
            //  - Mozo (antes "Cajero", que no existía como rol real de negocio):
            //    solo la operación de comedor vía la app móvil.
            migrationBuilder.Sql(@"
                UPDATE seguridad.roles SET descripcion = 'Baños Termales, Habitaciones, Tienda, Caja, Inventario y Comedor (mesas/categorías/menú)' WHERE rol_id = 1;
                UPDATE seguridad.roles SET nombre = 'Mozo', descripcion = 'Operación de comedor desde la app móvil (mesas, órdenes)' WHERE rol_id = 2;
                UPDATE seguridad.roles SET descripcion = 'Baños Termales y Habitaciones (check-in/check-out)' WHERE rol_id = 3;
                UPDATE seguridad.roles SET descripcion = 'Acceso total al sistema' WHERE rol_id = 4;
            ");

            // La(s) cuenta(s) que tenían el antiguo rol 'Administrador' (acceso total)
            // pasan a 'Supervisor', que es ahora el único rol con todos los módulos.
            migrationBuilder.Sql(@"UPDATE seguridad.usuarios SET rol_id = 4 WHERE rol_id = 1;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE seguridad.roles SET descripcion = 'Acceso total al sistema' WHERE rol_id = 1;
                UPDATE seguridad.roles SET nombre = 'Cajero', descripcion = 'Gestión de pagos y reservas' WHERE rol_id = 2;
                UPDATE seguridad.roles SET descripcion = 'Atención al cliente' WHERE rol_id = 3;
                UPDATE seguridad.roles SET descripcion = 'Supervisión y autorización de operaciones' WHERE rol_id = 4;
            ");

            migrationBuilder.DropForeignKey(
                name: "FK_usuarios_empleados_empleado_id",
                schema: "seguridad",
                table: "usuarios");

            migrationBuilder.DropIndex(
                name: "IX_usuarios_empleado_id",
                schema: "seguridad",
                table: "usuarios");

            migrationBuilder.DropColumn(
                name: "empleado_id",
                schema: "seguridad",
                table: "usuarios");

            migrationBuilder.DropColumn(
                name: "fecha_check_in",
                schema: "public",
                table: "habitaciones");

            migrationBuilder.DropColumn(
                name: "fecha_check_out",
                schema: "public",
                table: "habitaciones");
        }
    }
}
