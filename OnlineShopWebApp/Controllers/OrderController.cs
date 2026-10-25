using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Db.Interfaces;
using OnlineShop.Db.Models;
using OnlineShopWebApp.Areas.Client.Models;
using OnlineShopWebApp.Helpers;
using System.Security.Claims;

namespace OnlineShopWebApp.Controllers
{
    [Authorize]
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

        private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        public async Task<IActionResult> Index()
        {
            try
            {
                _logger.LogInformation("Получение корзины пользователя для оформления заказа");
                var cartViewModel = (await _cartsStorages.TryGetByUserIdAsync(UserId)).ToCartViewModel();

                // Раньше пустая корзина приводила к NullReferenceException и странице ошибки
                if (cartViewModel == null || cartViewModel.Items.Count == 0)
                {
                    TempData["InfoMessage"] = "Корзина пуста — добавьте товары, чтобы оформить заказ";
                    return RedirectToAction("Index", "Cart");
                }

                var order = new OrderViewModel
                {
                    Items = cartViewModel.Items,
                };

                return View(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Произошла ошибка при получении корзины пользователя. Order/Index");
                return View("Error");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Buy(OrderViewModel orders)
        {
            try
            {
                var cart = await _cartsStorages.TryGetByUserIdAsync(UserId);

                if (cart == null || cart.Items.Count == 0)
                {
                    TempData["InfoMessage"] = "Корзина пуста — добавьте товары, чтобы оформить заказ";
                    return RedirectToAction("Index", "Cart");
                }

                if (!ModelState.IsValid)
                {
                    orders.Items = cart.ToCartViewModel().Items;
                    return View("Index", orders);
                }

                var order = new Order
                {
                    DeliveryUserInfo = orders.DeliveryUserInfo.ToDeliveryUserInfo(),
                    Items = cart.Items,
                    UserId = UserId
                };

                await _orderStorages.AddAsync(order);
                await _cartsStorages.ClearAsync(UserId);

                TempData["OrderId"] = order.Id.ToString();
                return RedirectToAction(nameof(OrderSuccess));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Произошла ошибка при оформлении заказа. Order/Buy");
                return View("Error");
            }
        }

        public IActionResult OrderSuccess()
        {
            ViewData["OrderId"] = TempData["OrderId"];
            return View();
        }
    }
}
