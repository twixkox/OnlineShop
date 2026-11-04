using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using OnlineShop.Db.Models;
using OnlineShopWebApp;

namespace OnlineShop.Db
{
    public class IdentityInitializer
    {
        public static async Task InitializeAsync(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration, bool isDevelopment)
        {
            var adminEmail = "admin@gmail.com";

            string[] roles = { "Admin", "Manager", "User" };

            foreach (var role in roles)
            {
                var existingRoles = await roleManager.RoleExistsAsync(role);

                if (!existingRoles)
                {
                    await roleManager.CreateAsync(new IdentityRole (role));
                }
            }

            if (await roleManager.FindByNameAsync(Constants.AdminRoleName) == null)
            {
                await roleManager.CreateAsync(new IdentityRole(Constants.AdminRoleName));
            }

            if (await roleManager.FindByNameAsync(Constants.UserRoleName) == null)
            {
                await roleManager.CreateAsync(new IdentityRole(Constants.UserRoleName));
            }

            if (await userManager.FindByNameAsync(adminEmail) == null)
            {
                var adminPassword = configuration["Admin:Password"];
                if (string.IsNullOrWhiteSpace(adminPassword))
                {
                    if (!isDevelopment)
                        throw new InvalidOperationException("Для первого запуска задайте Admin:Password (не короче 12 символов).");
                    adminPassword = "Admin12345!";
                }
                if (!isDevelopment && adminPassword.Length < 12)
                    throw new InvalidOperationException("Пароль первого администратора должен содержать не менее 12 символов.");

                var admin = new User { Email = adminEmail, UserName = adminEmail, FirstName = "Admin",LastName = "Admin", PhoneNumber = "1123",CreationDateTime = DateTime.Now, ProfileImage = "" };

                var result = await userManager.CreateAsync(admin, adminPassword);
                if (!result.Succeeded)
                    throw new InvalidOperationException($"Не удалось создать администратора: {string.Join("; ", result.Errors.Select(e => e.Description))}");
                await userManager.AddToRoleAsync(admin, Constants.AdminRoleName);
            }
        }
    }
}
