using Microsoft.AspNetCore.Mvc;
using OnlineShop.Db.Interfaces;
using OnlineShopWebApp.Areas.Client.Models;
using OnlineShopWebApp.Helpers;
using System.Diagnostics;

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
            try
            {
                _logger.LogDebug("Получение данных для главной страницы");
                var products = await _productStorage.GetAllAsync();
                var categories = await _categoryStorages.GetAll();

                var viewModel = new HomeViewModel
                {
                    FeaturedProducts = products,
                    MainCategories = categories,
                    HeroTitle = "Сад, который радует круглый год",
                    HeroSubtitle = "Саженцы, цветы и всё для ухода за растениями — прямо из питомника, с доставкой и советом специалиста.",
                    WorkHours = "Ежедневно 10:00 — 19:00",
                    Address = "пос. Сосново, ул. Садовая, 1",
                    ContactPhone = "+7 (812) 925-05-08",
                    ContactEmail = "vopros@rus.garden",
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Произошла ошибка при получении данных главной страницы. Home/Index");
                return View("Error");
            }
        }

        public async Task<IActionResult> Search(string? query)
        {
            try
            {
                _logger.LogDebug("Поиск товаров по запросу {Query}", query);
                var products = await _productStorage.SearchAsync(query);
                var categories = await _categoryStorages.GetAll();
                ViewBag.Query = query;
                return View(products.ToProductsViewModels().WithCategoryNames(categories));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Произошла ошибка при поиске товара. Home/Search");
                return View("Error");
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            // Само исключение уже записано в журнал средой ASP.NET (ExceptionHandlerMiddleware) —
            // раньше оно логировалось здесь второй раз.
            // Код запроса совпадает с TraceId в журнале: по нему администратор найдёт ошибку в «Журнале»
            return View(new ErrorViewModel { RequestId = Activity.Current?.TraceId.ToString() ?? HttpContext.TraceIdentifier });
        }

        // Красивые страницы для 404/403 и прочих кодов ответа
        [Route("Home/Status/{code:int}")]
        public IActionResult Status(int code)
        {
            ViewData["StatusCode"] = code;
            return code == StatusCodes.Status404NotFound ? View("NotFound") : View("Error");
        }
    }
}
