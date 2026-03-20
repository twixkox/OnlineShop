using Microsoft.AspNetCore.Mvc;
using OnlineShop.Db.Interfaces;
using OnlineShop.Db.Models;
using OnlineShopWebApp.Areas.Admin.Intarfaces;
using OnlineShopWebApp.Areas.Client.Models;
using OnlineShopWebApp.Helpers;

namespace OnlineShopWebApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoryController : Controller
    {
        private readonly ICategoryStorages _category;
        private readonly IProductStorages _products;
        private readonly IFileStorageService _fileProvider;
        private readonly ILogger<CategoryController> _logger;

        public CategoryController(ICategoryStorages category, IProductStorages products, IFileStorageService fileProvider, ILogger<CategoryController> logger)
        {
            _category = category;
            _products = products;
            _fileProvider = fileProvider;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Запрос списка всех категорий");
            var category = await _category.GetAll();
            try
            {
                _logger.LogInformation($"Получено {category.Count} категорий");
                return View(category.ToListCategoryViewModels());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка получения списка категорий. Category/Index");
                return View("Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Add()
        {
            _logger.LogInformation("Запрос списка всех категорий для метода Add");
            var category = await _category.GetAll();
            try
            {
                _logger.LogInformation($"Получено {category.Count} категорий для метода Add");
                ViewBag.Categories = category;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка получения списка категорий. Category/Add");
                return View("Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Add(CategoryViewModel category)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Попытка передачи невалидной модели в метод Add");

                return View(category);
            }
            try
            {
                if (category.UploadedFile != null)
                {
                    _logger.LogInformation($"Сохранение изображения для категории {category.Name} для метода Add");
                    var path = await _fileProvider.SaveImageAsync(category.UploadedFile, "category");
                    var existingCategory = new Category
                    {
                        Name = category.Name,
                        Description = category.Description,
                        IdentityUrl = category.IdentityUrl,
                        PhotoPath = path
                    };

                    await _category.Add(existingCategory);
                    _logger.LogInformation($"Категория {existingCategory.Name} успешно добавлена");

                    return RedirectToAction("Index");
                }
                else
                {
                    var existingCategory = new Category
                    {
                        Name = category.Name,
                        Description = category.Description,
                        IdentityUrl = category.IdentityUrl,
                    };

                    await _category.Add(existingCategory);
                    _logger.LogInformation($"Категория {existingCategory.Name} успешно добавлена");

                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка при добавлении категории {category.Name}. Category/Add");
                return View("Error");
            }
        }
        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            _logger.LogInformation($"Получение категории с Id - {id}");
            var existingCategory = await _category.TryGetById(id);
            if (existingCategory == null)
            {
                _logger.LogError($"Категория с Id - {id} не найдена");
                return View("Error");
            }

            _logger.LogInformation("Получение списка всех категорий");
            var category = await _category.GetAll();
            try
            {
                ViewBag.ParrentCategories = category;

                return View(existingCategory.ToCategoryViewModel());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка при загрузке категории для редактирования Id - {id}. Category/Edit");
                return View("Error");
            }
        }
        [HttpPost]
        public async Task<IActionResult> Edit(CategoryViewModel category)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogError("Передана невалидная модель. Category/Edit");
                return View(category);
            }

            var categoryDb = new Category
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                IdentityUrl = category.IdentityUrl,
            };
            await _category.Edit(categoryDb);

            try
            {
                _logger.LogInformation($"Категория с Id - {category.Id} успешно изменена");

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка при обновлении категории с Id = {category.Id}. Category/Edit");
                return View("Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            _logger.LogInformation($"Удаление категории с Id = {id}");
            await _category.Delete(id);
            try
            {
                _logger.LogInformation($"Категория с Id = {id} успешно удалена");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка при удалении категории с Id = {id}. Category/Delete");
                return View("Error");
            }
        }
    }
}
