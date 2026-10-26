using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Db.Interfaces;
using OnlineShopWebApp.Helpers;
using System.Security.Claims;

namespace OnlineShopWebApp.Controllers
{
    [Authorize]
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

        private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        public async Task<IActionResult> Index()
        {
            try
            {
                _logger.LogDebug("Получение списка избранного пользователя");
                var favorites = await _favoriteStorages.TryGetByUserIdAsync(UserId);
                return View(favorites?.ToFavoriteViewModel());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Произошла ошибка при получении избранного. Favorite/Index");
                return View("Error");
            }
        }

        public async Task<IActionResult> Add(Guid productId)
        {
            try
            {
                var product = await _productStorages.TryGetProductByIdAsync(productId);
                if (product != null) await _favoriteStorages.AddAsync(product, UserId);

                if (Request.IsAjax())
                {
                    return Json(new { inFavorites = true, count = await CountAsync(), message = "Добавлено в избранное" });
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Произошла ошибка при добавлении товара в избранное. Id товара - {ProductId}", productId);
                return View("Error");
            }
        }

        // Добавляет товар, если его нет в избранном, и убирает, если он там уже есть (кнопка-сердечко на карточке)
        public async Task<IActionResult> Toggle(Guid productId)
        {
            try
            {
                var favorite = await _favoriteStorages.TryGetByUserIdAsync(UserId);
                var isInFavorites = favorite?.Products?.Any(p => p.Id == productId) == true;

                if (isInFavorites)
                {
                    await _favoriteStorages.DeleteAsync(productId, UserId);
                }
                else
                {
                    var product = await _productStorages.TryGetProductByIdAsync(productId);
                    if (product == null) return NotFound();
                    await _favoriteStorages.AddAsync(product, UserId);
                }

                if (Request.IsAjax())
                {
                    return Json(new
                    {
                        inFavorites = !isInFavorites,
                        count = await CountAsync(),
                        message = isInFavorites ? "Убрано из избранного" : "Добавлено в избранное"
                    });
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при изменении избранного. Id товара - {ProductId}", productId);
                return View("Error");
            }
        }

        // userId берётся из авторизации, а не из адресной строки: раньше можно было удалять чужое избранное
        public async Task<IActionResult> Delete(Guid productId)
        {
            try
            {
                await _favoriteStorages.DeleteAsync(productId, UserId);

                if (Request.IsAjax())
                {
                    return Json(new { inFavorites = false, count = await CountAsync(), message = "Убрано из избранного" });
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Произошла ошибка при удалении товара из избранного. Id товара - {ProductId}", productId);
                return View("Error");
            }
        }

        private async Task<int> CountAsync()
        {
            var favorite = await _favoriteStorages.TryGetByUserIdAsync(UserId);
            return favorite?.Products?.Count ?? 0;
        }
    }
}
