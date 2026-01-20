using Microsoft.AspNetCore.Identity;
using OnlineShop.Db.Models;
using OnlineShopWebApi.Models;

namespace OnlineShopWebApi
{
    public interface IUserService
    {
        Task<bool> IsValidUserForAuth(AuthUser authUser, SignInManager<User> signInManager);

        Task<bool> IsValidUserForRegister(RegUser regUser, UserManager<User> userManager);
    }
}
