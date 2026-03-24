namespace OnlineShopWebApp.Areas.Client.Models
{
    public class CartItemViewModel
    {
        public Guid Id { get; set; }

        public int Quantity { get; set; }

        public string UserId { get; set; }

        public ProductViewModel Product { get; set; }

        public decimal Cost => Product.Cost * Quantity;
    }
}
