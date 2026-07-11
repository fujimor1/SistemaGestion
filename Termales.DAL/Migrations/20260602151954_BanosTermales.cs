using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Termales.DAL.Migrations
{
    /// <inheritdoc />
    public partial class BanosTermales : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.EnsureSchema(
                name: "seguridad");

            migrationBuilder.CreateTable(
                name: "clientes",
                schema: "public",
                columns: table => new
                {
                    cliente_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombres = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    apellidos = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    dni = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    telefono = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true),
                    email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    direccion = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    fecha_registro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_clientes", x => x.cliente_id);
                });

            migrationBuilder.CreateTable(
                name: "empleados",
                schema: "public",
                columns: table => new
                {
                    empleado_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombres = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    apellidos = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    dni = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    cargo = table.Column<int>(type: "integer", nullable: false),
                    telefono = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true),
                    email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    password_hash = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    fecha_contrato = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_empleados", x => x.empleado_id);
                });

            migrationBuilder.CreateTable(
                name: "piscinas",
                schema: "public",
                columns: table => new
                {
                    piscina_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    descripcion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    temperatura_grados = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    capacidad_personas = table.Column<int>(type: "integer", nullable: false),
                    tarifa_por_hora = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    disponible = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_piscinas", x => x.piscina_id);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                schema: "seguridad",
                columns: table => new
                {
                    rol_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    descripcion = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.rol_id);
                });

            migrationBuilder.CreateTable(
                name: "Servicios",
                schema: "public",
                columns: table => new
                {
                    ServicioId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "text", nullable: false),
                    Descripcion = table.Column<string>(type: "text", nullable: true),
                    Precio = table.Column<decimal>(type: "numeric", nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Servicios", x => x.ServicioId);
                });

            migrationBuilder.CreateTable(
                name: "tipo_servicios",
                schema: "public",
                columns: table => new
                {
                    tipo_servicio_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    descripcion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    capacidad_maxima = table.Column<int>(type: "integer", nullable: false),
                    precio_por_persona = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tipo_servicios", x => x.tipo_servicio_id);
                });

            migrationBuilder.CreateTable(
                name: "reservas",
                schema: "public",
                columns: table => new
                {
                    reserva_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    cliente_id = table.Column<int>(type: "integer", nullable: false),
                    piscina_id = table.Column<int>(type: "integer", nullable: false),
                    fecha_reserva = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    fecha_ingreso = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    fecha_salida = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    numero_personas = table.Column<int>(type: "integer", nullable: false),
                    monto_total = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    estado = table.Column<int>(type: "integer", nullable: false),
                    observaciones = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    fecha_creacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reservas", x => x.reserva_id);
                    table.ForeignKey(
                        name: "FK_reservas_clientes_cliente_id",
                        column: x => x.cliente_id,
                        principalSchema: "public",
                        principalTable: "clientes",
                        principalColumn: "cliente_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_reservas_piscinas_piscina_id",
                        column: x => x.piscina_id,
                        principalSchema: "public",
                        principalTable: "piscinas",
                        principalColumn: "piscina_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "usuarios",
                schema: "seguridad",
                columns: table => new
                {
                    usuario_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    apellido = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    password_hash = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    rol_id = table.Column<int>(type: "integer", nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    fecha_creacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuarios", x => x.usuario_id);
                    table.ForeignKey(
                        name: "FK_usuarios_roles_rol_id",
                        column: x => x.rol_id,
                        principalSchema: "seguridad",
                        principalTable: "roles",
                        principalColumn: "rol_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "aforos",
                schema: "public",
                columns: table => new
                {
                    aforo_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tipo_servicio_id = table.Column<int>(type: "integer", nullable: false),
                    fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    capacidad_maxima = table.Column<int>(type: "integer", nullable: false),
                    ocupacion_actual = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_aforos", x => x.aforo_id);
                    table.ForeignKey(
                        name: "FK_aforos_tipo_servicios_tipo_servicio_id",
                        column: x => x.tipo_servicio_id,
                        principalSchema: "public",
                        principalTable: "tipo_servicios",
                        principalColumn: "tipo_servicio_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "pagos",
                schema: "public",
                columns: table => new
                {
                    pago_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    reserva_id = table.Column<int>(type: "integer", nullable: false),
                    monto = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    tipo_pago = table.Column<int>(type: "integer", nullable: false),
                    fecha_pago = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    numero_comprobante = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    observaciones = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pagos", x => x.pago_id);
                    table.ForeignKey(
                        name: "FK_pagos_reservas_reserva_id",
                        column: x => x.reserva_id,
                        principalSchema: "public",
                        principalTable: "reservas",
                        principalColumn: "reserva_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "reserva_servicios",
                schema: "public",
                columns: table => new
                {
                    reserva_servicio_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    reserva_id = table.Column<int>(type: "integer", nullable: false),
                    servicio_id = table.Column<int>(type: "integer", nullable: false),
                    cantidad = table.Column<int>(type: "integer", nullable: false),
                    precio_unitario = table.Column<decimal>(type: "numeric(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reserva_servicios", x => x.reserva_servicio_id);
                    table.ForeignKey(
                        name: "FK_reserva_servicios_Servicios_servicio_id",
                        column: x => x.servicio_id,
                        principalSchema: "public",
                        principalTable: "Servicios",
                        principalColumn: "ServicioId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_reserva_servicios_reservas_reserva_id",
                        column: x => x.reserva_id,
                        principalSchema: "public",
                        principalTable: "reservas",
                        principalColumn: "reserva_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "turnos",
                schema: "public",
                columns: table => new
                {
                    turno_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tipo_servicio_id = table.Column<int>(type: "integer", nullable: false),
                    fecha_hora = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    cantidad_personas = table.Column<int>(type: "integer", nullable: false),
                    monto_total = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    estado_pago = table.Column<int>(type: "integer", nullable: false),
                    metodo_pago = table.Column<int>(type: "integer", nullable: false),
                    usuario_id = table.Column<int>(type: "integer", nullable: false),
                    fecha_creacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_turnos", x => x.turno_id);
                    table.ForeignKey(
                        name: "FK_turnos_tipo_servicios_tipo_servicio_id",
                        column: x => x.tipo_servicio_id,
                        principalSchema: "public",
                        principalTable: "tipo_servicios",
                        principalColumn: "tipo_servicio_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_turnos_usuarios_usuario_id",
                        column: x => x.usuario_id,
                        principalSchema: "seguridad",
                        principalTable: "usuarios",
                        principalColumn: "usuario_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "usuario_roles",
                schema: "seguridad",
                columns: table => new
                {
                    usuario_rol_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    usuario_id = table.Column<int>(type: "integer", nullable: false),
                    rol_id = table.Column<int>(type: "integer", nullable: false),
                    fecha_asignacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuario_roles", x => x.usuario_rol_id);
                    table.ForeignKey(
                        name: "FK_usuario_roles_roles_rol_id",
                        column: x => x.rol_id,
                        principalSchema: "seguridad",
                        principalTable: "roles",
                        principalColumn: "rol_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_usuario_roles_usuarios_usuario_id",
                        column: x => x.usuario_id,
                        principalSchema: "seguridad",
                        principalTable: "usuarios",
                        principalColumn: "usuario_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "seguridad",
                table: "roles",
                columns: new[] { "rol_id", "activo", "descripcion", "nombre" },
                values: new object[,]
                {
                    { 1, true, "Acceso total al sistema", "Administrador" },
                    { 2, true, "Gestión de reservas y pagos", "Cajero" },
                    { 3, true, "Atención al cliente y reservas", "Recepcionista" }
                });

            migrationBuilder.InsertData(
                schema: "public",
                table: "tipo_servicios",
                columns: new[] { "tipo_servicio_id", "activo", "capacidad_maxima", "descripcion", "nombre", "precio_por_persona" },
                values: new object[,]
                {
                    { 1, true, 50, "Acceso a piscina termal", "Piscina", 15.00m },
                    { 2, true, 4, "Baño privado con agua termal", "Baño Privado", 25.00m }
                });

            migrationBuilder.CreateIndex(
                name: "IX_aforos_tipo_servicio_id_fecha",
                schema: "public",
                table: "aforos",
                columns: new[] { "tipo_servicio_id", "fecha" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_clientes_dni",
                schema: "public",
                table: "clientes",
                column: "dni",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_empleados_dni",
                schema: "public",
                table: "empleados",
                column: "dni",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_empleados_email",
                schema: "public",
                table: "empleados",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_pagos_reserva_id",
                schema: "public",
                table: "pagos",
                column: "reserva_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_reserva_servicios_reserva_id",
                schema: "public",
                table: "reserva_servicios",
                column: "reserva_id");

            migrationBuilder.CreateIndex(
                name: "IX_reserva_servicios_servicio_id",
                schema: "public",
                table: "reserva_servicios",
                column: "servicio_id");

            migrationBuilder.CreateIndex(
                name: "IX_reservas_cliente_id",
                schema: "public",
                table: "reservas",
                column: "cliente_id");

            migrationBuilder.CreateIndex(
                name: "IX_reservas_piscina_id",
                schema: "public",
                table: "reservas",
                column: "piscina_id");

            migrationBuilder.CreateIndex(
                name: "IX_roles_nombre",
                schema: "seguridad",
                table: "roles",
                column: "nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_turnos_tipo_servicio_id",
                schema: "public",
                table: "turnos",
                column: "tipo_servicio_id");

            migrationBuilder.CreateIndex(
                name: "IX_turnos_usuario_id",
                schema: "public",
                table: "turnos",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "IX_usuario_roles_rol_id",
                schema: "seguridad",
                table: "usuario_roles",
                column: "rol_id");

            migrationBuilder.CreateIndex(
                name: "IX_usuario_roles_usuario_id_rol_id",
                schema: "seguridad",
                table: "usuario_roles",
                columns: new[] { "usuario_id", "rol_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_email",
                schema: "seguridad",
                table: "usuarios",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_rol_id",
                schema: "seguridad",
                table: "usuarios",
                column: "rol_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "aforos",
                schema: "public");

            migrationBuilder.DropTable(
                name: "empleados",
                schema: "public");

            migrationBuilder.DropTable(
                name: "pagos",
                schema: "public");

            migrationBuilder.DropTable(
                name: "reserva_servicios",
                schema: "public");

            migrationBuilder.DropTable(
                name: "turnos",
                schema: "public");

            migrationBuilder.DropTable(
                name: "usuario_roles",
                schema: "seguridad");

            migrationBuilder.DropTable(
                name: "Servicios",
                schema: "public");

            migrationBuilder.DropTable(
                name: "reservas",
                schema: "public");

            migrationBuilder.DropTable(
                name: "tipo_servicios",
                schema: "public");

            migrationBuilder.DropTable(
                name: "usuarios",
                schema: "seguridad");

            migrationBuilder.DropTable(
                name: "clientes",
                schema: "public");

            migrationBuilder.DropTable(
                name: "piscinas",
                schema: "public");

            migrationBuilder.DropTable(
                name: "roles",
                schema: "seguridad");
        }
    }
}
