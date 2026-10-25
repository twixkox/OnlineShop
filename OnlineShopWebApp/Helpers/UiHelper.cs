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

        public const string DefaultAvatar = "/uploads/users/avatars/UserAvatar.png";

        public static string AvatarUrl(string? path)
        {
            if (string.IsNullOrWhiteSpace(path)) return DefaultAvatar;
            var normalized = path.Replace('\\', '/');
            return normalized.StartsWith('/') ? normalized : "/" + normalized;
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

        public static string LightName(OnlineShop.Db.Models.LightRequirement light) => light switch
        {
            OnlineShop.Db.Models.LightRequirement.Sun => "Солнце",
            OnlineShop.Db.Models.LightRequirement.PartShade => "Полутень",
            OnlineShop.Db.Models.LightRequirement.Shade => "Тень",
            _ => light.ToString()
        };

        public static string LightIcon(OnlineShop.Db.Models.LightRequirement light) => light switch
        {
            OnlineShop.Db.Models.LightRequirement.Sun => "bi-sun",
            OnlineShop.Db.Models.LightRequirement.PartShade => "bi-cloud-sun",
            _ => "bi-cloud"
        };

        public static string WateringName(OnlineShop.Db.Models.WateringLevel watering) => watering switch
        {
            OnlineShop.Db.Models.WateringLevel.Low => "Умеренный",
            OnlineShop.Db.Models.WateringLevel.Medium => "Регулярный",
            OnlineShop.Db.Models.WateringLevel.High => "Обильный",
            _ => watering.ToString()
        };

        /// <summary>Варианты фильтра «зимостойкость»: растение выдерживает зиму в этой зоне и холоднее.</summary>
        public static readonly (int Zone, string Label)[] ZoneOptions =
        {
            (2, "Очень зимостойкие (зона 2, до −45 °C)"),
            (3, "Зона 3 и лучше (до −40 °C)"),
            (4, "Зона 4 и лучше (до −34 °C)"),
            (5, "Зона 5 и лучше (до −29 °C)"),
            (6, "Зона 6 и лучше (до −23 °C)"),
        };

        public static string ZoneName(int zone)
        {
            var minTemp = zone switch
            {
                1 => -51, 2 => -45, 3 => -40, 4 => -34, 5 => -29, 6 => -23, 7 => -18, 8 => -12, 9 => -7, _ => -1
            };
            return $"зона {zone} (до {minTemp} °C)".Replace("-", "−");
        }

        /// <summary>Склонение: 1 отзыв, 2 отзыва, 5 отзывов.</summary>
        public static string Plural(int n, string one, string few, string many)
        {
            var n100 = Math.Abs(n) % 100;
            var n10 = n100 % 10;
            if (n100 > 10 && n100 < 20) return many;
            if (n10 > 1 && n10 < 5) return few;
            if (n10 == 1) return one;
            return many;
        }
    }
}
