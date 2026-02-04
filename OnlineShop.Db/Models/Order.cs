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

    }
}
