using Microsoft.AspNetCore.Mvc;
using OnlineShop.Db.Interfaces;
using OnlineShopWebApp.Helpers;

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
            var favorites = await _favoriteStorages.TryGetByUserIdAsync(Constants.UserId);

            return View(favorites?.ToFavoriteViewModel());
        }

        public async Task<IActionResult> Add(Guid productId,string userId = "UserId")
        {
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
