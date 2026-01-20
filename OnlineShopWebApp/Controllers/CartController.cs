using Microsoft.AspNetCore.Mvc;
using OnlineShop.Db.Interfaces;
using OnlineShopWebApp.Helpers;

namespace OnlineShopWebApp.Controllers
{
    public class CartController : Controller
    {
        private readonly ICartsStorages _cartsStorage;
        private readonly IProductStorages _productStorage;

        public CartController(ICartsStorages cartsStorage, IProductStorages productStorage)
        {
            _cartsStorage = cartsStorage;
            _productStorage = productStorage;
        }

        public async Task<IActionResult> Index()
        {
            var cart = await _cartsStorage.TryGetByUserIdAsync(Constants.UserId);

            return View(cart.ToCartViewModel());
        }
        public async Task<IActionResult> Add(Guid productId)
        {
            var product = await _productStorage.TryGetProductByIdAsync(productId);

            await _cartsStorage.AddAsync(product, Constants.UserId);

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Clear(string userId)
        {
            await _cartsStorage.ClearAsync(userId);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Subtract(Guid productId)
        {
            await _cartsStorage.SubtractAsync(productId,Constants.UserId);
            return RedirectToAction("Index");
        }
    }
}
