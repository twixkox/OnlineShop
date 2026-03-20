using Microsoft.AspNetCore.Mvc;
using OnlineShop.Db.Interfaces;
using OnlineShopWebApp.Helpers;

namespace OnlineShopWebApp.Areas.Client.Views.Shared.Components.Favorite
{
    public class FavoriteViewComponent(IFavoritesStorages favoriteStorages) : ViewComponent
    {
        private readonly IFavoritesStorages _favoritesStorages = favoriteStorages;

        public async Task<IViewComponentResult> InvokeAsync(string userId)
        {
            var favorite = await _favoritesStorages.TryGetByUserIdAsync(userId);

            var favoriteViewModel = favorite.ToFavoriteViewModel();

            var productCount = favoriteViewModel?.Items.Count()?? 0;

            return View("Favorite", productCount);
        }
    }
}
