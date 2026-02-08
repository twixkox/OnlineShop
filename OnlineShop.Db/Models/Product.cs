using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Diagnostics.CodeAnalysis;

namespace OnlineShop.Db.Models
{
    public class Product
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public decimal Cost { get; set; }

        public string CategoryId { get; set; }

        public string PhotoPath { get; set; }

        [MaybeNull]
        public string ThumbnailPath { get; set; }

        public List<CartItem> CartItems { get; set; }

        public List<Favorite> Favorite { get; set; }

        public Category? Category { get; set; }
    }
}

