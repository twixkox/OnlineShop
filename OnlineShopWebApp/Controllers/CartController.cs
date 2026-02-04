using Microsoft.AspNetCore.Mvc;
using OnlineShop.Db.Interfaces;
using OnlineShop.Db.Models;
using OnlineShopWebApp.Helpers;
using System.Security.Claims;

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
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var cart = await _cartsStorage.TryGetByUserIdAsync(userId);

            return View(cart.ToCartViewModel());

        }
        public async Task<IActionResult> Add(Guid productId, int quantity = 1)
        {

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var product = await _productStorage.TryGetProductByIdAsync(productId);

            await _cartsStorage.AddAsync(product, userId);

            return RedirectToAction("Index");

        }


        public async Task<IActionResult> Clear(string userId)
        {
            await _cartsStorage.ClearAsync(userId);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Subtract(Guid productId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            await _cartsStorage.SubtractAsync(productId, userId);
            return RedirectToAction("Index");
        }
    }
}
