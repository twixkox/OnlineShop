using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace OnlineShopWebApp.Areas.Client.Models
{
    public class OrderViewModel
    {
        public Guid Id { get; set; }

        [ValidateNever]
        public string UserId { get; set; }

        [ValidateNever]
        public List<CartItemViewModel> Items { get; set; }

        [Required]
        public DateTime CreationDateOrder { get; set; }
        [Required]
        public DeliveryUserInfoViewModel DeliveryUserInfo { get; set; }

        [Required]
        public OrderStatusViewModel Status { get; set; }

        public string? PromoCode { get; set; }

        public int DiscountPercent { get; set; }

        [Display(Name = "Способ оплаты")]
        public OnlineShop.Db.Models.PaymentMethod PaymentMethod { get; set; }

        public OnlineShop.Db.Models.PaymentStatus PaymentStatus { get; set; }

        /// <summary>Сумма без скидки</summary>
        public decimal? Subtotal => Items?.Sum(item => item.Cost);

        /// <summary>Итого со скидкой</summary>
        public decimal? TotalCost => Subtotal.HasValue ? OnlineShopWebApp.Services.PromoCodeService.ApplyDiscount(Subtotal.Value, DiscountPercent) : null;

        public string Number => Id.ToString()[..8].ToUpperInvariant();

        public int? ItemsQuantity => Items?.Sum(item => item.Quantity);
    }
}
