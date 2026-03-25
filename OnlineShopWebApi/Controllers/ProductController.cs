using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Db.Interfaces;

namespace OnlineShopWebApi.Controllers
{
    [Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : Controller
    {
        private readonly IProductStorages _productStorage;
        private readonly ILogger<ProductController> _logger;

        public ProductController(IProductStorages productStorage, ILogger<ProductController> logger)
        {
            _productStorage = productStorage;
            _logger = logger;
        }

        [HttpGet("GetProduct")]
        public async Task<IActionResult> Index(Guid id)
        {
            var product = await _productStorage.TryGetProductByIdAsync(id);
            _logger.LogInformation($"Получение продукта с id - {id}");
            try
            {
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка получения продукта id - {id}");

                return StatusCode(500);
            }
        }
    }
}