namespace OnlineShop.Db.Models
{
    public class PromoCode
    {
        public Guid Id { get; set; }

        /// <summary>Код, который вводит покупатель (хранится в верхнем регистре)</summary>
        public string Code { get; set; } = "";

        /// <summary>Скидка в процентах (1–90)</summary>
        public int DiscountPercent { get; set; }

        /// <summary>Минимальная сумма заказа для применения</summary>
        public decimal MinOrderAmount { get; set; }

        public DateTime? ValidUntil { get; set; }

        public bool IsActive { get; set; } = true;

        public string? Description { get; set; }
    }
}
