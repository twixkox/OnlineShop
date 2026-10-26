using OnlineShop.Db.Interfaces;
using OnlineShop.Db.Models;

namespace OnlineShopWebApp.Services
{
    public record PromoCheckResult(bool IsValid, int DiscountPercent, string? Code, string Message);

    /// <summary>
    /// Проверка промокодов и расчёт скидки.
    /// </summary>
    public class PromoCodeService
    {
        private readonly IPromoCodeStorages _storages;

        public PromoCodeService(IPromoCodeStorages storages)
        {
            _storages = storages;
        }

        public async Task<PromoCheckResult> CheckAsync(string? code, decimal subtotal)
        {
            if (string.IsNullOrWhiteSpace(code))
                return new(false, 0, null, "Введите промокод");

            var promo = await _storages.TryGetByCodeAsync(code);
            return Check(promo, subtotal, DateTime.Now);
        }

        /// <summary>Чистая проверка без обращения к базе (удобно для тестов).</summary>
        public static PromoCheckResult Check(PromoCode? promo, decimal subtotal, DateTime now)
        {
            if (promo == null || !promo.IsActive)
                return new(false, 0, null, "Такого промокода нет");

            if (promo.ValidUntil.HasValue && promo.ValidUntil.Value.Date < now.Date)
                return new(false, 0, null, "Срок действия промокода истёк");

            if (subtotal < promo.MinOrderAmount)
                return new(false, 0, null, $"Промокод действует при заказе от {promo.MinOrderAmount:N0} ₽");

            var percent = Math.Clamp(promo.DiscountPercent, 0, 90);
            return new(true, percent, promo.Code, $"Промокод {promo.Code} применён: скидка {percent}%");
        }

        public static decimal ApplyDiscount(decimal subtotal, double percent)
        {
            if (percent <= 0) return subtotal;
            return Math.Round(subtotal * (1 - (decimal)percent / 100m), 0, MidpointRounding.AwayFromZero);
        }
    }
}
