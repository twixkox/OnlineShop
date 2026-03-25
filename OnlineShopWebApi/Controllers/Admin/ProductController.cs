using Microsoft.AspNetCore.Mvc;
using OnlineShop.Db.Interfaces;
using OnlineShop.Db.Models;
using OnlineShopWebApp.Areas.Client.Models;

namespace OnlineShopWebApi.Controllers.Admin
{
    [Area("Admin")]
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : Controller
    {
        private readonly IProductStorages _products;
        private readonly ILogger _logger;
        public ProductController(IProductStorages products, ILogger<ProductController> logger)
        {
            _logger = logger;
            _products = products;
        }

        [HttpGet("GetProducts")]
        public async Task<ActionResult<List<Product>>> Index()
        {
            var products = await _products.GetAllAsync();
            try
            {
                _logger.LogInformation($"Получение списка продуктов успешно");

                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка получения списка продуктов");
                return StatusCode(500, "Произошла ошибка получения продуктов");
            }
        }
        [HttpDelete(nameof(Remove))]
        public async Task<IActionResult> Remove(Guid id)
        {
            await _products.DeleteAsync(id);
            try
            {
                _logger.LogInformation($"Удаление товара {id} выполнено");

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка при удалении продукта id - {id}");

                return StatusCode(500, "Произошла ошибка удаления продукта");
            }
        }

        [HttpPost(nameof(Add))]
        public async Task<ActionResult<Product>> Add(Product product)
        {
            var productDb = new Product
            {
                Name = product.Name,
                Description = product.Description,
                Cost = product.Cost,
            };

            await _products.AddAsync(productDb);
            try
            {   
                _logger.LogInformation($"Продукт успешно добавлен. id - {productDb.Id}");

                return Created();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка добавления продукта");

                return StatusCode(500, "Произошла ошибка добавления продукта");
            }
        }
        [HttpGet(nameof(Edit))]
        public async Task<ActionResult<Product>> Edit(Guid id)
        {
            var product = await _products.TryGetProductByIdAsync(id);
            try
            {
                if (product == null)
                {
                    _logger.LogWarning($"Продукт с id - {id} не найден");
                    return NotFound();
                }

                return Ok(product);
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка изменения (получения) продукта id - {id}");

                return StatusCode(500, "Произошла ошибка изменения продукта");
            }
        }

        [HttpPost(nameof(Edit))]
        public async Task<ActionResult<Product>> Edit(ProductViewModel product)
        {
            var productDb = new Product
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Cost = product.Cost,
            };

            await _products.EditProductAsync(productDb);
            try
            {
                _logger.LogInformation($"Изменение продукта с id {product.Id} успешно");

                return Ok(productDb);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, $"Ошибка изменения продукта id - {product.Id}");

                return BadRequest();
            }
        }
    }
}
