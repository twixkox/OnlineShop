using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace OnlineShop.Db.Models
{
    public class Product
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public decimal Cost { get; set; }

        public Guid CategoryId { get; set; }

        [NotMapped]
        public string? CategoryName { get; set; }

        public string PhotoPath { get; set; }

        [MaybeNull]
        public string ThumbnailPath { get; set; }

        public List<CartItem> CartItems { get; set; }

        public List<Favorite> Favorite { get; set; }

        public Category? Category { get; set; }

        /// <summary>Количество на складе</summary>
        public int Stock { get; set; } = 10;

        /// <summary>Высота взрослого растения, например «до 3 м»</summary>
        public string? Height { get; set; }

        /// <summary>Зона морозостойкости USDA (чем меньше — тем морозостойче)</summary>
        public int? FrostZone { get; set; }

        public LightRequirement? Light { get; set; }

        public WateringLevel? Watering { get; set; }

        /// <summary>Период цветения или плодоношения, например «июнь–июль»</summary>
        public string? FloweringPeriod { get; set; }

        public List<Review> Reviews { get; set; } = new();
    }
}

