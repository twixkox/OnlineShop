using Microsoft.AspNetCore.Mvc;
using OnlineShop.Db.Interfaces;
using OnlineShop.Db.Models;
using OnlineShopWebApp.Helpers;

namespace OnlineShopWebApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly IProductStorages _products;
        private readonly IWebHostEnvironment _appEnvironment;
        private readonly ILogger<ProductController> _logger;
        public ProductController(IProductStorages products, IWebHostEnvironment appEnvironment, ILogger<ProductController> logger)
        {

            _products = products;
            _appEnvironment = appEnvironment;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _products.GetAllAsync();

            return View(products.ToProductsViewModels());
        }

        public async Task<IActionResult> Remove(Guid id)
        {
            await _products.DeleteAsync(id);

            return RedirectToAction("Index");
        }

        public IActionResult Add()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Add(ProductViewModel product)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning($"Передана невалидная модель");

                return View(product);
            }

            try
            {
                if (product.UploadedFile != null)
                {
                    string productPhotoPath = Path.Combine(_appEnvironment.WebRootPath + "/images/products/");

                    if (!Directory.Exists(productPhotoPath))
                    {
                        Directory.CreateDirectory(productPhotoPath);
                    }

                    var fileName = Guid.NewGuid() + "." + product.UploadedFile.FileName.Split('.').Last();
                    using (var fileStream = new FileStream(productPhotoPath + fileName, FileMode.Create))
                    {
                        product.UploadedFile.CopyTo(fileStream);
                    }

                    var productDb = new Product
                    {
                        Name = product.Name,
                        Description = product.Description,
                        Cost = product.Cost,
                        PhotoPath = "/images/products/" + fileName
                    };
                  
                    await _products.AddAsync(productDb);

                    _logger.LogInformation($"Выполнено добавление продукта {product.Id}");
                }
                return RedirectToAction("Index");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, $"Ошибка добавления продукта {product.Id}");
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            try
            {
                var product = await _products.TryGetProductByIdAsync(id);

                _logger.LogInformation($"Выполнено получение продукта для редактирования id - {product.Id}.");

                return View(product);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex.Message, $"Ошибка получения продукта для редактирования id - {id}.");

                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(ProductViewModel product)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning($"Передана невалидная модель");

                return View(product);
            }
            try
            {
                if (product.UploadedFile != null)
                {
                    string productPhotoPath = Path.Combine(_appEnvironment.WebRootPath + "/images/products/");
                    var fileName = Guid.NewGuid() + "." + product.UploadedFile.FileName.Split('.').Last();
                    using (var fileStream = new FileStream(productPhotoPath + fileName, FileMode.Create))
                    {
                        product.UploadedFile.CopyTo(fileStream);
                    }

                    var productDb = new Product
                    {
                        Id = product.Id,
                        Name = product.Name,
                        Description = product.Description,
                        Cost = product.Cost,
                        PhotoPath = "/images/products/" + fileName,
                    };

                    await _products.EditProductAsync(productDb);

                    _logger.LogInformation($"Выполнено изменение продукта id - {product.Id}.");
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, $"Произошла ошибка редактирования товара id - {product.Id}");

                return RedirectToAction("Index");
            }
            
        }
    }
}
