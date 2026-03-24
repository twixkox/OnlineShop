using Microsoft.AspNetCore.Mvc;
using OnlineShop.Db.Interfaces;
using OnlineShop.Db.Models;

namespace OnlineShopWebApi.Controllers.Admin
{
    [Area("Admin")]
    [ApiController]
    [Route("[controller]")]
    public class OrderController : Controller
    {
        private readonly IOrderStorages _orders;
        private readonly ILogger _logger;
        public OrderController(IOrderStorages orders, ILogger<OrderController> logger)
        {
            _logger = logger;
            _orders = orders;
        }
        [HttpGet("GetOrders")]
        public async Task<ActionResult<List<Order>>> Index()
        {
            var orders = await _orders.GetAllAsync();
            try
            {
                _logger.LogInformation("Получение списка заказов успешно");
                return Ok(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,"Ошибка получения списка заказов");
                return StatusCode(500, "Произошла ошибка получения заказов");
            }

        }
        [HttpGet(nameof(Detail))]
        public async Task<ActionResult<Order>> Detail(Guid orderId)
        {
            var order = await _orders.TryGetByIdAsync(orderId);
            try
            {
                if (order == null)
                {
                    _logger.LogWarning($"Заказ с ID {orderId} не найден");
                    return NotFound();
                }

                _logger.LogInformation($"Заказ пользователя {orderId} получен");

                return Ok(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка получения заказа от пользователя.Id заказа - {orderId}");
                return StatusCode(500, "Произошла ошибка получения заказа");
            } 
        }

        [HttpPost(nameof(UpdateOrderStatus))]
        public async Task<ActionResult<Order>> UpdateOrderStatus(Guid orderId, OrderStatus status)
        {
            var currentOrder = await _orders.TryGetByIdAsync(orderId);
            await _orders.UpdateStatusAsync(orderId, status);

            var existingOrder = await _orders.TryGetByIdAsync(orderId);
            try
            {
                if (currentOrder == null)
                {
                    _logger.LogWarning($"Заказ с ID {orderId} не найден");
                    return NotFound();
                }

                return StatusCode(201,existingOrder);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка обновления статуса заказа. Id заказа - {orderId}");
                return BadRequest();
            }
        }
    }
}
