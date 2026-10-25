using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Db.Interfaces;
using OnlineShop.Db.Models;
using OnlineShopWebApp.Areas.Admin.Intarfaces;
using OnlineShopWebApp.Areas.Client.Models;
using OnlineShopWebApp.Helpers;
using OnlineShopWebApp.Models;
using System.Security.Claims;

namespace OnlineShopWebApp.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly ILogger<ProfileController> _logger;
        private readonly IOrderStorages _orderStorages;
        private readonly UserManager<User> _user;
        private readonly SignInManager<User> _signInManager;
        private readonly IFileStorageService _files;
        private readonly OnlineShopWebApp.Services.IPaymentService _payments;

        public ProfileController(ILogger<ProfileController> logger, IOrderStorages orderStorages, UserManager<User> user, SignInManager<User> signInManager,
            IFileStorageService files, OnlineShopWebApp.Services.IPaymentService payments)
        {
            _files = files;
            _payments = payments;
            _logger = logger;
            _orderStorages = orderStorages;
            _user = user;
            _signInManager = signInManager;
        }

        private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        // Ссылка «Отмена» на странице профиля вела на несуществующий Profile/Index
        public IActionResult Index() => RedirectToAction(nameof(ProfileEdit));

        [HttpGet]
        public async Task<IActionResult> ProfileEdit()
        {
            try
            {
                var currentUser = await _user.FindByIdAsync(UserId);
                return View("ProfileEdit", currentUser?.ToEditUserInfoViewModel());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Произошла ошибка при открытии профиля. Profile/ProfileEdit");
                return View("Error");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProfileEdit(EditUserInfoViewModel userViewModel)
        {
            // Логин на форме только для чтения, поэтому не проверяем его
            // (раньше email длиннее 30 символов не давал сохранить профиль)
            ModelState.Remove(nameof(EditUserInfoViewModel.UserName));
            ModelState.Remove(nameof(EditUserInfoViewModel.Id));

            // Id берём из авторизации, а не из скрытого поля формы: иначе можно было изменить чужой профиль
            var user = await _user.FindByIdAsync(UserId);
            if (user == null) return RedirectToAction("Index", "Authorization");

            userViewModel.Id = user.Id;
            userViewModel.UserName = user.UserName!;

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Передана невалидная модель. Profile/ProfileEdit");
                return View(userViewModel);
            }

            try
            {
                user.PhoneNumber = userViewModel.Phone;
                user.FirstName = userViewModel.FirstName;
                user.LastName = userViewModel.LastName;

                await _user.UpdateAsync(user);

                TempData["SuccessMessage"] = "Данные профиля сохранены";
                return RedirectToAction(nameof(ProfileEdit));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Произошла ошибка при редактировании профиля. Profile/ProfileEdit");
                return View("Error");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Avatar(IFormFile? avatar, bool remove = false)
        {
            var user = await _user.FindByIdAsync(UserId);
            if (user == null) return RedirectToAction("Index", "Authorization");

            try
            {
                if (remove)
                {
                    user.ProfileImage = _files.GetUserPhotoPath();
                    TempData["InfoMessage"] = "Фото профиля удалено";
                }
                else if (avatar != null)
                {
                    user.ProfileImage = await _files.SaveAvatarAsync(avatar, user.Id);
                    TempData["SuccessMessage"] = "Фото профиля обновлено";
                }

                await _user.UpdateAsync(user);
            }
            catch (ArgumentException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Не удалось сохранить аватар");
                TempData["ErrorMessage"] = "Не удалось загрузить изображение — попробуйте другой файл";
            }

            return RedirectToAction(nameof(ProfileEdit));
        }

        public async Task<IActionResult> UserOrder()
        {
            try
            {
                var orders = await _orderStorages.GetAllOrdersCurrentUser(UserId);
                var ordersViewModel = orders
                    .OrderByDescending(o => o.CreationDateOrder)
                    .ToList()
                    .ToOrdersViewModels();
                return View(ordersViewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Произошла ошибка при получении заказов пользователя. Profile/UserOrder");
                return View("Error");
            }
        }

        public async Task<IActionResult> DetailOrder(Guid orderId)
        {
            try
            {
                var order = await _orderStorages.TryGetByIdAsync(orderId);

                // Пользователь может смотреть только свои заказы
                if (order != null && order.UserId != UserId) order = null;

                // Если покупатель вернулся без перехода с ЮKassa — обновим статус оплаты
                if (order is { PaymentStatus: PaymentStatus.Pending } && !string.IsNullOrEmpty(order.PaymentId))
                {
                    var status = await _payments.GetPaymentStatusAsync(order.PaymentId);
                    if (status == "succeeded") { order.PaymentStatus = PaymentStatus.Paid; await _orderStorages.UpdatePaymentAsync(order.Id, PaymentStatus.Paid); }
                    if (status == "canceled") { order.PaymentStatus = PaymentStatus.Failed; await _orderStorages.UpdatePaymentAsync(order.Id, PaymentStatus.Failed); }
                }

                ViewBag.CanPay = _payments.IsEnabled;

                return View(order?.ToOrderViewModel());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Произошла ошибка при получении заказа Id - {OrderId}. Profile/DetailOrder", orderId);
                return View("Error");
            }
        }

        [HttpGet]
        public IActionResult EditPassword()
        {
            return View(nameof(EditPassword));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Передана невалидная модель. Profile/EditPassword");
                return View(model);
            }

            try
            {
                var currentUser = await _user.FindByIdAsync(UserId);
                if (currentUser == null) return RedirectToAction("Index", "Authorization");

                var result = await _user.ChangePasswordAsync(currentUser, model.CurrentPassword, model.NewPassword);

                if (result.Succeeded)
                {
                    await _signInManager.RefreshSignInAsync(currentUser);
                    TempData["SuccessMessage"] = "Ваш пароль успешно изменен";
                    return RedirectToAction("Index", "Home");
                }

                // Раньше ошибки Identity (например, неверный текущий пароль) не показывались пользователю
                foreach (var error in result.Errors)
                {
                    var message = error.Code == "PasswordMismatch" ? "Текущий пароль указан неверно" : error.Description;
                    ModelState.AddModelError(string.Empty, message);
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Произошла ошибка при смене пароля. Profile/EditPassword");
                return View("Error");
            }
        }
    }
}
