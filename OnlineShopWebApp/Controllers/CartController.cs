using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Db.Interfaces;
using OnlineShopWebApp.Helpers;
using System.Security.Claims;

namespace OnlineShopWebApp.Controllers
{
    [Authorize]
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

        private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        public async Task<IActionResult> Index()
        {
            try
            {
                _logger.LogInformation("Получение корзины пользователя");
                var cart = await _cartsStorage.TryGetByUserIdAsync(UserId);

                return View(cart.ToCartViewModel());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Произошла ошибка получения корзины пользователя. Cart/Index");
                return View("Error");
            }
        }

        public async Task<IActionResult> Add(Guid productId, int quantity = 1)
        {
            try
            {
                _logger.LogInformation("Добавление товара в корзину пользователя");
                var product = await _productStorage.TryGetProductByIdAsync(productId);

                if (product == null)
                {
                    return Request.IsAjax() ? NotFound() : RedirectToAction("AllProducts", "Catalog");
                }

                await _cartsStorage.AddAsync(product, UserId);

                if (Request.IsAjax())
                {
                    return Json(await GetCartStateAsync(productId, $"«{product.Name}» в корзине"));
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при добавлении товара в корзину. Id Товара - {ProductId}", productId);
                return View("Error");
            }
        }

        public async Task<IActionResult> Subtract(Guid productId)
        {
            try
            {
                _logger.LogInformation("Изменение кол-ва товаров в корзине");
                await _cartsStorage.SubtractAsync(productId, UserId);

                if (Request.IsAjax()) return Json(await GetCartStateAsync(productId));

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Произошла ошибка при изменении кол-ва товаров. Cart/Subtract");
                return View("Error");
            }
        }

        public async Task<IActionResult> Remove(Guid productId)
        {
            try
            {
                await _cartsStorage.RemoveAsync(productId, UserId);

                if (Request.IsAjax()) return Json(await GetCartStateAsync(productId, "Товар удалён из корзины"));

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Произошла ошибка при удалении товара из корзины. Cart/Remove");
                return View("Error");
            }
        }

        // userId больше не принимается из адресной строки: раньше любой мог очистить чужую корзину
        public async Task<IActionResult> Clear()
        {
            try
            {
                _logger.LogInformation("Очистка корзины пользователя");
                await _cartsStorage.ClearAsync(UserId);
                TempData["InfoMessage"] = "Корзина очищена";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Произошла ошибка при очистке корзины");
                return View("Error");
            }
        }

        private async Task<object> GetCartStateAsync(Guid productId, string? message = null)
        {
            var cart = (await _cartsStorage.TryGetByUserIdAsync(UserId)).ToCartViewModel();
            var item = cart?.Items.FirstOrDefault(i => i.Product.Id == productId);
            var culture = System.Globalization.CultureInfo.GetCultureInfo("ru-RU");

            return new
            {
                count = cart?.Quantity ?? 0,
                total = (cart?.TotalCost ?? 0).ToString("C0", culture),
                itemQuantity = item?.Quantity ?? 0,
                itemCost = (item?.Cost ?? 0).ToString("C0", culture),
                message
            };
        }
    }
}
