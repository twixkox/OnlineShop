using Microsoft.AspNetCore.Mvc;
using OnlineShop.Db.Interfaces;
using OnlineShopWebApp.Helpers;

namespace OnlineShopWebApp.Views.Shared.Components.Favorite
{
    public class FavoriteViewComponent(IFavoritesStorages favoriteStorages) : ViewComponent
    {
        private readonly IFavoritesStorages _favoritesStorages = favoriteStorages;

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var favorite = await _favoritesStorages.TryGetByUserIdAsync(Constants.UserId);

            var favoriteViewModel = favorite.ToFavoriteViewModel();

            var productCount = favoriteViewModel?.Items.Count()?? 0;

            return View("Favorite", productCount);
        }
    }
}
