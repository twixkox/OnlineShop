using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Db.Interfaces;
using OnlineShop.Db.Models;
using OnlineShopWebApp.Areas.Admin.Intarfaces;
using OnlineShopWebApp.Areas.Client.Models;
using OnlineShopWebApp.Helpers;

namespace OnlineShopWebApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Manager")]
    public class ProductController : Controller
    {
        private const string DefaultPhoto = "uploads/products/original/anyProduct.png";

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
            try
            {
                var products = await _products.GetAllAsync();
                var categories = await _categories.GetAll();
                return View(products.OrderBy(p => p.Name).ToList().ToProductsViewModels().WithCategoryNames(categories));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Произошла ошибка при получении списка товаров в методе Product/Index");
                return View("Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Remove(Guid Id)
        {
            try
            {
                await _products.DeleteAsync(Id);
                _logger.LogInformation("Выполнено удаление товара с Id - {Id}", Id);
                TempData["InfoMessage"] = "Товар удалён";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                // Товар, который уже есть в заказах, удалить нельзя — лучше обнулить остаток
                _logger.LogError(ex, "Произошла ошибка при удалении товара. Id - {Id}", Id);
                TempData["ErrorMessage"] = "Не удалось удалить товар: он уже есть в заказах. Поставьте остаток 0, чтобы снять его с продажи.";
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Add()
        {
            var viewModel = new ProductViewModel();
            await FillCategoriesAsync(viewModel);
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(ProductViewModel product)
        {
            var category = await _categories.TryGetById(product.CategoryId);
            if (category == null) ModelState.AddModelError(nameof(product.CategoryId), "Выберите категорию");

            if (!ModelState.IsValid)
            {
                // Раньше при ошибке валидации список категорий пропадал
                await FillCategoriesAsync(product);
                return View(product);
            }

            try
            {
                var productDb = new Product { CategoryName = category!.Name, PhotoPath = DefaultPhoto, ThumbnailPath = DefaultPhoto };
                Apply(product, productDb);

                if (product.UploadedFile != null)
                {
                    productDb.PhotoPath = await _fileProvider.SaveImageAsync(product.UploadedFile, "product");
                    productDb.ThumbnailPath = await _fileProvider.GenerateThumbnailImageAsync(productDb.PhotoPath);
                }

                await _products.AddAsync(productDb);
                _logger.LogInformation("Добавлен товар {Name}", productDb.Name);
                TempData["SuccessMessage"] = $"Товар «{productDb.Name}» добавлен";
                return RedirectToAction("Index");
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                await FillCategoriesAsync(product);
                return View(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка добавления продукта. Product/Add");
                return View("Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var product = await _products.TryGetProductByIdAsync(id);
            if (product == null) return NotFound();

            var viewModel = product.ToProductViewModel();
            await FillCategoriesAsync(viewModel);
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProductViewModel product)
        {
            var existing = await _products.TryGetProductByIdAsync(product.Id);
            if (existing == null) return NotFound();

            var category = await _categories.TryGetById(product.CategoryId);
            if (category == null) ModelState.AddModelError(nameof(product.CategoryId), "Выберите категорию");

            if (!ModelState.IsValid)
            {
                product.PhotoPath = existing.PhotoPath;
                product.ThumbnailsPhotoPath = existing.ThumbnailPath;
                await FillCategoriesAsync(product);
                return View(product);
            }

            try
            {
                // Раньше без нового файла фото товара затиралось (PhotoPath = null)
                var productDb = new Product
                {
                    Id = existing.Id,
                    PhotoPath = existing.PhotoPath,
                    ThumbnailPath = existing.ThumbnailPath,
                    CategoryName = category!.Name
                };
                Apply(product, productDb);

                if (product.UploadedFile != null)
                {
                    productDb.PhotoPath = await _fileProvider.SaveImageAsync(product.UploadedFile, "product");
                    productDb.ThumbnailPath = await _fileProvider.GenerateThumbnailImageAsync(productDb.PhotoPath);
                }

                await _products.EditProductAsync(productDb);
                TempData["SuccessMessage"] = $"Изменения в «{productDb.Name}» сохранены";
                return RedirectToAction("Index");
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                product.PhotoPath = existing.PhotoPath;
                await FillCategoriesAsync(product);
                return View(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Произошла ошибка редактирования товара id - {Id}. Product/Edit", product.Id);
                return View("Error");
            }
        }

        private static void Apply(ProductViewModel source, Product target)
        {
            target.Name = source.Name.Trim();
            target.Description = source.Description;
            target.Cost = source.Cost;
            target.CategoryId = source.CategoryId;
            target.Stock = source.Stock;
            target.Height = string.IsNullOrWhiteSpace(source.Height) ? null : source.Height.Trim();
            target.FrostZone = source.FrostZone;
            target.Light = source.Light;
            target.Watering = source.Watering;
            target.FloweringPeriod = string.IsNullOrWhiteSpace(source.FloweringPeriod) ? null : source.FloweringPeriod.Trim();
        }

        private async Task FillCategoriesAsync(ProductViewModel viewModel)
        {
            viewModel.AvailableCategory = (await _categories.GetAll()).OrderBy(c => c.Name).ToList().ToListCategoryViewModels();
        }
    }
}
