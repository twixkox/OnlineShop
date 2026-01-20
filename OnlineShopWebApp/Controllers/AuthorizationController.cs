using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Db.Models;
using OnlineShopWebApp.Models;

namespace OnlineShopWebApp.Controllers
{
    public class AuthorizationController : Controller
    {
        private readonly SignInManager<User> _signInManager;

        public AuthorizationController(SignInManager<User> signInManager)
        {
            _signInManager = signInManager;
        }

        public IActionResult Authorization()
        {
            return View("Index");
        }
        [HttpPost]
        public async Task<IActionResult> Authorization(AuthorizationUserViewModel user)
        {
            if (user.UserName == user.Password) ModelState.AddModelError("", "Логин и пароль не должны совпадать");
                          
            var result = await _signInManager.PasswordSignInAsync(user.UserName, user.Password, user.RememberMe, false);

            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ModelState.AddModelError("", "Неверный пароль");
            }
            if (!ModelState.IsValid) return View("Index", user);

            return RedirectToAction(nameof(Index), "Home");
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}
