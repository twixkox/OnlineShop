using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Db.Interfaces;
using OnlineShopWebApp.Helpers;
using OnlineShopWebApp.Models.ViewModels;

namespace OnlineShopWebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly IProductStorages _productStorage;
        private readonly ICategoryStorages _categoryStorages;

        public HomeController(IProductStorages productStorage, ICategoryStorages categoryStorages)
        {
            _productStorage = productStorage;
            _categoryStorages = categoryStorages;
        }

        //public async Task<IActionResult> Index()
        //{ 

        //    var products = await _productStorage.GetAllAsync();

        //    return View(products.ToProductsViewModels());
        //}

        public async Task<IActionResult> Index()
        {
            var products = await _productStorage.GetAllAsync();
            var category = await _categoryStorages.GetAll();
            var viewModel = new HomeViewModel
            {
                FeaturedProducts = products,
                MainCategories = category,
                HeroTitle = "Добро пожаловать в PlantShop!",
                HeroSubtitle = "Свежие растения прямо из питомника",

            };

            return View(viewModel);
        }

        public async Task<IActionResult> Search(string query)
        {
            var products = await _productStorage.SearchAsync(query);
            
            return View(products.ToProductsViewModels());
        }
    }
}
