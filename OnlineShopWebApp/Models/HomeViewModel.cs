using OnlineShop.Db.Models;

namespace OnlineShopWebApp.Areas.Client.Models
{
    public class HomeViewModel
    {

        public string HeroTitle { get; set; }
        public string HeroSubtitle { get; set; }

        public List<Category> MainCategories { get; set; }

        public List<Product> FeaturedProducts { get; set; }

        public string WorkHours { get; set; }

        public string Address { get; set; }

        public string ContactPhone { get; set; }

        public string ContactEmail { get; set; }
    }
}