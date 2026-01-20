namespace OnlineShop.Db.Models
{
    public class Cart
    {
        public Guid Id { get; set; }

        public string UserId { get; set; } = "UserId";

        public double Discount { get; set; }

        public List<CartItem> Items { get; set; }

        
    }
}
