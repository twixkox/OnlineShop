using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters.Xml;
using OnlineShop.Db.Interfaces;
using OnlineShop.Db.Models;
using OnlineShopWebApp.Helpers;

namespace OnlineShopWebApp.Controllers
{
    public class CatalogController : Controller
    {
        private readonly IProductStorages _products;
        private readonly ILogger<CatalogController> _logger;

        public CatalogController(IProductStorages products, ILogger<CatalogController> logger)
        {
            _logger = logger;
            _products = products;
        }

        public async Task<IActionResult> AllProducts()
        {
            _logger.LogInformation($"Получение списка всех товаров");
            var products = await _products.GetAllAsync();
            try
            {
                return View(products.ToProductsViewModels());
            }
           catch (Exception ex)
            {
                _logger.LogError(ex, $"Произошла ошибка получения всех товаров");
                return View("Error");
            }            
        }

        public async Task<IActionResult> CurrentCategory(string identityUrl, Guid categoryId)
        {
            _logger.LogInformation($"Получение списка товаров с категорией Id - {categoryId}");
            var productsId = await _products.TryGetProductsByCategoryId(categoryId);
            try
            {
                var productsList = new List<Product>();

                foreach (var product in productsId)
                {
                    var currentProduct = await _products.TryGetProductByIdAsync(product);

                    productsList.Add(currentProduct);
                }

                return View(productsList.ToProductsViewModels());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Произошла ошибка получения списка товаров категории Id - {categoryId}. Category/CurrentCategory");
                return View("Error");
            }
        }
    }
}
