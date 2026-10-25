using Microsoft.AspNetCore.Mvc;
using OnlineShop.Db.Interfaces;
using OnlineShopWebApp.Helpers;

namespace OnlineShopWebApp.Controllers
{
    public class CatalogController : Controller
    {
        private readonly IProductStorages _products;
        private readonly ICategoryStorages _categories;
        private readonly ILogger<CatalogController> _logger;

        public CatalogController(IProductStorages products, ICategoryStorages categories, ILogger<CatalogController> logger)
        {
            _logger = logger;
            _products = products;
            _categories = categories;
        }

        public async Task<IActionResult> AllProducts()
        {
            try
            {
                _logger.LogInformation("Получение списка всех товаров");
                var products = await _products.GetAllAsync();
                var categories = await _categories.GetAll();
                ViewBag.Categories = categories;
                return View(products.ToProductsViewModels().WithCategoryNames(categories));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Произошла ошибка получения всех товаров");
                return View("Error");
            }
        }

        public async Task<IActionResult> CurrentCategory(string identityUrl, Guid categoryId)
        {
            try
            {
                _logger.LogInformation("Получение списка товаров с категорией Id - {CategoryId}", categoryId);

                var category = await _categories.TryGetById(categoryId);
                if (category == null)
                {
                    return NotFound();
                }

                // Один запрос вместо отдельного запроса к БД на каждый товар
                var products = await _products.GetByCategoryIdAsync(categoryId);

                ViewBag.CategoryName = category.Name;
                ViewBag.CategoryDescription = category.Description;
                ViewBag.CurrentCategoryId = category.Id;
                ViewBag.Categories = await _categories.GetAll();

                return View(products.ToProductsViewModels());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Произошла ошибка получения списка товаров категории Id - {CategoryId}. Catalog/CurrentCategory", categoryId);
                return View("Error");
            }
        }
    }
}
