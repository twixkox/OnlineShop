using Microsoft.AspNetCore.Mvc;
using OnlineShop.Db.Interfaces;
using OnlineShopWebApp.Areas.Client.Models;
using OnlineShopWebApp.Helpers;

namespace OnlineShopWebApp.Controllers
{
    public class CatalogController : Controller
    {
        public const int PageSize = 12;

        private readonly IProductStorages _products;
        private readonly ICategoryStorages _categories;
        private readonly IReviewStorages _reviews;
        private readonly ILogger<CatalogController> _logger;

        public CatalogController(IProductStorages products, ICategoryStorages categories, IReviewStorages reviews, ILogger<CatalogController> logger)
        {
            _logger = logger;
            _products = products;
            _categories = categories;
            _reviews = reviews;
        }

        // /catalog
        public async Task<IActionResult> AllProducts(CatalogFilter filter)
        {
            try
            {
                return View("Index", await BuildPageAsync(filter, null));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Произошла ошибка получения каталога");
                return View("Error");
            }
        }

        // /catalog/{slug}
        public async Task<IActionResult> Category(string slug, CatalogFilter filter)
        {
            try
            {
                var category = await _categories.TryGetByIdentityUrlAsync(slug);
                if (category == null) return NotFound();

                return View("Index", await BuildPageAsync(filter, category));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Произошла ошибка получения категории {Slug}", slug);
                return View("Error");
            }
        }

        // Старый адрес /Catalog/CurrentCategory?identityUrl=..&categoryId=.. — перенаправляем на новый
        public async Task<IActionResult> CurrentCategory(string? identityUrl, Guid categoryId)
        {
            var category = await _categories.TryGetById(categoryId);
            var slug = category?.IdentityUrl ?? identityUrl;
            if (string.IsNullOrEmpty(slug)) return NotFound();

            return RedirectToActionPermanent(nameof(Category), new { slug });
        }

        private async Task<CatalogPageViewModel> BuildPageAsync(CatalogFilter filter, OnlineShop.Db.Models.Category? category)
        {
            var categories = await _categories.GetAll();
            var result = await _products.QueryAsync(filter.ToQuery(category?.Id, PageSize));
            var range = await _products.GetPriceRangeAsync(category?.Id);

            var products = result.Items.ToProductsViewModels().WithCategoryNames(categories);
            products.WithRatings(await _reviews.GetRatingsAsync(products.Select(p => p.Id)));

            filter.Page = result.Page;
            return new CatalogPageViewModel
            {
                Products = products,
                Filter = filter,
                Categories = categories,
                CurrentCategory = category,
                TotalCount = result.TotalCount,
                Page = result.Page,
                TotalPages = result.TotalPages,
                PriceMin = range.Min,
                PriceMax = range.Max
            };
        }
    }
}
