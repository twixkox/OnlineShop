using Microsoft.AspNetCore.Mvc;
using OnlineShop.Db.Interfaces;
using OnlineShop.Db.Models;
using OnlineShopWebApp.Helpers;
using OnlineShopWebApp.Models;

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
            var cartDb = await _cartsStorages.TryGetByUserIdAsync(Constants.UserId);

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
            var cart = await _cartsStorages.TryGetByUserIdAsync(Constants.UserId);

            var order = new Order
            {
                DeliveryUserInfo = Mapping.ToDeliveryUserInfo(orders.DeliveryUserInfo),
                Items = cart.Items,
            };

            if (!ModelState.IsValid) return View("Index", order.ToOrderViewModel());

            await _orderStorages.AddAsync(order);

            await _cartsStorages.ClearAsync(Constants.UserId);
            return RedirectToAction("OrderSuccess");
        }
        public IActionResult OrderSuccess()
        {
            return View();
        }
    }
}
