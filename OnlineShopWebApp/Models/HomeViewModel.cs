using System.Collections.Generic;
using OnlineShop.Db.Models; // Подключаем пространство имён с моделями Category и Product

namespace OnlineShopWebApp.Models.ViewModels
{
    /// <summary>
    /// Модель представления для главной страницы (Home/Index)
    /// </summary>
    public class HomeViewModel
    {
        /// <summary>
        /// Заголовок Hero-блока (крупный текст вверху страницы)
        /// </summary>
        public string HeroTitle { get; set; }

        /// <summary>
        /// Подзаголовок Hero-блока (более мелкий текст под заголовком)
        /// </summary>
        public string HeroSubtitle { get; set; }

        /// <summary>
        /// Список основных категорий для отображения на главной (например, в виде плиток)
        /// </summary>
        public List<Category> MainCategories { get; set; }

        /// <summary>
        /// Список популярных товаров (например, хиты продаж или рекомендуемые)
        /// </summary>
        public List<Product> FeaturedProducts { get; set; }

        // Дополнительные списки товаров (опционально, можно добавить при необходимости)
        // public List<Product> NewProducts { get; set; }
        // public List<Product> SaleProducts { get; set; }

        /// <summary>
        /// Режим работы магазина (строка для отображения)
        /// </summary>
        public string WorkHours { get; set; }

        /// <summary>
        /// Адрес магазина / питомника
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// Контактный телефон
        /// </summary>
        public string ContactPhone { get; set; }

        /// <summary>
        /// Контактный email
        /// </summary>
        public string ContactEmail { get; set; }

        // Можно добавить другие поля по мере необходимости:
        // public string VideoBlockTitle { get; set; }
        // public bool HasParking { get; set; }
        // и т.д.
    }
}