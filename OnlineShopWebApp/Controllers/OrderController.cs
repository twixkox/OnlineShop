using Microsoft.AspNetCore.Mvc;
using OnlineShop.Db.Interfaces;
using OnlineShop.Db.Models;
using OnlineShopWebApp.Helpers;
using OnlineShopWebApp.Models;
using System.Security.Claims;

namespace OnlineShopWebApp.Controllers
{
    public class OrderController : Controller
    {
        private readonly ICartsStorages _cartsStorages;
        private readonly IOrderStorages _orderStorages;
        public OrderController(ICartsStorages cartsStorages, IOrderStorages orderStorages)
        {
            _cartsStorages = cartsStorages;
            _orderStorages = orderStorages;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var cartDb = await _cartsStorages.TryGetByUserIdAsync(userId);

            var cartViewModel = cartDb.ToCartViewModel();

            var productCounts = cartViewModel?.Items.Count ?? 0;

            var existingOrder = new OrderViewModel
            {
                Items = cartViewModel.Items,
            };

            return View(existingOrder);
        }

        [HttpPost]
        public async Task<IActionResult> Buy(OrderViewModel orders)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            var cart = await _cartsStorages.TryGetByUserIdAsync(userId);

            var order = new Order
            {
                DeliveryUserInfo = Mapping.ToDeliveryUserInfo(orders.DeliveryUserInfo),
                Items = cart.Items,
                UserId = userId
            };

            if (!ModelState.IsValid) return View("Index", order.ToOrderViewModel());

            await _orderStorages.AddAsync(order);

            await _cartsStorages.ClearAsync(userId);
            return RedirectToAction("OrderSuccess");
        }
        public IActionResult OrderSuccess()
        {
            return View();
        }
    }
}
