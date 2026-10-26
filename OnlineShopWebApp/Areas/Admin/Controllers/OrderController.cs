using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Db.Interfaces;
using OnlineShop.Db.Models;
using OnlineShopWebApp.Helpers;
using OnlineShopWebApp.Services;

namespace OnlineShopWebApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Manager")]
    public class OrderController : Controller
    {
        private readonly IOrderStorages _orders;
        private readonly ILogger<OrderController> _logger;
        private readonly OrderNotificationService _notifications;
        public OrderController(IOrderStorages orders, OrderNotificationService notifications, ILogger<OrderController> logger)
        {
            _notifications = notifications;
            _orders = orders;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var orders = await _orders.GetAllAsync();
            try
            {
                _logger.LogDebug("Получение списка заказов. Всего {OrdersCount} заказов.", orders.Count);

                return View(orders.OrderByDescending(o => o.CreationDateOrder).ToList().ToOrdersViewModels());
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
            _logger.LogDebug("Получение заказа с Id - {OrderId}", orderId);
            var order = await _orders.TryGetByIdAsync(orderId);
            try
            {
                return View(order.ToOrderViewModel());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Произошла ошибка при получении заказа с Id - {OrderId}. Order/DetailAsync", orderId);
                return View("Error");
            }
        }
        [HttpPost]
        public async Task<IActionResult> UpdateOrderStatus(Guid orderId, OrderStatus status)
        {
            try
            {
                var before = await _orders.TryGetByIdAsync(orderId);
                if (before == null) return NotFound();
                var changed = before.Status != status;

                await _orders.UpdateStatusAsync(orderId, status);
                _logger.LogInformation("Обновление статуса заказа Id - {Id} выполнено", orderId);

                // Покупатель получает письмо о новом статусе
                if (changed)
                {
                    before.Status = status;
                    var url = Url.Action("DetailOrder", "Profile", new { area = "", orderId }, Request.Scheme) ?? "";
                    await _notifications.StatusChangedAsync(before, url);
                }

                TempData["SuccessMessage"] = changed
                    ? $"Статус заказа № {OrderNotificationService.Number(before)}: {OrderNotificationService.StatusName(status).ToLower()}"
                    : "Статус не изменился";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка обновления статуса заказа Id - {OrderId}. Order/UpdateOrderStatus", orderId);
                return View("Error");
            }
        }
    }
}
