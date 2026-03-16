using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using OnlineShop.Db.Interfaces;
using OnlineShop.Db.Models;
using OnlineShopWebApp.Helpers;
using OnlineShopWebApp.Models;
using System.Security.Claims;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace OnlineShopWebApp.Controllers
{
    public class ProfileController : Controller
    {
        private readonly ILogger<ProfileController> _logger;
        private readonly IOrderStorages _orderStorages;
        private readonly UserManager<User> _user;

        public ProfileController(ILogger<ProfileController> logger, IOrderStorages orderStorages, UserManager<User> user)
        {
            _logger = logger;
            _orderStorages = orderStorages;
            _user = user;
        }
        [HttpGet]
        public async Task<IActionResult> ProfileEdit()
        {
            _logger.LogInformation($"Получение id пользователя");
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogInformation($"Поиск пользователя с Id - {userId}");
            var currentUser = await _user.FindByIdAsync(userId);
            try
            {
                return View("ProfileEdit", currentUser.ToUserViewModel());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Произошла ошибка при редактировании профиля. Profile/ProfileEdit");
                return View("Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> ProfileEdit(UserViewModel userViewModel)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning($"Передана невалидная модель. Profile/EditPassword");
                return View(userViewModel);
            }

            _logger.LogInformation($"Поиск пользователя с Id - {userViewModel.Id}");
            var user = await _user.FindByIdAsync(userViewModel.Id);
            try
            {
                user.PhoneNumber = userViewModel.Phone;
                user.FirstName = userViewModel.FirstName;
                user.LastName = userViewModel.LastName;

                await _user.UpdateAsync(user);

                return View("ProfileEdit", user.ToUserViewModel());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Произошла ошибка при редактировании профиля. Profile/ProfileEdit");
                return View("Error");
            }
        }

        public async Task<IActionResult> UserOrder()
        {
            _logger.LogInformation($"Получение id пользователя");
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogInformation($"Получение заказов пользователя");
            var orders = await _orderStorages.GetAllOrdersCurrentUser(userId);
            try
            {
                var ordersViewModel = orders.ToOrdersViewModels();
                return View(ordersViewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Произошла ошибка при получении заказов пользователя Id - {userId}. Profile/UserOrder");
                return View("Error");
            }
        }

        public async Task<IActionResult> DetailOrder(Guid orderId)
        {
            _logger.LogInformation($"Получение заказов пользователя");
            var order = await _orderStorages.TryGetByIdAsync(orderId);

            try
            {
                return View(order.ToOrderViewModel());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Произошла ошибка при получении заказов пользователя Id заказа - {orderId} . Profile/DetailOrder");
                return View("Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditPassword()
        {
            return View(nameof(EditPassword));
        }

        [HttpPost]
        public async Task<IActionResult> EditPassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning($"Передана невалидная модель. Profile/EditPassword");
                return View(model);
            }
            _logger.LogInformation($"Получение id пользователя");
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogInformation($"Поиск пользователя с Id - {userId}");
            var currentUser = await _user.FindByIdAsync(userId);
            try
            {
                var result = await _user.ChangePasswordAsync(currentUser, model.CurrentPassword, model.NewPassword);

                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = "Ваш пароль успешно изменен";
                    return RedirectToAction("Index", "Home");
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Произошла ошибка при смене пароля у пользователя Id - {userId}. Profile/EditPassword");
                return View("Error");
            }
        }
    }
}
