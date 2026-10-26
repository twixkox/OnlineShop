using Microsoft.AspNetCore.Mvc;
using OnlineShop.Db.Interfaces;
using OnlineShopWebApp.Helpers;
using OnlineShopWebApp.Services;
using System.Globalization;

namespace OnlineShopWebApp.Controllers
{
    /// <summary>
    /// Корзина работает и для гостей (по cookie): после входа гостевая корзина переносится в аккаунт.
    /// </summary>
    public class CartController : Controller
    {
        private static readonly CultureInfo Ru = CultureInfo.GetCultureInfo("ru-RU");

        private readonly ICartsStorages _cartsStorage;
        private readonly IProductStorages _productStorage;
        private readonly ICartOwnerProvider _owner;
        private readonly PromoCodeService _promo;
        private readonly ILogger<CartController> _logger;

        public CartController(ICartsStorages cartsStorage, IProductStorages productStorage, ICartOwnerProvider owner,
            PromoCodeService promo, ILogger<CartController> logger)
        {
            _cartsStorage = cartsStorage;
            _productStorage = productStorage;
            _owner = owner;
            _promo = promo;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var cart = await _cartsStorage.TryGetByUserIdAsync(_owner.GetOwnerId(createIfMissing: false));
                var viewModel = cart.ToCartViewModel();

                // Промокод мог стать недействительным (истёк, сумма уменьшилась) — пересчитываем
                if (viewModel != null && !string.IsNullOrEmpty(viewModel.PromoCode))
                {
                    var check = await _promo.CheckAsync(viewModel.PromoCode, viewModel.Subtotal);
                    if (!check.IsValid)
                    {
                        await _cartsStorage.SetPromoCodeAsync(cart!.UserId!, null, 0);
                        viewModel.PromoCode = null;
                        viewModel.Discount = 0;
                        TempData["InfoMessage"] = $"Промокод снят: {check.Message.ToLower(Ru)}";
                    }
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Произошла ошибка получения корзины. Cart/Index");
                return View("Error");
            }
        }

        public async Task<IActionResult> Add(Guid productId)
        {
            try
            {
                var product = await _productStorage.TryGetProductByIdAsync(productId);
                if (product == null)
                {
                    return Request.IsAjax() ? NotFound() : RedirectToAction("AllProducts", "Catalog");
                }

                var added = await _cartsStorage.AddAsync(product, _owner.GetOwnerId());

                if (!added)
                {
                    var message = product.Stock <= 0
                        ? $"«{product.Name}» сейчас нет в наличии"
                        : $"Больше нет на складе: доступно {product.Stock} шт.";

                    if (Request.IsAjax())
                    {
                        Response.StatusCode = StatusCodes.Status409Conflict;
                        return Json(new { message });
                    }

                    TempData["ErrorMessage"] = message;
                    return RedirectToAction(nameof(Index));
                }

                if (Request.IsAjax()) return Json(await GetCartStateAsync(productId, $"«{product.Name}» в корзине"));

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
                await _cartsStorage.SubtractAsync(productId, _owner.GetOwnerId(createIfMissing: false));

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
                await _cartsStorage.RemoveAsync(productId, _owner.GetOwnerId(createIfMissing: false));

                if (Request.IsAjax()) return Json(await GetCartStateAsync(productId, "Товар удалён из корзины"));

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Произошла ошибка при удалении товара из корзины. Cart/Remove");
                return View("Error");
            }
        }

        public async Task<IActionResult> Clear()
        {
            try
            {
                await _cartsStorage.ClearAsync(_owner.GetOwnerId(createIfMissing: false));
                TempData["InfoMessage"] = "Корзина очищена";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Произошла ошибка при очистке корзины");
                return View("Error");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApplyPromo(string? code)
        {
            var ownerId = _owner.GetOwnerId(createIfMissing: false);
            var cart = (await _cartsStorage.TryGetByUserIdAsync(ownerId)).ToCartViewModel();
            if (cart == null || cart.Items.Count == 0) return RedirectToAction(nameof(Index));

            var check = await _promo.CheckAsync(code, cart.Subtotal);
            if (check.IsValid)
            {
                await _cartsStorage.SetPromoCodeAsync(ownerId, check.Code, check.DiscountPercent);
                TempData["SuccessMessage"] = check.Message;
            }
            else
            {
                TempData["ErrorMessage"] = check.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemovePromo()
        {
            await _cartsStorage.SetPromoCodeAsync(_owner.GetOwnerId(createIfMissing: false), null, 0);
            TempData["InfoMessage"] = "Промокод удалён";
            return RedirectToAction(nameof(Index));
        }

        private async Task<object> GetCartStateAsync(Guid productId, string? message = null)
        {
            var cart = (await _cartsStorage.TryGetByUserIdAsync(_owner.GetOwnerId(createIfMissing: false))).ToCartViewModel();
            var item = cart?.Items.FirstOrDefault(i => i.Product.Id == productId);

            return new
            {
                count = cart?.Quantity ?? 0,
                subtotal = (cart?.Subtotal ?? 0).ToString("C0", Ru),
                discount = (cart?.DiscountAmount ?? 0).ToString("C0", Ru),
                total = (cart?.TotalCost ?? 0).ToString("C0", Ru),
                itemQuantity = item?.Quantity ?? 0,
                itemCost = (item?.Cost ?? 0).ToString("C0", Ru),
                maxReached = item != null && item.Quantity >= item.Product.Stock,
                message
            };
        }
    }
}
