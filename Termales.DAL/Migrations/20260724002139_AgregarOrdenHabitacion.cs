using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Termales.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AgregarOrdenHabitacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "orden",
                schema: "public",
                table: "habitaciones",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            // Deja el orden manual inicial igual al orden alfabético que ya se
            // mostraba, para que no "salte" nada al desplegar este cambio.
            migrationBuilder.Sql(@"
                UPDATE public.habitaciones h
                SET orden = sub.rn
                FROM (
                    SELECT habitacion_id, ROW_NUMBER() OVER (ORDER BY nombre) - 1 AS rn
                    FROM public.habitaciones
                ) sub
                WHERE h.habitacion_id = sub.habitacion_id;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "orden",
                schema: "public",
                table: "habitaciones");
        }
    }
}
