using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Db.Models;
using OnlineShopWebApp.Areas.Client.Models;

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

        // Страница входа. Сюда же ведёт LoginPath из настроек cookie (/Authorization/Index)
        [HttpGet]
        public IActionResult Index(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToLocal(returnUrl);
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View("Index");
        }

        [HttpGet]
        public IActionResult Authorization(string? returnUrl = null)
        {
            return Index(returnUrl);
        }

        [HttpPost]
        public async Task<IActionResult> Authorization(AuthorizationUserViewModel user, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Передана невалидная модель. Authorization/Authorization");
                return View("Index", user);
            }

            try
            {
                _logger.LogInformation("Проверка авторизации пользователя {UserName}", user.UserName);
                var result = await _signInManager.PasswordSignInAsync(user.UserName.Trim(), user.Password, user.RememberMe, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    _logger.LogInformation("Авторизация успешна");
                    TempData["SuccessMessage"] = "Рады видеть вас снова!";
                    return RedirectToLocal(returnUrl);
                }

                _logger.LogInformation("Ошибка авторизации по логину/паролю");
                ModelState.AddModelError(string.Empty, "Неверный email или пароль");
                return View("Index", user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Произошла ошибка авторизации пользователя. Authorization/Authorization");
                return View("Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            TempData["InfoMessage"] = "Вы вышли из аккаунта";
            return RedirectToAction("Index", "Home");
        }

        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }
    }
}
