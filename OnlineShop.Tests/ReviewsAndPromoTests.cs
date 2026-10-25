using OnlineShop.Db.Models;
using OnlineShop.Db.Storages;
using OnlineShopWebApp.Services;

namespace OnlineShop.Tests
{
    public class ReviewsStorageTests
    {
        [Fact]
        public async Task Save_SecondReviewFromSameUser_UpdatesFirst()
        {
            using var db = TestDb.Create();
            var p = TestDb.Product("Гортензия", 890);
            db.Products.Add(p);
            await db.SaveChangesAsync();
            var reviews = new ReviewsDbStorages(db);

            await reviews.SaveAsync(new Review { ProductId = p.Id, UserId = "u1", UserName = "Анна", Rating = 3 });
            await reviews.SaveAsync(new Review { ProductId = p.Id, UserId = "u1", UserName = "Анна", Rating = 5, Text = "Отлично" });
            await reviews.SaveAsync(new Review { ProductId = p.Id, UserId = "u2", UserName = "Иван", Rating = 4 });

            var list = await reviews.GetByProductAsync(p.Id);
            Assert.Equal(2, list.Count);

            var ratings = await reviews.GetRatingsAsync(new[] { p.Id });
            Assert.Equal(4.5, ratings[p.Id].Average);
            Assert.Equal(2, ratings[p.Id].Count);
        }

        [Fact]
        public async Task Save_ClampsRating()
        {
            using var db = TestDb.Create();
            var p = TestDb.Product("Пион", 890);
            db.Products.Add(p);
            await db.SaveChangesAsync();
            var reviews = new ReviewsDbStorages(db);

            await reviews.SaveAsync(new Review { ProductId = p.Id, UserId = "u1", UserName = "Анна", Rating = 10 });
            Assert.Equal(5, (await reviews.GetByProductAsync(p.Id)).Single().Rating);
        }
    }

    public class PromoCodeTests
    {
        private static readonly DateTime Today = new(2026, 5, 1);

        [Fact]
        public void Check_ValidCode() =>
            Assert.True(PromoCodeService.Check(new PromoCode { Code = "SPRING10", DiscountPercent = 10 }, 1000, Today).IsValid);

        [Fact]
        public void Check_Unknown() => Assert.False(PromoCodeService.Check(null, 1000, Today).IsValid);

        [Fact]
        public void Check_Inactive() =>
            Assert.False(PromoCodeService.Check(new PromoCode { Code = "X", DiscountPercent = 10, IsActive = false }, 1000, Today).IsValid);

        [Fact]
        public void Check_Expired() =>
            Assert.False(PromoCodeService.Check(new PromoCode { Code = "X", DiscountPercent = 10, ValidUntil = Today.AddDays(-1) }, 1000, Today).IsValid);

        [Fact]
        public void Check_LastDayIsStillValid() =>
            Assert.True(PromoCodeService.Check(new PromoCode { Code = "X", DiscountPercent = 10, ValidUntil = Today }, 1000, Today).IsValid);

        [Fact]
        public void Check_BelowMinimum()
        {
            var result = PromoCodeService.Check(new PromoCode { Code = "GARDEN15", DiscountPercent = 15, MinOrderAmount = 5000 }, 4999, Today);
            Assert.False(result.IsValid);
            Assert.Contains("5", result.Message);
        }

        [Fact]
        public void Check_DiscountIsCappedAt90() =>
            Assert.Equal(90, PromoCodeService.Check(new PromoCode { Code = "X", DiscountPercent = 150 }, 1000, Today).DiscountPercent);

        [Theory]
        [InlineData(17690, 10, 15921)]
        [InlineData(1000, 0, 1000)]
        [InlineData(999, 15, 849)]
        public void ApplyDiscount_RoundsToRubles(decimal subtotal, double percent, decimal expected) =>
            Assert.Equal(expected, PromoCodeService.ApplyDiscount(subtotal, percent));

        [Fact]
        public async Task Storage_NormalizesCode()
        {
            using var db = TestDb.Create();
            var storage = new PromoCodesDbStorages(db);
            await storage.AddAsync(new PromoCode { Code = "  summer20 ", DiscountPercent = 20 });

            Assert.NotNull(await storage.TryGetByCodeAsync("Summer20"));
        }
    }
}
