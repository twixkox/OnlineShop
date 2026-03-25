using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Db.Interfaces;

namespace OnlineShopWebApi.Controllers
{
    [Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    [Route("api/[controller]")]
    public class FavoriteController : Controller
    {
        private readonly IProductStorages _productStorages;
        private readonly IFavoritesStorages _favoriteStorages;
        private readonly ILogger<FavoriteController> _logger;

        public FavoriteController(IProductStorages productStorages, IFavoritesStorages favoritesStorages, ILogger<FavoriteController> logger)
        {
            _productStorages = productStorages;
            _favoriteStorages = favoritesStorages;
            _logger = logger;
        }

        [HttpGet("GetFavorite")]
        public async Task<IActionResult> Index(string userId)
        {
            var favorites = _favoriteStorages.TryGetByUserIdAsync(userId);
            _logger.LogInformation($"Получение избранного у пользователя id - {userId}");
            try
            {
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка получения избранного пользователя id - {userId}");

                return StatusCode(500);
            }
        }

        [HttpPost(nameof(Add))]
        public async Task<IActionResult> Add(Guid productId, string userId)
        {
            var product = await _productStorages.TryGetProductByIdAsync(productId);
            _logger.LogInformation($"Получение продукта id - {productId}");

            await _favoriteStorages.AddAsync(product, userId);
            try
            {
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка добавления в избранное =");

                return StatusCode(500);
            }
        }

        [HttpDelete(nameof(Delete))]
        public async Task<IActionResult> Delete(Guid productId, string userId)
        {
            await _favoriteStorages.DeleteAsync(productId, userId);
            _logger.LogInformation($"Удаление продукта из избранного выполнено");
            try
            {
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка удаления из избранного id пользователя - {userId}, id продутка -{productId}");

                return StatusCode(500);
            }
            
        }
    }
}
