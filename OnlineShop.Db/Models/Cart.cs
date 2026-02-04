namespace OnlineShop.Db.Models
{
    public class Cart
    {
        public Guid Id { get; set; }

        public string? UserId { get; set; }

        public string? SessionId { get; set; }

        public double Discount { get; set; }

        public List<CartItem> Items { get; set; } = new List<CartItem>();

        public bool IsAnonynous => string.IsNullOrEmpty(UserId);
    }
}
