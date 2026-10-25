using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Db.Interfaces;
using OnlineShop.Db.Models;
using OnlineShopWebApp.Areas.Client.Models;
using OnlineShopWebApp.Helpers;
using OnlineShopWebApp.Services;
using System.Security.Claims;

namespace OnlineShopWebApp.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly ICartsStorages _cartsStorages;
        private readonly IOrderStorages _orderStorages;
        private readonly IProductStorages _productStorages;
        private readonly PromoCodeService _promo;
        private readonly IPaymentService _payments;
        private readonly OrderNotificationService _notifications;
        private readonly UserManager<User> _users;
        private readonly ILogger<OrderController> _logger;

        public OrderController(ICartsStorages cartsStorages, IOrderStorages orderStorages, IProductStorages productStorages,
            PromoCodeService promo, IPaymentService payments, OrderNotificationService notifications, UserManager<User> users,
            ILogger<OrderController> logger)
        {
            _cartsStorages = cartsStorages;
            _orderStorages = orderStorages;
            _productStorages = productStorages;
            _promo = promo;
            _payments = payments;
            _notifications = notifications;
            _users = users;
            _logger = logger;
        }

        private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        public async Task<IActionResult> Index()
        {
            try
            {
                var cartViewModel = (await _cartsStorages.TryGetByUserIdAsync(UserId)).ToCartViewModel();

                if (cartViewModel == null || cartViewModel.Items.Count == 0)
                {
                    TempData["InfoMessage"] = "Корзина пуста — добавьте товары, чтобы оформить заказ";
                    return RedirectToAction("Index", "Cart");
                }

                // Подставляем данные из профиля, чтобы не вводить их каждый раз
                var user = await _users.FindByIdAsync(UserId);
                var order = new OrderViewModel
                {
                    Items = cartViewModel.Items,
                    PromoCode = cartViewModel.PromoCode,
                    DiscountPercent = (int)cartViewModel.Discount,
                    DeliveryUserInfo = new DeliveryUserInfoViewModel
                    {
                        UserName = $"{user?.FirstName} {user?.LastName}".Trim(),
                        Phone = user?.PhoneNumber ?? "",
                        Adress = "",
                        Apartment = "",
                        DeliveryDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1))
                    }
                };

                ViewBag.OnlinePaymentEnabled = _payments.IsEnabled;
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

                var cartViewModel = cart.ToCartViewModel();

                if (!ModelState.IsValid)
                {
                    orders.Items = cartViewModel.Items;
                    orders.PromoCode = cartViewModel.PromoCode;
                    orders.DiscountPercent = (int)cartViewModel.Discount;
                    ViewBag.OnlinePaymentEnabled = _payments.IsEnabled;
                    return View("Index", orders);
                }

                // Промокод проверяем ещё раз: он мог истечь, пока покупатель заполнял форму
                var discount = 0;
                string? promoCode = null;
                if (!string.IsNullOrEmpty(cart.PromoCode))
                {
                    var check = await _promo.CheckAsync(cart.PromoCode, cartViewModel.Subtotal);
                    if (check.IsValid)
                    {
                        discount = check.DiscountPercent;
                        promoCode = check.Code;
                    }
                }

                // Списываем остатки; если чего-то не хватает — возвращаем в корзину
                var missing = await _productStorages.TryReserveStockAsync(cart.Items.Select(i => (i.Product.Id, i.Quantity)));
                if (missing.Count > 0)
                {
                    TempData["ErrorMessage"] = $"Недостаточно на складе: {string.Join(", ", missing)}. Измените количество в корзине.";
                    return RedirectToAction("Index", "Cart");
                }

                var online = orders.PaymentMethod == PaymentMethod.Online && _payments.IsEnabled;
                var order = new Order
                {
                    DeliveryUserInfo = orders.DeliveryUserInfo.ToDeliveryUserInfo(),
                    Items = cart.Items,
                    UserId = UserId,
                    PromoCode = promoCode,
                    DiscountPercent = discount,
                    PaymentMethod = online ? PaymentMethod.Online : PaymentMethod.OnDelivery,
                    PaymentStatus = online ? PaymentStatus.Pending : PaymentStatus.NotRequired
                };

                await _orderStorages.AddAsync(order);
                await _cartsStorages.ClearAsync(UserId);

                await _notifications.OrderCreatedAsync(order, OrderUrl(order.Id));

                if (online) return await StartPaymentAsync(order);

                TempData["OrderId"] = order.Id.ToString();
                return RedirectToAction(nameof(OrderSuccess));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Произошла ошибка при оформлении заказа. Order/Buy");
                return View("Error");
            }
        }

        /// <summary>Повторная попытка оплаты заказа (например, если покупатель закрыл страницу оплаты).</summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Pay(Guid orderId)
        {
            var order = await _orderStorages.TryGetByIdAsync(orderId);
            if (order == null || order.UserId != UserId) return NotFound();

            if (order.PaymentMethod != PaymentMethod.Online || order.PaymentStatus == PaymentStatus.Paid || !_payments.IsEnabled)
            {
                return RedirectToAction("DetailOrder", "Profile", new { orderId });
            }

            return await StartPaymentAsync(order);
        }

        /// <summary>Сюда ЮKassa возвращает покупателя после оплаты.</summary>
        public async Task<IActionResult> PaymentReturn(Guid orderId)
        {
            var order = await _orderStorages.TryGetByIdAsync(orderId);
            if (order == null || order.UserId != UserId) return NotFound();

            if (!string.IsNullOrEmpty(order.PaymentId))
            {
                var status = await _payments.GetPaymentStatusAsync(order.PaymentId);
                if (status == "succeeded")
                {
                    await _orderStorages.UpdatePaymentAsync(order.Id, PaymentStatus.Paid);
                    TempData["OrderId"] = order.Id.ToString();
                    TempData["Paid"] = true;
                    return RedirectToAction(nameof(OrderSuccess));
                }

                if (status == "canceled")
                {
                    await _orderStorages.UpdatePaymentAsync(order.Id, PaymentStatus.Failed);
                    TempData["ErrorMessage"] = "Оплата не прошла. Попробуйте ещё раз или выберите оплату при получении — позвоните нам.";
                }
                else
                {
                    TempData["InfoMessage"] = "Платёж обрабатывается — статус обновится автоматически.";
                }
            }

            return RedirectToAction("DetailOrder", "Profile", new { orderId });
        }

        public IActionResult OrderSuccess()
        {
            ViewData["OrderId"] = TempData["OrderId"];
            ViewData["Paid"] = TempData["Paid"];
            return View();
        }

        private async Task<IActionResult> StartPaymentAsync(Order order)
        {
            try
            {
                var amount = OrderNotificationService.Total(order);
                var returnUrl = Url.Action(nameof(PaymentReturn), "Order", new { orderId = order.Id }, Request.Scheme)!;
                var payment = await _payments.CreatePaymentAsync(order.Id, amount, $"Заказ № {OrderNotificationService.Number(order)} в PlantShop", returnUrl);

                await _orderStorages.UpdatePaymentAsync(order.Id, PaymentStatus.Pending, payment.PaymentId);
                return Redirect(payment.ConfirmationUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Не удалось создать платёж для заказа {OrderId}", order.Id);
                TempData["ErrorMessage"] = "Заказ оформлен, но перейти к оплате не получилось. Нажмите «Оплатить» на странице заказа.";
                return RedirectToAction("DetailOrder", "Profile", new { orderId = order.Id });
            }
        }

        private string OrderUrl(Guid orderId) =>
            Url.Action("DetailOrder", "Profile", new { orderId }, Request.Scheme) ?? "";
    }
}
