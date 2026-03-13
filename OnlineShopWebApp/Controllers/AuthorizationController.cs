using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Db.Models;
using OnlineShopWebApp.Models;

namespace OnlineShopWebApp.Controllers
{
    public class AuthorizationController : Controller
    {
        private readonly SignInManager<User> _signInManager;
        private readonly ILogger<AuthorizationController> _logger;

        public AuthorizationController(SignInManager<User> signInManager, ILogger<AuthorizationController> logger)
        {
            _logger = logger;
            _signInManager = signInManager;
        }

        public IActionResult Authorization()
        {
            return View("Index");
        }
        [HttpPost]
        public async Task<IActionResult> Authorization(AuthorizationUserViewModel user)
        {
            try
            {
                _logger.LogInformation($"Проверка авторизации пользователя {user.UserName}");
                var result = await _signInManager.PasswordSignInAsync(user.UserName, user.Password, user.RememberMe, false);

                if (result.Succeeded)
                {
                    _logger.LogInformation($"Авторизация успешна");
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    _logger.LogInformation($"Ошибка авторизаци по логину/паролю");
                    ModelState.AddModelError("", "Неверный пароль");
                }
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning($"Передана невалидная модель. Authorization/Authorization");
                    return View("Index", user);
                }
                return RedirectToAction(nameof(Index), "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Произошла ошибка авторизации пользователя. Authorization/Authorization");
                return View("Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}
