using Microsoft.AspNetCore.Mvc;
using OnlineShop.Db.Interfaces;
using OnlineShop.Db.Models;
using OnlineShopWebApp.Helpers;

namespace OnlineShopWebApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class OrderController : Controller
    {
        private readonly IOrderStorages _orders;
        public OrderController(IOrderStorages orders)
        {
            _orders = orders;
        }

        public async Task<IActionResult> Index()
        {
            var orders = await _orders.GetAllAsync();

            return View(orders.ToOrdersViewModels());
        }

        public async Task<IActionResult> DetailAsync(Guid orderId)
        {
            var order = await _orders.TryGetByIdAsync(orderId);

            return View(order.ToOrderViewModel());
        }
        [HttpPost]
        public async Task<IActionResult> UpdateOrderStatus(Guid orderId, OrderStatus status)
        {
            await _orders.UpdateStatusAsync(orderId, status);

            return RedirectToAction(nameof(Index));
        }
    }
}
