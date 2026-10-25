using Microsoft.AspNetCore.Mvc;
using OnlineShop.Db.Interfaces;
using OnlineShopWebApp.Helpers;

namespace OnlineShopWebApp.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductStorages _productStorage;
        private readonly ICategoryStorages _categoryStorages;
        private readonly ILogger<ProductController> _logger;
        public ProductController(IProductStorages productStorage, ICategoryStorages categoryStorages, ILogger<ProductController> logger)
        {
            _productStorage = productStorage;
            _categoryStorages = categoryStorages;
            _logger = logger;
        }

        public async Task<IActionResult> Index(Guid id)
        {
            try
            {
                _logger.LogInformation("Получение товара с Id - {ProductId}", id);
                var product = await _productStorage.TryGetProductByIdAsync(id);

                // Раньше несуществующий товар приводил к NullReferenceException
                if (product == null)
                {
                    Response.StatusCode = StatusCodes.Status404NotFound;
                    return View(model: null);
                }

                var category = await _categoryStorages.TryGetById(product.CategoryId);
                var viewModel = product.ToProductViewModel();
                viewModel.CurrentCategoryName = category?.Name;

                ViewBag.Category = category;
                ViewBag.Related = (await _productStorage.GetByCategoryIdAsync(product.CategoryId))
                    .Where(p => p.Id != product.Id)
                    .Take(4)
                    .ToList()
                    .ToProductsViewModels();

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Произошла ошибка при получении товара. Product/Index");
                return View("Error");
            }
        }
    }
}
