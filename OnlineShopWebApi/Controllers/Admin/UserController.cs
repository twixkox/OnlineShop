using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Db.Models;
using OnlineShopWebApp.Areas.Client.Models;

namespace OnlineShopWebApi.Controllers.Admin
{
    [Area("Admin")]
    [ApiController]
    [Route("[controller]")]
    public class UserController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<UserController> _logger;


        public UserController(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, ILogger<UserController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }
        [HttpGet("GetAllUsers")]
        public async Task<ActionResult<User>> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            try
            {
                _logger.LogInformation($"Получение списка пользователей");

                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"Ошибка получения списка пользователей");

                return BadRequest();
            }
        }
        [HttpGet("DetailUser")]
        public async Task<IActionResult> Detail(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            try
            {
                _logger.LogInformation($"Поиск пользователя с Id - {userId}");

                if (user == null)
                {
                    _logger.LogWarning($"Пользователь с Id - {userId} не найден");

                    return BadRequest();
                }
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка получения пользователя id - {userId}");

                return BadRequest();
            }
        }


        [HttpPost("AddUser")]
        public async Task<IActionResult> Add(RegistrationUser user)
        {
            var checkLogin = await _userManager.FindByEmailAsync(user.UserName);
            if (checkLogin != null)
            {
                _logger.LogWarning($"Пользователь с логином {user.UserName}уже существует");

                return BadRequest();
            }

            var existingUser = new User()
            {
                Email = user.UserName,
                UserName = user.UserName,
                PhoneNumber = user.Phone,
                FirstName = user.FirstName,
                CreationDateTime = DateTime.Now,
                LastName = user.LastName,
            };

            await _userManager.CreateAsync(existingUser, user.Password);
            try
            {
                _logger.LogInformation($"Пользователь успешно создан");

                return Created();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка при создании пользователя");

                return BadRequest();
            }
        }

        [HttpGet("UpdateUserInfo")]
        public async Task<IActionResult> UpdateAsync(string userId)
        {
            var existingUser = await _userManager.FindByIdAsync(userId);
            try
            {
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"Ошибка получения пользователя {userId}");

                return BadRequest();
            }

        }

        [HttpPut("UpdateUserInfo")]
        public async Task<IActionResult> UpdateAsync(User user)
        {
            await _userManager.UpdateAsync(user);
            try
            {
                _logger.LogInformation($"Обновление пользователя {user.Id} успешно выполнено");

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Ошибка обновления пользователя {user.Id}");

                return StatusCode(500);
            }
        }

        [HttpDelete("DeleteUser")]
        public async Task<IActionResult> DeleteAsync(string userId)
        {
            var existingUser = await _userManager.FindByIdAsync(userId);
            _logger.LogInformation($"Получение пользователя с id - {userId}");

            await _userManager.DeleteAsync(existingUser);
            try
            {
                _logger.LogInformation($"Удаление пользователя id - {userId} выполнено");

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка удаления пользователя id - {userId}");

                return StatusCode(500);
            }

        }
        [HttpGet("GetUserRole")]
        public async Task<IActionResult> Roles(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            _logger.LogInformation($"Получение пользователя с id - {userId}");

            var userRoles = await _userManager.GetRolesAsync(user);
            try
            {
                _logger.LogInformation($"Получение роли пользователя id - {userId}");

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка получения пользователя/роли id - {userId}");

                return StatusCode(500);
            }
        }

        [HttpGet("ChangePasswordUser")]
        public async Task<IActionResult> ChangePasswordAsync(string userId)
        {
            var existingUser = await _userManager.FindByIdAsync(userId);
            _logger.LogInformation($"Получение пользователя id - {userId}");
            try
            {
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка получения пользователя id - {userId}");

                return StatusCode(500);
            }
        }

        [HttpPost("ChangePasswordUser")]
        public async Task<IActionResult> ChangePassword(OnlineShopWebApp.Areas.Admin.Models.AdminChangePasswordViewModel user)
        {
            var existingUser = await _userManager.FindByIdAsync(user.UserId);
            _logger.LogInformation($"Получение пользователя id - {user.UserId}");

            var token = await _userManager.RemovePasswordAsync(existingUser);
            _logger.LogInformation($"Удаление текущего пароля пользователя id - {user.UserId}");

            var result = await _userManager.AddPasswordAsync(existingUser, user.Password);
            _logger.LogInformation($"Назначение нового пароля выполнено");
            try
            {
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка при смене пароля пользователя id - {user.UserId}");

                return StatusCode(500);
            }
        }
    }
}
