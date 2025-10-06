using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WorkAcademy.Data;

namespace WorkAcademy
{
    public static class DbInitializer
    {
        public static void Initialize(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            // Garante que o banco está criado
            context.Database.Migrate();

            // Cria perfil de admin se não existir
            const string adminEmail = "admin@workacademy.com";
            const string adminSenha = "Admin123!";

            if (userManager.FindByEmailAsync(adminEmail).Result == null)
            {
                var user = new IdentityUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var result = userManager.CreateAsync(user, adminSenha).Result;

                if (result.Succeeded)
                {
                    // Criar role "Admin" se não existir
                    if (!roleManager.RoleExistsAsync("Admin").Result)
                    {
                        roleManager.CreateAsync(new IdentityRole("Admin")).Wait();
                    }

                    userManager.AddToRoleAsync(user, "Admin").Wait();
                }
            }
        }
    }
}
