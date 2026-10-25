namespace OnlineShop.Db.Models
{
    public enum ProductSort
    {
        Default = 0,
        PriceAsc = 1,
        PriceDesc = 2,
        Name = 3,
        Newest = 4
    }

    /// <summary>
    /// Параметры серверной фильтрации и постраничного вывода каталога.
    /// </summary>
    public class ProductQuery
    {
        public Guid? CategoryId { get; set; }

        public string? Search { get; set; }

        public decimal? MinPrice { get; set; }

        public decimal? MaxPrice { get; set; }

        public List<LightRequirement> Light { get; set; } = new();

        public List<WateringLevel> Watering { get; set; } = new();

        /// <summary>Показывать растения, которые выдерживают зиму в этой зоне (FrostZone ≤ значения)</summary>
        public int? MaxFrostZone { get; set; }

        public bool InStockOnly { get; set; }

        public ProductSort Sort { get; set; }

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 12;
    }

    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();

        public int TotalCount { get; set; }

        public int Page { get; set; }

        public int PageSize { get; set; }

        public int TotalPages => PageSize <= 0 ? 1 : Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));
    }
}
