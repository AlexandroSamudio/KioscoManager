using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace API.Data.SeedData;

public class DBSeeder
{
    public static async Task SeedAllAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DataContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<AppRole>>();
        var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();

        var categoriaLogger = loggerFactory.CreateLogger("SeedCategorias");
        var productoLogger = loggerFactory.CreateLogger("SeedProductos");
        var userAndRoleLogger = loggerFactory.CreateLogger("SeedUsersAndRoles");
        var kioscoLogger = loggerFactory.CreateLogger("SeedKioscos");

        await context.Database.MigrateAsync();

        await SeedUsersAndRoles.SeedRolesAsync(roleManager, userAndRoleLogger);
        await SeedUsersAndRoles.SeedUsersAsync(userManager, roleManager, userAndRoleLogger);

        await JsonDataSeeder.SeedAsync<Categoria>(context, "categorias.json", categoriaLogger);
        await JsonDataSeeder.SeedAsync<Kiosco>(context, "kioscos.json", kioscoLogger);
        await context.Database.ExecuteSqlRawAsync("SELECT setval(pg_get_serial_sequence('public.\"Kioscos\"', 'Id'), COALESCE((SELECT MAX(\"Id\") FROM public.\"Kioscos\"), 0) + 1, false);");
        await JsonDataSeeder.SeedAsync<Producto>(context, "productos.json", productoLogger);
    }
}
