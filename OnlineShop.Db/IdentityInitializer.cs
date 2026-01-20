using Microsoft.AspNetCore.Identity;
using OnlineShop.Db.Models;
using OnlineShopWebApp;

namespace OnlineShop.Db
{
    public class IdentityInitializer
    {
        public static async Task InitializeAsync(UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            var adminEmail = "admin@gmail.com";
            var adminPassword = "Admin12345!";

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
                var admin = new User { Email = adminEmail, UserName = adminEmail, FirstName = "Admin",LastName = "Admin", PhoneNumber = "1123",CreationDateTime = DateTime.Now };

                var result = await userManager.CreateAsync(admin, adminPassword);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, Constants.AdminRoleName);
                }
            }
        }
    }
}
