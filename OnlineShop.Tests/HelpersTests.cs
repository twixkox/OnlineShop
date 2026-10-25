using Microsoft.AspNetCore.Http;
using OnlineShop.Db.Models;
using OnlineShopWebApp.Areas.Client.Models;
using OnlineShopWebApp.Helpers;
using OnlineShopWebApp.Services;
using System.Security.Claims;
using System.Text.Json;

namespace OnlineShop.Tests
{
    public class HelpersTests
    {
        [Theory]
        [InlineData("uploads/products/original/a/a.png", "/uploads/products/original/a/a.png")]
        [InlineData("/uploads/x.png", "/uploads/x.png")]
        [InlineData(@"uploads\users\avatars\x.jpg", "/uploads/users/avatars/x.jpg")]
        [InlineData("uploads/products/original/anyProduct.png", UiHelper.PlaceholderImage)]
        [InlineData(null, UiHelper.PlaceholderImage)]
        public void ImageUrl_NormalizesPaths(string? input, string expected) => Assert.Equal(expected, UiHelper.ImageUrl(input));

        [Theory]
        [InlineData(1, "отзыв")]
        [InlineData(3, "отзыва")]
        [InlineData(5, "отзывов")]
        [InlineData(11, "отзывов")]
        [InlineData(21, "отзыв")]
        [InlineData(104, "отзыва")]
        public void Plural_Russian(int n, string expected) => Assert.Equal(expected, UiHelper.Plural(n, "отзыв", "отзыва", "отзывов"));

        [Fact]
        public void CatalogFilter_RouteValues_SkipEmptyAndDefaults()
        {
            var filter = new CatalogFilter { Light = { LightRequirement.Shade }, Stock = true, Sort = ProductSort.PriceAsc };
            var values = filter.ToRouteValues(page: 2);

            Assert.Equal("Shade", values["light[0]"]);
            Assert.Equal("true", values["stock"]);
            Assert.Equal("PriceAsc", values["sort"]);
            Assert.Equal("2", values["page"]);
            Assert.False(values.ContainsKey("q"));
            Assert.False(filter.ToRouteValues(page: 1, sort: ProductSort.Default).ContainsKey("sort"));
        }

        [Fact]
        public void YooKassaRequest_FormatsAmountWithDot()
        {
            var json = JsonSerializer.Serialize(YooKassaPaymentService.BuildCreateRequest(Guid.Empty, 15921.5m, "Заказ", "https://shop/return"));
            using var doc = JsonDocument.Parse(json);

            Assert.Equal("15921.50", doc.RootElement.GetProperty("amount").GetProperty("value").GetString());
            Assert.Equal("RUB", doc.RootElement.GetProperty("amount").GetProperty("currency").GetString());
            Assert.True(doc.RootElement.GetProperty("capture").GetBoolean());
            Assert.Equal("https://shop/return", doc.RootElement.GetProperty("confirmation").GetProperty("return_url").GetString());
        }

        private static CartOwnerProvider Provider(string? cookie, string? userId = null)
        {
            var context = new DefaultHttpContext();
            if (cookie != null) context.Request.Headers.Cookie = $"{CartOwnerProvider.CookieName}={cookie}";
            if (userId != null) context.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId) }, "test"));
            return new CartOwnerProvider(new HttpContextAccessor { HttpContext = context });
        }

        [Fact]
        public void CartOwner_AuthenticatedUser_UsesUserId() =>
            Assert.Equal("user-1", Provider("0123456789abcdef0123456789abcdef", "user-1").GetOwnerId());

        [Fact]
        public void CartOwner_ValidGuestCookie() =>
            Assert.Equal("guest:0123456789abcdef0123456789abcdef", Provider("0123456789abcdef0123456789abcdef").GetGuestOwnerId());

        [Fact]
        public void CartOwner_RejectsForgedCookie()
        {
            // Подставить в cookie чужой id пользователя нельзя
            Assert.Null(Provider("8f5c1a8e-real-user-id").GetGuestOwnerId());
        }

        [Fact]
        public void CartOwner_CreatesGuestIdWhenMissing()
        {
            var provider = Provider(null);
            var id = provider.GetOwnerId();
            Assert.StartsWith("guest:", id);
            Assert.Equal(id, provider.GetOwnerId());
        }
    }
}
