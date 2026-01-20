namespace OnlineShop.Db.Models
{
    public class CartItem
    {
        public Guid Id { get; set; }

        public int Quantity { get; set; }

        public Product Product { get; set; }

        public List<Order> Order { get; set; }

        public List<Cart> Cart {  get; set; }
    }
}
