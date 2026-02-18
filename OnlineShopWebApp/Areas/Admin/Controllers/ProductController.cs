using Microsoft.AspNetCore.Mvc;
using OnlineShop.Db.Interfaces;
using OnlineShop.Db.Models;
using OnlineShopWebApp.Areas.Admin.Intarfaces;
using OnlineShopWebApp.Helpers;
using OnlineShopWebApp.Models;
using OnlineShopWebApp.Models.Category;
using System.Threading.Tasks;

namespace OnlineShopWebApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly IProductStorages _products;
        private readonly IFileStorageService _fileProvider;
        private readonly ILogger<ProductController> _logger;
        private readonly ICategoryStorages _categories;
        public ProductController(IProductStorages products, IFileStorageService fileProvider, ILogger<ProductController> logger, ICategoryStorages categories)
        {
            _fileProvider = fileProvider;
            _products = products;
            _categories = categories;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _products.GetAllAsync();

            return View(products.ToProductsViewModels());
        }

        public async Task<IActionResult> Remove(Guid id)
        {
            await _products.DeleteAsync(id);

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Add()
        {
            var category = await _categories.GetAll();
            var productViewModel = new ProductViewModel
            {
                AvailableCategory = category.ToListCategoryViewModels(),

            };
            return View(productViewModel);
        }
        [HttpPost]
        public async Task<IActionResult> Add(ProductViewModel product)
        {
            //if (!ModelState.IsValid)
            //{
            //    _logger.LogWarning($"Передана невалидная модель");

            //    return View(product);
            //}

            try
            {
                if (product.UploadedFile != null)
                {
                    var path = await _fileProvider.SaveImageAsync(product.UploadedFile);
                    var thumbnailImage = await _fileProvider.GenerateThumbnailImageAsync(path);
                    var category = await _categories.TryGetById(product.CategoryId);

                    var productDb = new Product
                    {
                        Name = product.Name,
                        Description = product.Description,
                        Cost = product.Cost,
                        PhotoPath = path,
                        ThumbnailPath = thumbnailImage,
                        CategoryId = product.CategoryId,
                        CategoryName = category.Name,
                    };

                    await _products.AddAsync(productDb);

                    _logger.LogInformation($"Выполнено добавление продукта {product.Id}");
                }
                return RedirectToAction("Index");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, $"Ошибка добавления продукта {product.Id}");
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            try
            {
                var product = await _products.TryGetProductByIdAsync(id);

                var category = await _categories.TryGetById(product.CategoryId);
                product.CategoryName = category.Name;

                var productViewModel = product.ToProductViewModel();

                var allCategories = await _categories.GetAll();
                productViewModel.AvailableCategory = allCategories.Select(c => new CategoryViewModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,

                }).ToList();

                _logger.LogInformation($"Выполнено получение продукта для редактирования id - {product.Id}.");

            return View(productViewModel);
        }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, $"Ошибка получения продукта для редактирования id - {id}.");

                return RedirectToAction("Index");
    }
}

[HttpPost]
public async Task<IActionResult> Edit(ProductViewModel product)
{
            if (!ModelState.IsValid)
            {
                _logger.LogWarning($"Передана невалидная модель");

                return View(product);
            }
            try
    {
        if (product.UploadedFile != null)
        {
            var path = await _fileProvider.SaveImageAsync(product.UploadedFile);
            var thumbnailImage = await _fileProvider.GenerateThumbnailImageAsync(path);
            var existingCategory = await _categories.TryGetById(product.CategoryId);
                    var productDb = new Product
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Cost = product.Cost,
                PhotoPath = path,
                ThumbnailPath = thumbnailImage,
                CategoryId = product.CategoryId,
                CategoryName = existingCategory.Name,
            };

            await _products.EditProductAsync(productDb);

            _logger.LogInformation($"Выполнено изменение продукта id - {product.Id}.");
        }

        return RedirectToAction("Index");
    }
    catch (Exception ex)
    {
        _logger.LogError(ex.Message, $"Произошла ошибка редактирования товара id - {product.Id}");

        return RedirectToAction("Index");
    }

}
    }
}
