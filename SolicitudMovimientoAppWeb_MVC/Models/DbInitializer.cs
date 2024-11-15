using Microsoft.AspNetCore.Identity;

namespace SolicitudMovimientoAppWeb_MVC.Models
{
    public static class DbInitializer
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            // Seed Roles
            string[] roleNames = { "Generador", "Delegador", "Chofer" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole<int>(roleName));
                }
            }

            // Seed Estados
            if (!context.Estados.Any())
            {
                context.Estados.AddRange(
                    new Estado { Descripcion = "Pendiente" },
                    new Estado { Descripcion = "Atendido" },
                    new Estado { Descripcion = "Realizado" }
                );
            }

            // Seed Tipos de Embalaje
            if (!context.TiposDeEmbalaje.Any())
            {
                context.TiposDeEmbalaje.AddRange(
                    new TipoDeEmbalaje { Descripcion = "Bulto" },
                    new TipoDeEmbalaje { Descripcion = "Tarima" },
                    new TipoDeEmbalaje { Descripcion = "Capacho" },
                    new TipoDeEmbalaje { Descripcion = "Suelto" }
                );
            }

            // Guardar los cambios en la base de datos
            await context.SaveChangesAsync();
        }
    }
}

