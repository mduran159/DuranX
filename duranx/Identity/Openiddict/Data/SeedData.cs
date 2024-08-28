using Microsoft.AspNetCore.Identity;

namespace OpeniddictServer.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider, UserManager<IdentityUser> userManager)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            string[] roleNames = { Roles.Admin, Roles.User };
            IdentityResult roleResult;

            foreach (var roleName in roleNames)
            {
                // Verifica si el rol ya existe, si no, lo crea
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    roleResult = await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // Crear un usuario administrador predeterminado
            var adminEmail = "admin@duranx.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                var newUser = new IdentityUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                };
                string adminPassword = "Duranx159!";
                var createUser = await userManager.CreateAsync(newUser, adminPassword);
                if (createUser.Succeeded)
                {
                    await userManager.AddToRoleAsync(newUser, Roles.Admin);
                }
            }
        }
    }
}
