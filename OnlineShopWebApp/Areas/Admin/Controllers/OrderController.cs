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
        private readonly ILogger<OrderController> _logger;
        public OrderController(IOrderStorages orders, ILogger<OrderController> logger)
        {
            _orders = orders;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var orders = await _orders.GetAllAsync();
            try
            {
                _logger.LogInformation($"Получение списка заказов. Всего {orders.Count} заказов.");

                return View(orders.ToOrdersViewModels());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка получения всех заказов. Order/Index");
                return View("Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> DetailAsync(Guid orderId)
        {
            _logger.LogInformation($"Получение заказа с Id - {orderId}");
            var order = await _orders.TryGetByIdAsync(orderId);
            try
            {
                return View(order.ToOrderViewModel());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Произошла ошибка при получении заказа с Id - {orderId}. Order/DetailAsync");
                return View("Error");
            }
        }
        [HttpPost]
        public async Task<IActionResult> UpdateOrderStatus(Guid orderId, OrderStatus status)
        {
            await _orders.UpdateStatusAsync(orderId, status);
            try
            {
                _logger.LogInformation("Обновление статуса заказа Id - {Id} выполнено", orderId);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка обновления статуса заказа Id - {orderId}. Order/UpdateOrderStatus");
                return View("Error");
            }
        }
    }
}
