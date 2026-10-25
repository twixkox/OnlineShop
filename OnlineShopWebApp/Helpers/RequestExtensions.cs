namespace OnlineShopWebApp.Helpers
{
    public static class RequestExtensions
    {
        /// <summary>
        /// Запрос отправлен из скрипта (fetch с заголовком X-Requested-With), а не обычным переходом по ссылке.
        /// </summary>
        public static bool IsAjax(this HttpRequest request)
        {
            return string.Equals(request.Headers.XRequestedWith, "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);
        }
    }
}
