using OnlineShop.Db.Models;

namespace OnlineShopWebApp.Areas.Client.Models
{
    public class FavoriteViewModel
    {
        public Guid Id { get; set; }
        public string UserId { get; set; }
        public List<Product> Items {  get; set; }
    }
}
