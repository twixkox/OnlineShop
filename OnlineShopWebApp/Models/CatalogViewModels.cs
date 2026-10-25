using OnlineShop.Db.Models;

namespace OnlineShopWebApp.Areas.Client.Models
{
    /// <summary>
    /// Параметры фильтра каталога из адресной строки (?q=&min=&max=&light=&water=&zone=&stock=&sort=&page=).
    /// </summary>
    public class CatalogFilter
    {
        public string? Q { get; set; }
        public decimal? Min { get; set; }
        public decimal? Max { get; set; }
        public List<LightRequirement> Light { get; set; } = new();
        public List<WateringLevel> Water { get; set; } = new();
        public int? Zone { get; set; }
        public bool Stock { get; set; }
        public ProductSort Sort { get; set; }
        public int Page { get; set; } = 1;

        public bool HasFilters => !string.IsNullOrWhiteSpace(Q) || Min.HasValue || Max.HasValue || Light.Count > 0 ||
                                  Water.Count > 0 || Zone.HasValue || Stock;

        public int ActiveCount => (string.IsNullOrWhiteSpace(Q) ? 0 : 1) + (Min.HasValue || Max.HasValue ? 1 : 0) +
                                  Light.Count + Water.Count + (Zone.HasValue ? 1 : 0) + (Stock ? 1 : 0);

        public ProductQuery ToQuery(Guid? categoryId, int pageSize) => new()
        {
            CategoryId = categoryId,
            Search = Q,
            MinPrice = Min,
            MaxPrice = Max,
            Light = Light,
            Watering = Water,
            MaxFrostZone = Zone,
            InStockOnly = Stock,
            Sort = Sort,
            Page = Page,
            PageSize = pageSize
        };

        /// <summary>Параметры для ссылок (пагинация, сортировка), без номера страницы.</summary>
        public Dictionary<string, string> ToRouteValues(int? page = null, ProductSort? sort = null)
        {
            var values = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(Q)) values["q"] = Q;
            if (Min.HasValue) values["min"] = Min.Value.ToString("0");
            if (Max.HasValue) values["max"] = Max.Value.ToString("0");
            for (var i = 0; i < Light.Count; i++) values[$"light[{i}]"] = Light[i].ToString();
            for (var i = 0; i < Water.Count; i++) values[$"water[{i}]"] = Water[i].ToString();
            if (Zone.HasValue) values["zone"] = Zone.Value.ToString();
            if (Stock) values["stock"] = "true";
            var s = sort ?? Sort;
            if (s != ProductSort.Default) values["sort"] = s.ToString();
            if (page is > 1) values["page"] = page.Value.ToString();
            return values;
        }
    }

    public class CatalogPageViewModel
    {
        public List<ProductViewModel> Products { get; set; } = new();
        public CatalogFilter Filter { get; set; } = new();
        public List<Category> Categories { get; set; } = new();
        public Category? CurrentCategory { get; set; }
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int TotalPages { get; set; }
        public decimal PriceMin { get; set; }
        public decimal PriceMax { get; set; }
    }
}
