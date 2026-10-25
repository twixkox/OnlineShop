using OnlineShop.Db.Storages;

namespace OnlineShop.Tests
{
    public class CartsStorageTests
    {
        [Fact]
        public async Task Add_RespectsStock()
        {
            using var db = TestDb.Create();
            var spruce = TestDb.Product("Ель", 5600, stock: 2);
            db.Products.Add(spruce);
            await db.SaveChangesAsync();
            var carts = new CartsDbStorages(db);

            Assert.True(await carts.AddAsync(spruce, "user"));
            Assert.True(await carts.AddAsync(spruce, "user"));
            Assert.False(await carts.AddAsync(spruce, "user"));

            var cart = await carts.TryGetByUserIdAsync("user");
            Assert.Equal(2, cart.Items.Single().Quantity);
        }

        [Fact]
        public async Task Add_OutOfStock_DoesNotCreateCart()
        {
            using var db = TestDb.Create();
            var rose = TestDb.Product("Роза", 890, stock: 0);
            db.Products.Add(rose);
            await db.SaveChangesAsync();
            var carts = new CartsDbStorages(db);

            Assert.False(await carts.AddAsync(rose, "user"));
            Assert.Null(await carts.TryGetByUserIdAsync("user"));
        }

        [Fact]
        public async Task Subtract_ToZero_RemovesItem()
        {
            using var db = TestDb.Create();
            var p = TestDb.Product("Туя", 1800);
            db.Products.Add(p);
            await db.SaveChangesAsync();
            var carts = new CartsDbStorages(db);

            await carts.AddAsync(p, "user");
            await carts.SubtractAsync(p.Id, "user");

            Assert.Empty((await carts.TryGetByUserIdAsync("user")).Items);
        }

        [Fact]
        public async Task Merge_IntoEmptyAccount_TransfersCart()
        {
            using var db = TestDb.Create();
            var p = TestDb.Product("Пион", 890);
            db.Products.Add(p);
            await db.SaveChangesAsync();
            var carts = new CartsDbStorages(db);

            await carts.AddAsync(p, "guest:abc");
            await carts.SetPromoCodeAsync("guest:abc", "SPRING10", 10);
            await carts.MergeAsync("guest:abc", "user");

            Assert.Null(await carts.TryGetByUserIdAsync("guest:abc"));
            var cart = await carts.TryGetByUserIdAsync("user");
            Assert.Single(cart.Items);
            Assert.Equal("SPRING10", cart.PromoCode);
        }

        [Fact]
        public async Task Merge_IntoExistingCart_SumsQuantitiesWithinStock()
        {
            using var db = TestDb.Create();
            var p = TestDb.Product("Лилия", 320, stock: 3);
            var q = TestDb.Product("Хоста", 650);
            db.Products.AddRange(p, q);
            await db.SaveChangesAsync();
            var carts = new CartsDbStorages(db);

            await carts.AddAsync(p, "user");
            await carts.AddAsync(p, "user");
            await carts.AddAsync(p, "guest:abc");
            await carts.AddAsync(p, "guest:abc");
            await carts.AddAsync(q, "guest:abc");

            await carts.MergeAsync("guest:abc", "user");

            var cart = await carts.TryGetByUserIdAsync("user");
            Assert.Equal(3, cart.Items.Single(i => i.Product.Id == p.Id).Quantity); // 2 + 2, но на складе 3
            Assert.Equal(1, cart.Items.Single(i => i.Product.Id == q.Id).Quantity);
            Assert.Null(await carts.TryGetByUserIdAsync("guest:abc"));
        }
    }
}
