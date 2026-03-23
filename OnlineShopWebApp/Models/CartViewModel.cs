namespace OnlineShopWebApp.Areas.Client.Models
{
    public class CartViewModel
    {
        public Guid Id { get; set; }

        public string UserId { get; set; }

        public int Discount { get; set; }

        public List<CartItemViewModel> Items { get; set; }

        public decimal TotalCost => Items.Sum(x => x.Cost);

        public int Quantity => Items.Sum(x => x.Quantity);
    }
}
