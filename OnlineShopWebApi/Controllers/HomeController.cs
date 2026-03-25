using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Db.Interfaces;

namespace OnlineShopWebApi.Controllers
{
    [Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    [Route("api/[controller]")]
    public class HomeController : Controller
    {
        private readonly IProductStorages _productStorage;
        private readonly ILogger<HomeController> _logger;

        public HomeController(IProductStorages productStorage, ILogger<HomeController> logger)
        {
            _productStorage = productStorage;
            _logger = logger;
        }

        [HttpGet("GetAllProducts")]
        public async Task<IActionResult> Index()
        {
            var products = await _productStorage.GetAllAsync();
            _logger.LogInformation($"Получение списка продуктов выполнено");
            try
            {
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка получения продуктов");

                return StatusCode(500);
            }
        }

        [HttpGet(nameof(Search))]
        public async Task<IActionResult> Search(string query)
        {
            try
            {
                var products = _productStorage.SearchAsync(query);
                _logger.LogInformation($"Поиск продукта по параметрам выполнено");

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка поиска продукта");

                return StatusCode(500);
            }
        }
    }
}
