namespace OnlineShopWebApp.Areas.Client.Models
{
    public class CartViewModel
    {
        public Guid Id { get; set; }

        public string UserId { get; set; }

        public List<CartItemViewModel> Items { get; set; }

        public string? PromoCode { get; set; }

        /// <summary>Скидка в процентах</summary>
        public double Discount { get; set; }

        /// <summary>Сумма без скидки</summary>
        public decimal Subtotal => Items.Sum(x => x.Cost);

        public decimal DiscountAmount => Subtotal - TotalCost;

        /// <summary>Итого со скидкой</summary>
        public decimal TotalCost => OnlineShopWebApp.Services.PromoCodeService.ApplyDiscount(Subtotal, Discount);

        public int Quantity => Items.Sum(x => x.Quantity);
    }
}
