using OnlineShop.Db.Models;

namespace OnlineShopWebApp.Models
{
    public class FavoriteViewModel
    {
        public Guid Id { get; set; }
        public string UserId { get; set; } = "UserId";
        public List<Product> Items {  get; set; }
    }
}
