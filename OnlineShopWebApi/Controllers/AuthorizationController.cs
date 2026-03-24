using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Db.Models;
using OnlineShopWebApp.Areas.Client.Models;

namespace OnlineShopWebApi.Controllers
{
   [Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
   [ApiController]
   [Route("api/[controller]")]
    public class AuthorizationController : Controller
    {
        private readonly SignInManager<User> _signInManager;
        private readonly ILogger<AuthorizationController> _logger;

        public AuthorizationController(SignInManager<User> signInManager, ILogger<AuthorizationController> logger)
        {
            _signInManager = signInManager;
            _logger = logger;
        }

        [HttpPost("Authorize")]
        public async Task<IActionResult> AuthorizationAsync(AuthorizationUserViewModel user)
        {
            if (user.UserName == user.Password) return Conflict($"Имя и пароль не должны совпадать");

            var result = await _signInManager.PasswordSignInAsync(user.UserName, user.Password, user.RememberMe, false);
            _logger.LogInformation($"Авторизация пользователя - {user.UserName}");
            try
            {
                if (result.Succeeded)
                {
                    return Ok();
                }
                else
                {
                    return Conflict("Неверный логин или  пароль");
                }
               
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка авторизации пользователя {user.UserName}");

                return StatusCode(500);
            }
        }

        [HttpGet(nameof(Logout))]
        public async Task<IActionResult> Logout()
        {
            try
            {
                await _signInManager.SignOutAsync();
                _logger.LogInformation($"Выполнен выход пользователя");
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,$"Ошибка выхода пользователя");
                return StatusCode(500);
            }
        }
    }
}
