using Microsoft.AspNetCore.Mvc;
using OnlineShop.Db.Interfaces;
using OnlineShopWebApp.Helpers;

namespace ProductController
{
    public class ProductController : Controller
    {
        private readonly IProductStorages _productStorage;
        private readonly ILogger<ProductController> _logger;
        public ProductController(IProductStorages productStorage, ILogger<ProductController> logger)
        {
            _productStorage = productStorage;
            _logger = logger;
        }

        public async Task<IActionResult> Index(Guid id)
        {
            _logger.LogInformation($"Получение товара с Id - {id}"); 
            var product = await _productStorage.TryGetProductByIdAsync(id);
            try
            {
                return View(product.ToProductViewModel());
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, $"Произошла ошибка при получении товара. Product/Index");
                return View("Error");
            }
        }
    }
}