using Microsoft.AspNetCore.Mvc;
using OnlineShop.Db.Interfaces;
using OnlineShopWebApp.Helpers;

namespace OnlineShopWebApp.Views.Shared.ViewComponents.CartViewComponents
{
    public class CartViewComponent (ICartsStorages cartsStorages) : ViewComponent
    {
        private readonly ICartsStorages _cartStorages = cartsStorages;

        public async Task <IViewComponentResult> InvokeAsync()
        {
            var cart = await _cartStorages.TryGetByUserIdAsync(Constants.UserId);

            var carViewModel = cart.ToCartViewModel();

            var productCount = carViewModel?.Quantity ?? 0;

            return View("Cart", productCount);
        }
    }
}
