using Microsoft.AspNetCore.Mvc;
using OnlineShop.Db.Interfaces;
using OnlineShop.Db.Models;
using OnlineShopWebApp.Helpers;
using OnlineShopWebApp.Models;
using System.Threading.Tasks;

namespace OnlineShopWebApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoryController : Controller
    {
        private readonly ICategoryStorages _category;
        //private readonly ILogger _logger;

        public CategoryController(ICategoryStorages category) //ILogger logger)
        {
            _category = category;
            //_logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var category = await _category.GetAll();

            return View(category.ToListCategoryViewModels());
        }

        public async Task<IActionResult> Add()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Add(CategoryViewModel category)
        {
            if (!ModelState.IsValid)
            {
                return View(category);
            }
            var existingCategory = new Category
            {
                Name = category.Name,
                Description = category.Description,
            };

            await _category.Add(existingCategory);

            return RedirectToAction("Index");
        }


    }
}
//добавление категории
//редактирование
//удаление
