using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Db.Models;
using OnlineShopWebApp.Areas.Admin.Intarfaces;
using OnlineShopWebApp.Areas.Admin.Models;
using OnlineShopWebApp.Areas.Client.Models;
using OnlineShopWebApp.Helpers;


namespace OnlineShopWebApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class UserController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IWebHostEnvironment _appEnviroment;
        private readonly IFileStorageService _fileProvider;
        private readonly ILogger<UserController> _logger;

        public UserController(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, IWebHostEnvironment appEnviroment, IFileStorageService fileProvider, ILogger<UserController> logger)
        {
            _fileProvider = fileProvider;
            _userManager = userManager;
            _roleManager = roleManager;
            _appEnviroment = appEnviroment;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            _logger.LogInformation($"Получение списка пользователей");
            var users = await _userManager.Users.ToListAsync();
            _logger.LogInformation($"Получено {users.Count} пользователей");
            var result = users.ToListUserViewModels();
            try
            {
                foreach (var user in users)
                {
                    var role = await _userManager.GetRolesAsync(user);

                    if (role != null)
                    {
                        var existingUser = result.FirstOrDefault(x => x.Id == user.Id);
                        existingUser.Role = role.First();
                    }
                }
                return View(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Произошла ошибка получения пользователей. User/Index");
                return RedirectToAction("Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Detail(string userId)
        {
            _logger.LogInformation($"Получение пользователя с Id - {userId}");
            var user = await _userManager.FindByIdAsync(userId);
            _logger.LogInformation($"Получение роли пользователя");
            var role = await _userManager.GetRolesAsync(user);
            try
            {
                var result = user.ToUserViewModel();
                result.Role = role.First();

                return View(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Произошла ошибка при получении пользователя с Id - {userId}");
                return RedirectToAction("Error");
            }
        }

        [HttpGet]
        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Add(RegistrationUser user)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning($"Передана невалидная модель. User/Add");
                return View(user);
            }
            _logger.LogInformation($"Проверка пользователя по UserName - {user.UserName}");
            var checkLogin = await _userManager.FindByEmailAsync(user.UserName);

            if (checkLogin != null)
            {
                _logger.LogWarning($"Пользователь с логином {user.UserName} уже существует");
                ModelState.AddModelError("", "Пользователь с таким логином уже существует");
            }
            try
            {
                var existingUser = new User()
                {
                    Email = user.UserName,
                    UserName = user.UserName,
                    PhoneNumber = user.Phone,
                    FirstName = user.FirstName,
                    CreationDateTime = DateTime.Now,
                    LastName = user.LastName,
                    ProfileImage = _fileProvider.GetUserPhotoPath()
                };
                _logger.LogInformation($"Создание пользователя");
                await _userManager.CreateAsync(existingUser, user.Password);
                _logger.LogInformation($"Присвоение роли пользователю");
                await _userManager.AddToRoleAsync(existingUser, "User");

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Произошла ошибка при добавлении пользователя. User/Add");
                return RedirectToAction("Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Update(string userId)
        {
            _logger.LogInformation($"Получение пользователя с Id - {userId} для редактирования");
            var existingUser = await _userManager.FindByIdAsync(userId);
            try
            {
                return View(existingUser.ToUserViewModel());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Произошла ошибка при получении пользователя с Id - {userId} для редактирования. User/Update");
                return RedirectToAction("Error");
            }

        }

        [HttpPost]
        public async Task<IActionResult> Update(UserViewModel user)
        {
            _logger.LogInformation($"Получение пользователя с Id - {user.Id}");
            var existingUser = await _userManager.FindByIdAsync(user.Id);
            try
            {
                existingUser.FirstName = user.FirstName;
                existingUser.LastName = user.LastName;
                existingUser.PhoneNumber = user.Phone;

                _logger.LogInformation($"Обновление данных пользователя");
                await _userManager.UpdateAsync(existingUser);

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Прооизошла ошибка при обновлении даныых пользователя Id - {user.Id}. User/Update");
                return RedirectToAction("Error");
            }
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(string userId)
        {
            _logger.LogInformation($"Получение пользователя с Id - {userId}");
            var existingUser = await _userManager.FindByIdAsync(userId);
            _logger.LogInformation($"Удаление пользователя с Id - {userId}");
            await _userManager.DeleteAsync(existingUser);
            try
            {
                _logger.LogInformation($"Удаление выполнено");

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Произошла ошибка при удалении пользователя Id - {userId}. User/Delete");
                return RedirectToAction("Error");
            }
        }
        [HttpGet]
        public async Task<IActionResult> Roles(string userId)
        {
            _logger.LogInformation($"Получение пользователя Id - {userId}");
            var user = await _userManager.FindByIdAsync(userId);
            try
            {
                if (user == null)
                {
                    _logger.LogWarning($"Пользователь с Id - {userId} не найден");
                    TempData["ErrorMessage"] = "Пользователь не найден";
                    return RedirectToAction("Index");
                }
                _logger.LogInformation($"Получение списка доступных ролей");
                var userRoles = await _userManager.GetRolesAsync(user);
                ViewBag.UserName = user.UserName;
                ViewBag.UserId = userId;

                return View(userRoles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Произошла ошибка при получении ролей для назначения. User/Roles");
                return RedirectToAction("Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> ChangePassword(string userId)
        {
            _logger.LogInformation($"Получение пользователя Id - {userId}");
            var existingUser = await _userManager.FindByIdAsync(userId);
            try
            {
                var result = new AdminChangePasswordViewModel
                {
                    UserId = userId,
                    Email = existingUser.Email
                };

                return View(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Произошла ошибка получения пользователя. User/ChangePassword");
                return RedirectToAction("Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(AdminChangePasswordViewModel user)
        {
            _logger.LogInformation($"Получение пользователя с Id - {user.UserId}");
            var existingUser = await _userManager.FindByIdAsync(user.UserId);
            _logger.LogInformation($"Удаление текущего пароля у пользователя");
            var token = await _userManager.RemovePasswordAsync(existingUser);
            _logger.LogInformation($"Установка нового пароля");
            var result = await _userManager.AddPasswordAsync(existingUser, user.Password);
            try
            {
                    TempData["SuccessMessage"] = "Пароль успешно изменен";
                    return RedirectToAction("DetailAsync", new { userId = user.UserId });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка при смене пароля у пользователя {user.UserId}. User/ChangePassword");
                return RedirectToAction("Error");
            }
        }
    }
}
