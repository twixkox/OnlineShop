using OnlineShop.Db.Models;
using OnlineShop.Db.Storages;

namespace OnlineShop.Tests
{
    public class ProductsStorageTests
    {
        private static async Task<(ProductsDbStorages Storage, Guid Conifers)> SeedAsync(OnlineShop.Db.DatabaseContext db)
        {
            var conifers = Guid.NewGuid();
            db.Products.AddRange(
                TestDb.Product("Ель голубая", 5600, 3, conifers, LightRequirement.Sun, WateringLevel.Low, 2),
                TestDb.Product("Туя западная", 1800, 30, conifers, LightRequirement.PartShade, WateringLevel.Medium, 4),
                TestDb.Product("Хоста", 650, 20, null, LightRequirement.Shade, WateringLevel.High, 3),
                TestDb.Product("Роза чайная", 890, 0, null, LightRequirement.Sun, WateringLevel.Medium, 6),
                TestDb.Product("Секатор", 1200, 12));
            await db.SaveChangesAsync();
            return (new ProductsDbStorages(db), conifers);
        }

        [Fact]
        public async Task Query_FiltersByLightAndZone()
        {
            using var db = TestDb.Create();
            var (storage, _) = await SeedAsync(db);

            var shade = await storage.QueryAsync(new ProductQuery { Light = { LightRequirement.Shade } });
            Assert.Equal(new[] { "Хоста" }, shade.Items.Select(p => p.Name));

            var hardy = await storage.QueryAsync(new ProductQuery { MaxFrostZone = 3 });
            Assert.Equal(new[] { "Ель голубая", "Хоста" }, hardy.Items.Select(p => p.Name).OrderBy(n => n));
        }

        [Fact]
        public async Task Query_FiltersByPriceStockCategoryAndSearch()
        {
            using var db = TestDb.Create();
            var (storage, conifers) = await SeedAsync(db);

            Assert.Equal(2, (await storage.QueryAsync(new ProductQuery { MinPrice = 800, MaxPrice = 1500 })).TotalCount);
            Assert.DoesNotContain((await storage.QueryAsync(new ProductQuery { InStockOnly = true })).Items, p => p.Stock == 0);
            Assert.Equal(2, (await storage.QueryAsync(new ProductQuery { CategoryId = conifers })).TotalCount);
            Assert.Equal("Туя западная", (await storage.QueryAsync(new ProductQuery { Search = "Туя" })).Items.Single().Name);
        }

        [Fact]
        public async Task Query_SortsAndPaginates()
        {
            using var db = TestDb.Create();
            var (storage, _) = await SeedAsync(db);

            var page1 = await storage.QueryAsync(new ProductQuery { Sort = ProductSort.PriceDesc, PageSize = 2, Page = 1 });
            var page3 = await storage.QueryAsync(new ProductQuery { Sort = ProductSort.PriceDesc, PageSize = 2, Page = 3 });

            Assert.Equal(5, page1.TotalCount);
            Assert.Equal(3, page1.TotalPages);
            Assert.Equal(new[] { "Ель голубая", "Туя западная" }, page1.Items.Select(p => p.Name));
            Assert.Equal("Хоста", page3.Items.Single().Name);
        }

        [Fact]
        public async Task Query_DefaultSort_PutsOutOfStockLast()
        {
            using var db = TestDb.Create();
            var (storage, _) = await SeedAsync(db);

            var all = await storage.QueryAsync(new ProductQuery());
            Assert.Equal("Роза чайная", all.Items.Last().Name);
        }

        [Fact]
        public async Task Query_PageOutOfRange_IsClamped()
        {
            using var db = TestDb.Create();
            var (storage, _) = await SeedAsync(db);

            var result = await storage.QueryAsync(new ProductQuery { Page = 99, PageSize = 2 });
            Assert.Equal(3, result.Page);
        }

        [Fact]
        public async Task ReserveStock_DecrementsAll_OrNothing()
        {
            using var db = TestDb.Create();
            var (storage, _) = await SeedAsync(db);
            var spruce = db.Products.Single(p => p.Name == "Ель голубая");
            var thuja = db.Products.Single(p => p.Name == "Туя западная");

            var missing = await storage.TryReserveStockAsync(new[] { (spruce.Id, 5), (thuja.Id, 1) });
            Assert.Equal(new[] { "Ель голубая" }, missing);
            Assert.Equal(30, thuja.Stock); // ничего не списали

            missing = await storage.TryReserveStockAsync(new[] { (spruce.Id, 3), (thuja.Id, 1) });
            Assert.Empty(missing);
            Assert.Equal(0, spruce.Stock);
            Assert.Equal(29, thuja.Stock);
        }
    }
}
