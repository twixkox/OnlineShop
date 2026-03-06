using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using OnlineShop.Db.Interfaces;
using OnlineShop.Db.Models;
using OnlineShopWebApp.Areas.Admin.Intarfaces;
using OnlineShopWebApp.Helpers;
using OnlineShopWebApp.Models;
using System.Threading.Tasks;

namespace OnlineShopWebApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoryController : Controller
    {
        private readonly ICategoryStorages _category;
        private readonly IProductStorages _products;
        private readonly IFileStorageService _fileProvider;
        //private readonly ILogger _logger;

        public CategoryController(ICategoryStorages category, IProductStorages products, IFileStorageService fileProvider)
        {
            _category = category;
            _products = products;
            _fileProvider = fileProvider;
        }

        public async Task<IActionResult> Index()
        {
            var category = await _category.GetAll();

            return View(category.ToListCategoryViewModels());
        }

        public async Task<IActionResult> Add()
        {
            var category = await _category.GetAll();

            ViewBag.Categories = category;
           
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Add(CategoryViewModel category)
        {
            if (!ModelState.IsValid)
            {
                return View(category);
            }
            if(category.UploadedFile != null)
            {
                var path = await _fileProvider.SaveImageAsync(category.UploadedFile, "category");
                var existingCategory = new Category
                {
                    Name = category.Name,
                    Description = category.Description,
                    IdentityUrl = category.IdentityUrl,
                    PhotoPath = path
                };

                await _category.Add(existingCategory);

                return RedirectToAction("Index");
            }
           
            return RedirectToAction("Index");
        }
        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var existingCategory = await _category.TryGetById(id);
            var category = await _category.GetAll();

            ViewBag.ParrentCategories = category;

            return View(existingCategory.ToCategoryViewModel());
        }
        [HttpPost]
        public async Task<IActionResult> Edit(CategoryViewModel category)
        {
            if (!ModelState.IsValid)
            {
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

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {

            await _category.Delete(id);

            return RedirectToAction("Index");
        }
    }
}
//добавление категории
//редактирование
//удаление
