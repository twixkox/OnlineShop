namespace OnlineShopWebApp.Helpers
{
    public class CookieHelper
    {
        public static void SetCookie(HttpResponse response, string key, string value, int expireDays = 30)
        {
            var options = new CookieOptions
            {
                Expires = DateTime.Now.AddDays(expireDays),
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
            };
            response.Cookies.Append(key, value, options);
        }
        public static string GetCookie(HttpRequest request, string key)
        {
            return request.Cookies[key];
        }

        public static bool Exists(HttpRequest request, string key)
        {
            return request.Cookies.ContainsKey(key);
        }

        public static void RemoveCookie(HttpResponse response, string key)
        {
            response.Cookies.Delete(key);
        }
    }

}
