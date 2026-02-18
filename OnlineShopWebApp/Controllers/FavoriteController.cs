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

        public FavoriteController(IProductStorages productStorages, IFavoritesStorages favoritesStorages)
        {
            _productStorages = productStorages;
            _favoriteStorages = favoritesStorages;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var favorites = await _favoriteStorages.TryGetByUserIdAsync(userId);

            return View(favorites?.ToFavoriteViewModel());
        }

        public async Task<IActionResult> Add(Guid productId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return RedirectToAction("Authorization", "Authorization");
            }

            var product = await _productStorages.TryGetProductByIdAsync(productId);

            if (product != null) await _favoriteStorages.AddAsync(product, userId);
            
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(Guid productId, string userId)
        {
            await _favoriteStorages.DeleteAsync(productId, userId);

            return RedirectToAction(nameof(Index));
        }
    }
}
