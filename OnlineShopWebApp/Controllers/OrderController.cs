using Microsoft.AspNetCore.Mvc;
using OnlineShop.Db.Interfaces;
using OnlineShop.Db.Models;
using OnlineShopWebApp.Areas.Client.Models;
using OnlineShopWebApp.Helpers;
using System.Security.Claims;

namespace OnlineShopWebApp.Controllers
{
    public class OrderController : Controller
    {
        private readonly ICartsStorages _cartsStorages;
        private readonly IOrderStorages _orderStorages;
        private readonly ILogger<OrderController> _logger;
        public OrderController(ICartsStorages cartsStorages, IOrderStorages orderStorages, ILogger<OrderController> logger)
        {
            _cartsStorages = cartsStorages;
            _orderStorages = orderStorages;
            _logger = logger;
        }
        public async Task<IActionResult> Index()
        {
            _logger.LogInformation($"Получение Id пользователя");
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogInformation($"Получение корзины пользователя с Id - {userId}");
            var cartDb = await _cartsStorages.TryGetByUserIdAsync(userId);
            try
            {
                var cartViewModel = cartDb.ToCartViewModel();
                var productCounts = cartViewModel?.Items.Count ?? 0;
                var existingOrder = new OrderViewModel
                {
                    Items = cartViewModel.Items,
                };

                return View(existingOrder);
            }
           catch(Exception ex)
            {
                _logger.LogError(ex,$"Произошла ошибка при получении корзины пользователя Id - {userId}. Oderd/Index");
                return View("Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Buy(OrderViewModel orders)
        {
            _logger.LogInformation($"Получение Id пользователя");
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogInformation($"Получение корзины пользователя с Id - {userId}");
            var cart = await _cartsStorages.TryGetByUserIdAsync(userId);
            try
            {
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
            catch(Exception ex)
            {
                _logger.LogError(ex, $"Произошла ошибка при оформлении заказа. Order/Buy");
                return View("Error");
            }
        }
        public IActionResult OrderSuccess()
        {
            return View();
        }
    }
}
