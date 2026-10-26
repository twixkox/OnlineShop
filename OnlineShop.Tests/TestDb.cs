using Microsoft.EntityFrameworkCore;
using OnlineShop.Db;
using OnlineShop.Db.Models;

namespace OnlineShop.Tests
{
    /// <summary>Отдельная база в памяти для каждого теста.</summary>
    public static class TestDb
    {
        public static DatabaseContext Create()
        {
            var options = new DbContextOptionsBuilder<DatabaseContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new DatabaseContext(options);
        }

        public static Product Product(string name, decimal cost, int stock = 10, Guid? categoryId = null,
            LightRequirement? light = null, WateringLevel? watering = null, int? zone = null) => new()
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = $"Описание: {name}",
            Cost = cost,
            Stock = stock,
            CategoryId = categoryId ?? Guid.Empty,
            PhotoPath = "uploads/products/original/anyProduct.png",
            ThumbnailPath = "uploads/products/original/anyProduct.png",
            Light = light,
            Watering = watering,
            FrostZone = zone
        };
    }
}
