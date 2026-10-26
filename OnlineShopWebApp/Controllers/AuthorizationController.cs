using Microsoft.AspNetCore.Identity;
using OnlineShopWebApp.Logging;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Db.Models;
using OnlineShop.Db.Interfaces;
using OnlineShopWebApp.Areas.Client.Models;
using OnlineShopWebApp.Services;

namespace OnlineShopWebApp.Controllers
{
    public class AuthorizationController : Controller
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly ICartsStorages _carts;
        private readonly ICartOwnerProvider _cartOwner;
        private readonly OrderNotificationService _notifications;
        private readonly ILogger<AuthorizationController> _logger;

        public AuthorizationController(SignInManager<User> signInManager, UserManager<User> userManager, ICartsStorages carts,
            ICartOwnerProvider cartOwner, OrderNotificationService notifications, ILogger<AuthorizationController> logger)
        {
            _logger = logger;
            _signInManager = signInManager;
            _userManager = userManager;
            _carts = carts;
            _cartOwner = cartOwner;
            _notifications = notifications;
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
                _logger.LogDebug("Проверка авторизации пользователя {UserName}", LogMask.Email(user.UserName));
                var result = await _signInManager.PasswordSignInAsync(user.UserName.Trim(), user.Password, user.RememberMe, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    _logger.LogInformation("Вход выполнен: {UserName}", LogMask.Email(user.UserName));
                    await MergeGuestCartAsync(user.UserName.Trim());
                    TempData["SuccessMessage"] = "Рады видеть вас снова!";
                    return RedirectToLocal(returnUrl);
                }

                // Предупреждение — по таким записям видно подбор паролей
                _logger.LogWarning("Неудачная попытка входа: {UserName}", LogMask.Email(user.UserName));
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

        // ---------- Восстановление пароля ----------

        [HttpGet]
        public IActionResult ForgotPassword() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email.Trim()) ?? await _userManager.FindByNameAsync(model.Email.Trim());
            if (user?.Email != null)
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var url = Url.Action(nameof(ResetPassword), "Authorization", new { email = user.Email, token }, Request.Scheme)!;
                await _notifications.PasswordResetAsync(user.Email, url);
                _logger.LogInformation("Запрошено восстановление пароля для {Email}", LogMask.Email(user.Email));
            }

            // Одинаковый ответ, есть пользователь или нет: так нельзя узнать, зарегистрирован ли email
            return View("ForgotPasswordSent", model);
        }

        [HttpGet]
        public IActionResult ResetPassword(string? email, string? token)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
            {
                TempData["ErrorMessage"] = "Ссылка для восстановления пароля недействительна";
                return RedirectToAction(nameof(ForgotPassword));
            }

            return View(new ResetPasswordViewModel { Email = email, Token = token });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null)
            {
                var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);
                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Code == "InvalidToken"
                            ? "Ссылка устарела или уже использована. Запросите восстановление ещё раз."
                            : error.Description);
                    }
                    return View(model);
                }
            }

            TempData["SuccessMessage"] = "Пароль изменён — теперь можно войти";
            return RedirectToAction(nameof(Index));
        }

        private async Task MergeGuestCartAsync(string userName)
        {
            var guestId = _cartOwner.GetGuestOwnerId();
            if (guestId == null) return;

            var user = await _userManager.FindByNameAsync(userName);
            if (user == null) return;

            await _carts.MergeAsync(guestId, user.Id);
            _cartOwner.ForgetGuest();
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
