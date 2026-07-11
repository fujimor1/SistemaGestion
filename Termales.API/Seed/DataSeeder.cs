using Microsoft.EntityFrameworkCore;
using Termales.DAL.Context;
using Termales.Entities.Models;
using Termales.Entities.Models.Seguridad;

namespace Termales.API.Seed;

public static class DataSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TermalesDbContext>();

        try
        {
            await SeedRolesAsync(context);
            await SeedAdminAsync(context);
            Console.WriteLine("[DataSeeder] Seed completado correctamente.");
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[DataSeeder] ERROR: {ex.Message}");
            Console.WriteLine(ex.InnerException?.Message);
            Console.ResetColor();
        }
    }

    private static async Task SeedRolesAsync(TermalesDbContext context)
    {
        var rolesIniciales = new[]
        {
            new Rol { RolId = 1, Nombre = "Administrador", Descripcion = "Baños Termales, Habitaciones, Tienda, Caja, Inventario y Comedor (mesas/categorías/menú)" },
            new Rol { RolId = 2, Nombre = "Mozo",          Descripcion = "Operación de comedor desde la app móvil (mesas, órdenes)" },
            new Rol { RolId = 3, Nombre = "Recepcionista", Descripcion = "Baños Termales y Habitaciones (check-in/check-out)" },
            new Rol { RolId = 4, Nombre = "Supervisor",    Descripcion = "Acceso total al sistema" },
        };

        foreach (var rol in rolesIniciales)
        {
            if (!await context.Roles.AnyAsync(r => r.RolId == rol.RolId))
                context.Roles.Add(rol);
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedAdminAsync(TermalesDbContext context)
    {
        if (await context.Usuarios.AnyAsync())
            return;

        var empleadoSistema = await context.Empleados.FirstOrDefaultAsync(e => e.Dni == "00000000");
        if (empleadoSistema is null)
        {
            empleadoSistema = new Empleado { Nombres = "Sistema", Apellidos = "Administrador", Dni = "00000000", Activo = true };
            context.Empleados.Add(empleadoSistema);
            await context.SaveChangesAsync();
        }

        var hash = BCrypt.Net.BCrypt.HashPassword("Admin123!");

        // SQL raw para evitar problemas de mapeo EF con relaciones opcionales
        await context.Database.ExecuteSqlRawAsync(@"
            INSERT INTO seguridad.usuarios (email, password_hash, rol_id, activo, fecha_creacion, empleado_id)
            VALUES ({0}, {1}, {2}, {3}, {4}, {5})",
            "admin@collpa.pe", hash, 4, true, DateTime.UtcNow, empleadoSistema.EmpleadoId);

        Console.WriteLine("[DataSeeder] Usuario admin creado: admin@collpa.pe / Admin123!");
    }
}
