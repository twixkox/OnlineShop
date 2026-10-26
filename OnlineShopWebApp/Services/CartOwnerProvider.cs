using System.Security.Claims;

namespace OnlineShopWebApp.Services
{
    /// <summary>
    /// Определяет, чья корзина: авторизованного пользователя или гостя (по cookie).
    /// </summary>
    public interface ICartOwnerProvider
    {
        /// <summary>Id владельца корзины. Для гостя при необходимости создаёт cookie.</summary>
        string GetOwnerId(bool createIfMissing = true);

        /// <summary>Id гостевой корзины из cookie (если есть).</summary>
        string? GetGuestOwnerId();

        void ForgetGuest();
    }

    public class CartOwnerProvider : ICartOwnerProvider
    {
        public const string CookieName = "ps_guest_cart";
        private const string Prefix = "guest:";

        private readonly IHttpContextAccessor _accessor;
        private string? _created;

        public CartOwnerProvider(IHttpContextAccessor accessor)
        {
            _accessor = accessor;
        }

        private HttpContext Context => _accessor.HttpContext ?? throw new InvalidOperationException("Нет HTTP-контекста");

        public string GetOwnerId(bool createIfMissing = true)
        {
            var userId = Context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId)) return userId;

            var guest = GetGuestOwnerId();
            if (guest != null) return guest;

            if (!createIfMissing) return Prefix + "none";

            var token = Guid.NewGuid().ToString("N");
            Context.Response.Cookies.Append(CookieName, token, new CookieOptions
            {
                HttpOnly = true,
                IsEssential = true,
                SameSite = SameSiteMode.Lax,
                Secure = Context.Request.IsHttps,
                Expires = DateTimeOffset.UtcNow.AddDays(30)
            });
            _created = Prefix + token;
            return _created;
        }

        public string? GetGuestOwnerId()
        {
            if (_created != null) return _created;

            var token = Context.Request.Cookies[CookieName];
            // В cookie допускаем только наш формат, чтобы нельзя было подставить чужой userId
            return !string.IsNullOrEmpty(token) && token.Length == 32 && token.All(Uri.IsHexDigit) ? Prefix + token : null;
        }

        public void ForgetGuest()
        {
            Context.Response.Cookies.Delete(CookieName);
            _created = null;
        }
    }
}
