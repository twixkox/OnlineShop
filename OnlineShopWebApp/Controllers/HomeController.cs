using Microsoft.AspNetCore.Mvc;
using OnlineShop.Db.Interfaces;
using OnlineShopWebApp.Helpers;

namespace OnlineShopWebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly IProductStorages _productStorage;

        public HomeController(IProductStorages productStorage)
        {
            _productStorage = productStorage;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _productStorage.GetAllAsync();

            return View(products.ToProductsViewModels());
        }

        public async Task<IActionResult> Search(string query)
        {
            var products = await _productStorage.SearchAsync(query);
            
            return View(products.ToProductsViewModels());
        }
    }
}
