using Microsoft.AspNetCore.Mvc;
using OnlineShop.Db.Interfaces;
using OnlineShopWebApp.Helpers;
using OnlineShopWebApp.Areas.Client.Models;

namespace OnlineShopWebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly IProductStorages _productStorage;
        private readonly ICategoryStorages _categoryStorages;
        private readonly ILogger<HomeController> _logger;
        public HomeController(IProductStorages productStorage, ICategoryStorages categoryStorages, ILogger<HomeController> logger)
        {
            _productStorage = productStorage;
            _categoryStorages = categoryStorages;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            _logger.LogInformation($"Получение списка всех товаров");
            var products = await _productStorage.GetAllAsync();
            _logger.LogInformation($"Получение списка категорий товаров");
            var category = await _categoryStorages.GetAll();
            var viewModel = new HomeViewModel
            {
                FeaturedProducts = products,
                MainCategories = category,
                HeroTitle = "Добро пожаловать в PlantShop!",
                HeroSubtitle = "Свежие растения прямо из питомника",

            };
            try
            {
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Произошла ошибка при получении всех товаров. Home/Index");
                return View("Error");
            }
        }

        public async Task<IActionResult> Search(string query)
        {
            _logger.LogInformation($"Поиск товаров по параметру поиска");
            var products = await _productStorage.SearchAsync(query);
            try
            {
                return View(products.ToProductsViewModels());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Произошла ошибка при поиске товара. Home/Search");
                return View("Error");
            }
        }
    }
}
