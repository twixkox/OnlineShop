using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OnlineShopWebApp.Models;
using OnlineShop.Db.Models;
using Microsoft.AspNetCore.Authorization;
namespace OnlineShopWebApi.Controllers
{
    [Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    [Route("api/[controller]")]
    public class RegistrationController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ILogger<RegistrationController> _logger;
        public RegistrationController(UserManager<User> userManager, SignInManager<User> signInManager, ILogger<RegistrationController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        [HttpPost(nameof(Registration))]
        public async Task<IActionResult> Registration(RegistrationUser user)
        {
            try
            {
                var currentUser = new User()
                {
                    Email = user.UserName,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    PhoneNumber = user.Phone,
                    CreationDateTime = DateTime.Now,
                    UserName = user.UserName,

                };

                await _userManager.CreateAsync(currentUser, user.Password);
                _logger.LogInformation($"Создание пользователя выполнено");

                await _signInManager.SignInAsync(currentUser, true);
                _logger.LogInformation($"Авторизация пользователя выполнена");

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,$"Ошибка создания пользователя {user.UserName}");

                return StatusCode(500);
            }
        }
    }
}
