using Microsoft.AspNetCore.Mvc;
using OnlineShop.Db.Interfaces;
using OnlineShop.Db.Models;
using OnlineShopWebApp.Helpers;

namespace OnlineShopWebApp.Controllers
{
    public class CatalogController : Controller
    {
        private readonly IProductStorages _products;

        public CatalogController(IProductStorages products)
        {
            _products = products;
        }

        public async Task<IActionResult> Index(string identityUrl, Guid categoryId)
        {
            var productsId = await _products.TryGetProductsByCategoryId(categoryId);

            var productsList = new List<Product>();

            foreach (var product in productsId)
            {
                var currentProduct = await _products.TryGetProductByIdAsync(product);

                productsList.Add(currentProduct);
            }

            return View(productsList.ToProductsViewModels());
        }

        public async Task<IActionResult> ShowProductsInCurrentCategory(Guid categoryId)
        {
            var productsId = await _products.TryGetProductsByCategoryId(categoryId);

            var productsList = new List<Product>();

            foreach (var product in productsId)
            {
                var currentProduct = await _products.TryGetProductByIdAsync(product);

                productsList.Add(currentProduct);
            }

            return View(productsList.ToProductsViewModels());
        }
    }
}
