using Microsoft.AspNetCore.Mvc;
using OnlineShop.Db.Interfaces;
using OnlineShopWebApp.Helpers;
using System.Security.Claims;

namespace OnlineShopWebApp.Controllers
{
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

        public async Task<IActionResult> Index()
        {
            _logger.LogInformation($"Получение пользовательского Id");
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            try
            {
                _logger.LogInformation($"Получение корзины пользователя");
                var cart = await _cartsStorage.TryGetByUserIdAsync(userId);

                return View(cart.ToCartViewModel());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,$"Произошла ошибка получения корзины пользователя. Cart/Index");
                return View("Error");
            }
        }
       
        public async Task<IActionResult> Add(Guid productId, int quantity = 1)
        {
            _logger.LogInformation($"Получение пользовательского Id");
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            try
            { 
                if (userId == null)
                {
                    _logger.LogInformation($"Пользователь не авторизован. Перенаправление на авторизацию");
                    TempData["InfoMessage"] = "Для продолжения необходимо авторизоваться";
                    return RedirectToAction("Authorization", "Authorization");
                }
                _logger.LogInformation($"Добавление товара в корзину пользователя");
                var product = await _productStorage.TryGetProductByIdAsync(productId);

                await _cartsStorage.AddAsync(product, userId);

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,$"Ошибка при добавлении товара в корзину. Id Товара -{productId}.Id Пользователя - {userId}");
                return View("Error");
            }
        }

        public async Task<IActionResult> Clear(string userId)
        {
            _logger.LogInformation($"Очистка корзины пользователя");
            await _cartsStorage.ClearAsync(userId);
            try
            {
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Произошла ошибка при очистке корзины у пользователя Id - {userId}");
                return View("Error");
            }
        }

        public async Task<IActionResult> Subtract(Guid productId)
        {
            _logger.LogInformation($"Получение пользовательского Id");
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            try
            {
                _logger.LogInformation($"Изменение кол-ва товаров в корзине");
                await _cartsStorage.SubtractAsync(productId, userId);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Произошла ошибка при изменении кол-ва товаров. Cart/Subtract");
                return View("Error");
            }
        }
    }
}
