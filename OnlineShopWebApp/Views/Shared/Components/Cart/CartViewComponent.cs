using Microsoft.AspNetCore.Mvc;
using OnlineShop.Db.Interfaces;
using OnlineShopWebApp.Helpers;

namespace OnlineShopWebApp.Areas.Client.Views.Shared.Components.Cart
{
    public class CartViewComponent (ICartsStorages cartsStorages) : ViewComponent
    {
        private readonly ICartsStorages _cartStorages = cartsStorages;

        public async Task <IViewComponentResult> InvokeAsync(string userId)
        {
            var cart = await _cartStorages.TryGetByUserIdAsync(userId);

            var carViewModel = cart.ToCartViewModel();

            var productCount = carViewModel?.Quantity ?? 0;

            return View("Cart", productCount);
        }
    }
}
