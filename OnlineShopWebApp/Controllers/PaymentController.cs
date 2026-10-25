using Microsoft.AspNetCore.Mvc;
using OnlineShop.Db.Interfaces;
using OnlineShop.Db.Models;
using OnlineShopWebApp.Services;
using System.Text.Json;

namespace OnlineShopWebApp.Controllers
{
    /// <summary>
    /// Уведомления ЮKassa о статусе платежа (HTTP-уведомления, «webhook»).
    /// Адрес для настройки в личном кабинете ЮKassa: https://ваш-сайт/payment/yookassa
    /// </summary>
    [IgnoreAntiforgeryToken]
    public class PaymentController : Controller
    {
        private readonly IOrderStorages _orders;
        private readonly IPaymentService _payments;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(IOrderStorages orders, IPaymentService payments, ILogger<PaymentController> logger)
        {
            _orders = orders;
            _payments = payments;
            _logger = logger;
        }

        [HttpPost("/payment/yookassa")]
        public async Task<IActionResult> YooKassaWebhook()
        {
            try
            {
                using var doc = await JsonDocument.ParseAsync(Request.Body);
                var paymentId = doc.RootElement.GetProperty("object").GetProperty("id").GetString();
                if (string.IsNullOrEmpty(paymentId)) return Ok();

                // Данным из уведомления не доверяем: статус всегда перепроверяем запросом к API ЮKassa
                var status = await _payments.GetPaymentStatusAsync(paymentId);
                var order = await _orders.TryGetByPaymentIdAsync(paymentId);
                if (order == null) return Ok();
                // ЮKassa недоступна — просим повторить уведомление позже
                if (status == null) return StatusCode(StatusCodes.Status503ServiceUnavailable);

                var newStatus = status switch
                {
                    "succeeded" => PaymentStatus.Paid,
                    "canceled" => PaymentStatus.Failed,
                    _ => order.PaymentStatus
                };

                if (newStatus != order.PaymentStatus)
                {
                    await _orders.UpdatePaymentAsync(order.Id, newStatus);
                    _logger.LogInformation("ЮKassa: заказ {OrderId} — платёж {Status}", order.Id, status);
                }

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка обработки уведомления ЮKassa");
                // Код ошибки — ЮKassa повторит уведомление
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
