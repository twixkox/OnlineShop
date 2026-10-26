namespace OnlineShop.Db.Models
{
    public class Order
    {
        public Guid Id { get; set; }

        public string UserId { get; set; }

        public List<CartItem> Items { get; set; }

        public DeliveryUserInfo DeliveryUserInfo { get; set; }

        public DateTime CreationDateOrder { get; set; } = DateTime.Now;

        public OrderStatus Status { get; set; } = OrderStatus.Created;

        public string? PromoCode { get; set; }

        public int DiscountPercent { get; set; }

        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.OnDelivery;

        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.NotRequired;

        /// <summary>Идентификатор платежа в ЮKassa</summary>
        public string? PaymentId { get; set; }
    }
}
