using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Db.Interfaces;
using OnlineShop.Db.Models;
using OnlineShopWebApp;
using OnlineShopWebApp.Helpers;
using OnlineShopWebApp.Models;

namespace OnlineShopWebApi.Controllers
{
    [Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : Controller
    {
        private readonly ICartsStorages _cartsStorages;
        private readonly IOrderStorages _orderStorages;
        private readonly ILogger _logger;
        public OrderController(ICartsStorages cartsStorages, IOrderStorages orderStorages, ILogger logger)
        {
            _cartsStorages = cartsStorages;
            _orderStorages = orderStorages;
            _logger = logger;
        }


        [HttpGet("GetCurrentCart")]
        public async Task<IActionResult> Index(string userId)
        {
            try
            {
                await _cartsStorages.TryGetByUserIdAsync(userId);
                _logger.LogInformation($"Получение корзины пользователя id - {userId}");

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка получения корзины пользователя id - {userId}");

                return StatusCode(500);
            }
        }
        [HttpPost(nameof(Buy))]
        public async Task<IActionResult> Buy(OrderViewModel orders,string userId)
        {
            try
            {
                var cart = await _cartsStorages.TryGetByUserIdAsync(userId);
                _logger.LogInformation($"Получение корзины пользователя id - {userId}");

                var order = new Order
                {
                    DeliveryUserInfo = Mapping.ToDeliveryUserInfo(orders.DeliveryUserInfo),
                    Items = cart.Items,
                };

                await _orderStorages.AddAsync(order);
                _logger.LogInformation($"Добавление корзины к заказу");

                await _cartsStorages.ClearAsync(userId);
                _logger.LogInformation($"Очистка корзины");

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка формирования заказа");

                return StatusCode(500);
            }
        }
    }
}
