namespace OnlineShopWebApp.Helpers
{
    /// <summary>
    /// Небольшие помощники для представлений: адреса картинок, иконки и оттенки категорий.
    /// </summary>
    public static class UiHelper
    {
        public const string PlaceholderImage = "/img/plant-placeholder.svg";

        /// <summary>
        /// Стандартные заглушки «фото не загружено» заменяем на аккуратную иллюстрацию.
        /// </summary>
        public static bool IsPlaceholder(string? path)
        {
            return string.IsNullOrWhiteSpace(path)
                || path.Contains("anyProduct", StringComparison.OrdinalIgnoreCase)
                || path.Contains("anyCategory", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Пути в БД хранятся без ведущего слеша («uploads/...»). Без него картинки ломались
        /// на вложенных адресах (например, /Favorite/Index искал /Favorite/uploads/...).
        /// </summary>
        public static string ImageUrl(string? path)
        {
            if (IsPlaceholder(path)) return PlaceholderImage;

            var normalized = path!.Replace('\\', '/');
            return normalized.StartsWith('/') || normalized.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                ? normalized
                : "/" + normalized;
        }

        /// <summary>
        /// Лучшая картинка товара для карточки: миниатюра, затем оригинал.
        /// </summary>
        public static string ProductImage(string? thumbnail, string? original)
        {
            return !IsPlaceholder(thumbnail) ? ImageUrl(thumbnail) : ImageUrl(original);
        }

        public static string CategoryIcon(string? identityUrl) => identityUrl switch
        {
            "plodovye" => "bi-basket2",
            "listvennye" => "bi-tree",
            "hvoinye" => "bi-tree-fill",
            "odnoletniki" => "bi-flower2",
            "mnogoletniki" => "bi-flower1",
            "rozy" => "bi-flower3",
            "soputstvuyushchie-tovary" => "bi-scissors",
            _ => "bi-flower1"
        };

        private static readonly string[] Tints =
        {
            "152, 42%, 30%",  // лесной
            "24, 52%, 46%",   // терракота
            "168, 38%, 32%",  // хвоя
            "340, 42%, 46%",  // розовый
            "40, 58%, 42%",   // охра
            "200, 34%, 36%",  // сизый
            "96, 30%, 36%",   // оливковый
        };

        /// <summary>
        /// Стабильный цвет-подложка для категории/товара (по Guid), чтобы заглушки различались.
        /// </summary>
        public static string Tint(Guid id) => Tints[(id.GetHashCode() & 0x7fffffff) % Tints.Length];

        public static int Hue(Guid id) => ((id.GetHashCode() & 0x7fffffff) % 7 - 3) * 12;
    }
}
