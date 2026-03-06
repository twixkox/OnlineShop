namespace OnlineShop.Db.Models
{
    public class Category
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string IdentityUrl { get; set; }
        public string? Description { get; set; }

        public string PhotoPath { get; set; } = "uploads/category/anyCategory.png";
        public ICollection<Product>? Products { get; set; }
    }
}
