using Microsoft.AspNetCore.Mvc;
using OnlineShop.Db.Interfaces;
using OnlineShopWebApp.Helpers;

namespace ProductController
{
    public class ProductController : Controller
    {
        private readonly IProductStorages _productStorage;

        public ProductController(IProductStorages productStorage)
        {
            _productStorage = productStorage;
        }

        public async Task<IActionResult> Index(Guid id)
        {
            var product = await _productStorage.TryGetProductByIdAsync(id);

            return View(product.ToProductViewModel());    
        }
    }
}