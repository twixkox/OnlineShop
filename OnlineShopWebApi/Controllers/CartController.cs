using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Db.Interfaces;
using OnlineShopWebApp;

namespace OnlineShopWebApi.Controllers
{
    [Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    [Route("api/[controller]")]
    public class CartController : Controller
    {
        private readonly ICartsStorages _cartsStorage;
        private readonly IProductStorages _productStorage;
        private readonly ILogger<CartController> _logger;

        public CartController(ICartsStorages cartsStorage, IProductStorages productStorage, ILogger<CartController> logger)
        {
            _cartsStorage = cartsStorage;
            _productStorage = productStorage;
            _logger = logger;
        }

        [HttpGet("GetCart")]
        public async Task<IActionResult> Index(string userId)
        {
            var cart = await _cartsStorage.TryGetByUserIdAsync(userId);
            _logger.LogInformation($"Получение корзины пользователя id - {userId}");
            try
            {
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка получения корзины пользователя id - {userId}");

                return StatusCode(500);
            }
            
        }

        [HttpPost("AddProductInCart")]
        public async Task<IActionResult> Add(Guid productId)
        {
            var product = await _productStorage.TryGetProductByIdAsync(productId);
            _logger.LogInformation($"Получение продукта с id - {productId}");

            await _cartsStorage.AddAsync(product, Constants.UserId);
            _logger.LogInformation($"Продукт был успешно добавлен в корзину");
            try
            {
                return Created();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Произошла ошибка добавления продукта. Id - {productId}");

                return StatusCode(500);
            }
        }

        [HttpDelete("ClearCart")]
        public IActionResult Clear(string userId)
        {
            _cartsStorage.ClearAsync(userId);
            _logger.LogInformation($"Очистка корзыины пользователя id - {userId} выполнена");

            try
            {
                return Ok();
            }
            catch(Exception ex)
            {
                _logger.LogError(ex,$"Ошибка очистки корзины пользователя id - {userId}");

                return StatusCode(500);
            }
        }

        [HttpPatch(nameof(Subtract))]
        public IActionResult Subtract(Guid productId, string userId)
        {
            _cartsStorage.SubtractAsync(productId, userId);
            try
            {
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка подписки на количество id - {userId}");

                return StatusCode(500);
            }
        }
    }
}
