using Microsoft.AspNetCore.Mvc;
using OnlineShop.Db.Interfaces;
using OnlineShopWebApp.Helpers;
using System.Security.Claims;

namespace OnlineShopWebApp.Controllers
{
    public class FavoriteController : Controller
    {
        private readonly IProductStorages _productStorages;
        private readonly IFavoritesStorages _favoriteStorages;
        private readonly ILogger<FavoriteController> _logger;

        public FavoriteController(IProductStorages productStorages, IFavoritesStorages favoritesStorages, ILogger<FavoriteController> logger)
        {
            _productStorages = productStorages;
            _favoriteStorages = favoritesStorages;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            _logger.LogInformation($"Получение пользователя");
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogInformation($"Получение списка избранного пользователя Id - {userId}");
            var favorites = await _favoriteStorages.TryGetByUserIdAsync(userId);
            try
            {
                return View(favorites?.ToFavoriteViewModel());
            }
            catch(Exception ex)
            {
                _logger.LogError($"Произошла ошибка при получении избранного. Favorite/Index");
                return View("Error");
            }
        }

        public async Task<IActionResult> Add(Guid productId)
        {
            _logger.LogInformation($"Получение id пользователя");
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                TempData["InfoMessage"] = "Для продолжения необходимо авторизоваться";
                return RedirectToAction("Authorization", "Authorization");
            }
            _logger.LogInformation($"Получение товара с Id - {productId}");
            var product = await _productStorages.TryGetProductByIdAsync(productId);
            try
            {
                if (product != null) await _favoriteStorages.AddAsync(product, userId);

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,$"Произошла ошибка при добавлении товара в избранное. Favorite/Add" +
                    $"Id пользователя - {userId}" +
                    $"Id товара - {productId}");
                return View("Error");
            }
        }
        public async Task<IActionResult> Delete(Guid productId, string userId)
        {
            _logger.LogError($"Удаление товара с Id - {productId}." +
                $"Id пользователя - {userId}");
            await _favoriteStorages.DeleteAsync(productId, userId);
            try
            {

                return RedirectToAction(nameof(Index));
            }
            catch(Exception ex)
            {
                _logger.LogError(ex,$"Произошла ошибка при удалении товара из избранонго" +
                    $"Id пользователя - {userId}" +
                    $"Id товара - {productId}");
                return View("Error");
            }
        }
    }
}
