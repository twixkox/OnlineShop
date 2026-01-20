using Microsoft.AspNetCore.Identity;
using OnlineShop.Db.Models;
using OnlineShopWebApi.Models;


namespace OnlineShopWebApi
{
    public class UserService : IUserService
    {
        public async Task<bool> IsValidUserForAuth (AuthUser authUser, SignInManager<User> signInManager)
        {
            var result = await signInManager.PasswordSignInAsync(authUser.UserName, authUser.Password, false, false);

            return result.Succeeded;
        }

        public async Task<bool> IsValidUserForRegister (RegUser regUser, UserManager<User> userManager)
        {
            var user = new User { UserName = regUser.UserName,FirstName = "Swagger", LastName = "User" };
            var result = await userManager.CreateAsync(user, regUser.Password);

            return result.Succeeded;
        }
    }
}
