using Estacionamiento_grupo_9.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Estacionamiento_grupo_9.Data
{
    public static class SeedData
    {
        // Aplica las migraciones pendientes, crea los roles y un admin inicial.
        public static async Task InicializarAsync(IServiceProvider services)
        {
            var db = services.GetRequiredService<AppDbContext>();
            await db.Database.MigrateAsync();

            var roleManager = services.GetRequiredService<RoleManager<IdentityRole<int>>>();
            foreach (var rol in Rol.Todos)
            {
                if (!await roleManager.RoleExistsAsync(rol))
                {
                    await roleManager.CreateAsync(new IdentityRole<int>(rol));
                }
            }

            var userManager = services.GetRequiredService<UserManager<Staff>>();
            var config = services.GetRequiredService<IConfiguration>();

            var userName = config["SeedAdmin:UserName"] ?? "admin";
            var email = config["SeedAdmin:Email"] ?? "admin@estacionamiento.local";
            var password = config["SeedAdmin:Password"] ?? "Admin123";

            if (await userManager.FindByNameAsync(userName) == null)
            {
                var admin = new Staff
                {
                    UserName = userName,
                    Email = email,
                    EmailConfirmed = true
                };

                var resultado = await userManager.CreateAsync(admin, password);
                if (!resultado.Succeeded)
                {
                    var errores = string.Join("; ", resultado.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"No se pudo crear el admin inicial: {errores}");
                }

                await userManager.AddToRoleAsync(admin, Rol.Admin);
            }
        }
    }
}
