using Microsoft.AspNetCore.Authorization;
using OnlineShopWebApp.Logging;
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
    [Authorize(Roles = "Admin")]
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
            _logger.LogDebug("Получение списка пользователей");
            var users = await _userManager.Users.ToListAsync();
            _logger.LogDebug("Получено {UsersCount} пользователей", users.Count);
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
                _logger.LogError(ex, "Произошла ошибка получения пользователей. User/Index");
                return RedirectToAction("Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Detail(string userId)
        {
            _logger.LogDebug("Получение пользователя с Id - {UserId}", userId);
            var user = await _userManager.FindByIdAsync(userId);
            _logger.LogDebug("Получение роли пользователя");
            var role = await _userManager.GetRolesAsync(user);
            try
            {
                var result = user.ToUserViewModel();
                result.Role = role.First();

                return View(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Произошла ошибка при получении пользователя с Id - {UserId}", userId);
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
                _logger.LogWarning("Передана невалидная модель. User/Add");
                return View(user);
            }
            _logger.LogDebug("Проверка пользователя по UserName - {UserName}", LogMask.Email(user.UserName));
            var checkLogin = await _userManager.FindByEmailAsync(user.UserName);

            if (checkLogin != null)
            {
                _logger.LogWarning("Пользователь с логином {UserName} уже существует", LogMask.Email(user.UserName));
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
                _logger.LogInformation("Создание пользователя");
                await _userManager.CreateAsync(existingUser, user.Password);
                _logger.LogInformation("Присвоение роли пользователю");
                await _userManager.AddToRoleAsync(existingUser, "User");

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Произошла ошибка при добавлении пользователя. User/Add");
                return RedirectToAction("Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Update(string userId)
        {
            _logger.LogDebug("Получение пользователя с Id - {UserId} для редактирования", userId);
            var existingUser = await _userManager.FindByIdAsync(userId);
            try
            {
                return View(existingUser.ToUserViewModel());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Произошла ошибка при получении пользователя с Id - {UserId} для редактирования. User/Update", userId);
                return RedirectToAction("Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Update(UserViewModel user)
        {
            _logger.LogDebug("Получение пользователя с Id - {UserId}", user.Id);
            var existingUser = await _userManager.FindByIdAsync(user.Id);
            try
            {
                existingUser.FirstName = user.FirstName;
                existingUser.LastName = user.LastName;
                existingUser.PhoneNumber = user.Phone;

                _logger.LogInformation("Обновление данных пользователя");
                await _userManager.UpdateAsync(existingUser);

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Прооизошла ошибка при обновлении даныых пользователя Id - {UserId}. User/Update", user.Id);
                return RedirectToAction("Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Delete(string userId)
        {
            _logger.LogDebug("Получение пользователя с Id - {UserId}", userId);
            var existingUser = await _userManager.FindByIdAsync(userId);
            _logger.LogInformation("Удаление пользователя с Id - {UserId}", userId);
            await _userManager.DeleteAsync(existingUser);
            try
            {
                _logger.LogInformation("Удаление выполнено");

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Произошла ошибка при удалении пользователя Id - {UserId}. User/Delete", userId);
                return RedirectToAction("Error");
            }
        }
        [HttpGet]
        public async Task<IActionResult> Roles(string userId)
        {
            _logger.LogDebug("Получение пользователя Id - {UserId}", userId);
            var user = await _userManager.FindByIdAsync(userId);
            try
            {
                if (user == null)
                {
                    _logger.LogWarning("Пользователь с Id - {UserId} не найден", userId);
                    TempData["ErrorMessage"] = "Пользователь не найден";
                    return RedirectToAction("Index");
                }
                _logger.LogDebug("Получение списка доступных ролей");
                var userRoles = await _userManager.GetRolesAsync(user);
                ViewBag.UserName = user.UserName;
                ViewBag.UserId = userId;

                return View(userRoles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Произошла ошибка при получении ролей для назначения. User/Roles");
                return RedirectToAction("Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> ChangePassword(string userId)
        {
            _logger.LogDebug("Получение пользователя Id - {UserId}", userId);
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
                _logger.LogError(ex, "Произошла ошибка получения пользователя. User/ChangePassword");
                return RedirectToAction("Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(AdminChangePasswordViewModel user)
        {
            _logger.LogDebug("Получение пользователя с Id - {UserId}", user.UserId);
            var existingUser = await _userManager.FindByIdAsync(user.UserId);
            _logger.LogInformation("Удаление текущего пароля у пользователя");
            var token = await _userManager.RemovePasswordAsync(existingUser);
            _logger.LogInformation("Установка нового пароля");
            var result = await _userManager.AddPasswordAsync(existingUser, user.Password);
            try
            {
                    TempData["SuccessMessage"] = "Пароль успешно изменен";
                    return RedirectToAction("DetailAsync", new { userId = user.UserId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при смене пароля у пользователя {UserId}. User/ChangePassword", user.UserId);
                return RedirectToAction("Error");
            }
        }
    }
}
