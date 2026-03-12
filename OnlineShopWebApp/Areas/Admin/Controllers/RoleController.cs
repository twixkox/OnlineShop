using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Db.Models;
using OnlineShopWebApp.Models;

namespace OnlineShopWebApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class RoleController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<RoleController> _logger;

        public RoleController(RoleManager<IdentityRole> roleManager, UserManager<User> userManager, ILogger<RoleController> logger)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            _logger.LogInformation($"Получение списка ролей");
            var roleViewModels = roles.Select(r => new RoleViewModel
            {
                Id = r.Id,
                Name = r.Name,
                Description = r.Name
                switch
                {
                    "Admin" => "Полный доступ ко всем функциям системы",
                    "Manager" => "Управление товарами и заказами",
                    "User" => "Обычный пользователь",
                    _ => "Роль пользователя"
                }
            }).ToList();

            return View(roleViewModels);
        }

        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(RoleViewModel role)
        {
            if (!ModelState.IsValid) 
            {
                _logger.LogWarning($"Передана невалидная роль. Role/Add");
                return View(role); 
            }
            _logger.LogInformation($"Поиск роли по имени - {role.Name}");
            var roleName = await _roleManager.RoleExistsAsync(role.Name);

            if (roleName)
            {
                _logger.LogWarning($"Попытка создания существующей роли. Role/Add");
                ModelState.AddModelError("", "Данная роль уже существует"); 
            }

            var existingRole = new IdentityRole(role.Name);
            _logger.LogInformation("Создание роли {RoleName}", role.Name);
            var result = await _roleManager.CreateAsync(existingRole);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = $"Роль {role.Name} успешно создана.";
                return RedirectToAction("Index");
            }

            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string roleId)
        {
            _logger.LogInformation($"Поиск роли с Id - {roleId}");
            var existingRole = await _roleManager.FindByIdAsync(roleId);

            if (existingRole == null)
            {
                _logger.LogWarning($"Роль с Id - {roleId}, не найдена");    
                TempData["ErrorMessage"] = $"Роль не найдена";
                return RedirectToAction("Error");
            }

            var usersInRole = await _userManager.GetUsersInRoleAsync(existingRole.Name);
            if (usersInRole.Any())
            {
                TempData["ErrorMessage"] = $"Удаление роли {existingRole.Name} невозможно, {usersInRole.Count} имеют данную роль";
                return RedirectToAction("Index");
            }
            
            var result = await _roleManager.DeleteAsync(existingRole);

            if (result.Succeeded)
            {
                TempData["SuccessMessaage"] = $"Роль {existingRole.Name} удалена";
            }
            else
            {
                TempData["ErrorMessage"] = $"Ошибка удаления роли";
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> AssignRole(string userId)
        {
            _logger.LogInformation($"Получение пользователя Id - {userId}");
            var existingUser = await _userManager.FindByIdAsync(userId);
            if (existingUser == null)
            {
                ModelState.AddModelError("", "Не удалось найти пользователя с таким ID");
                _logger.LogError($"Не удалось найти пользователя с Id - {userId}");
                TempData["WarningMessage"] = $"Не удалось назначить роль пользователю {existingUser}";
                return RedirectToAction("Index", "User");
            }
            _logger.LogInformation($"Получение роли пользователя");
            var userRole = await _userManager.GetRolesAsync(existingUser);
            var currentRole = userRole.FirstOrDefault();

            _logger.LogInformation($"Получение списка всех ролей");
            var roles = await _roleManager.Roles.ToListAsync();
            var availableRoles = roles.Select(r => new RoleViewModel
            {
                Id = r.Id,
                Name = r.Name,
                Description = r.Name switch
                {
                    "Admin" => "Полный доступ ко всем функциям системы",
                    "Manager" => "Управление товарами и заказами",
                    "User" => "Обычный пользователь",
                    _ => "Второстепенная роль"
                }
            }).ToList();

            var model = new AssignRoleViewModel
            {
                UserId = existingUser.Id,
                UserName = existingUser.UserName,
                CurrentRole = currentRole,
                AvailableRoles = availableRoles,
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AssignRole(AssignRoleViewModel model)
        {
            if (ModelState.IsValid) 
            {
                _logger.LogWarning($"Передана невалидная роль. Role/AssignRole");
                return View(model); 
            }
            try
            {
                _logger.LogInformation($"Поиск пользователя с Id - {model.UserId}");
                var user = await _userManager.FindByIdAsync(model.UserId);

                if (user == null)
                {
                    _logger.LogError($"Пользователь с Id - {model.UserId} не найден.Role/AssignRole");
                    ModelState.AddModelError("", "Пользователь с таким Id не найден");
                    return View(model);
                }
                _logger.LogInformation($"Получение роли для назначения");
                var selectionRole = await _roleManager.FindByIdAsync(model.SelectedRoleId);
                if (selectionRole == null)
                {
                    _logger.LogError($"Выбранная роль не найдена. Role/AssignRole");
                    ModelState.AddModelError("", "Роль не найдена");
                    return View(model);
                }
                _logger.LogInformation($"Получение роли пользователя Id - {user.Id}");
                var currentRoles = await _userManager.GetRolesAsync(user);

                if (currentRoles.Any())
                {
                    _logger.LogInformation($"Удаление текущей роли у пользователя Id - {user.Id}");
                    var removeRole = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                    if (!removeRole.Succeeded)
                    {
                        _logger.LogError($"Ошибка удаления роли у пользователя Id - {user.Id}. Role/AssignRole");
                        foreach (var error in removeRole.Errors)
                        {
                            ModelState.AddModelError("", error.Description);
                        }
                        return View(model);
                    }
                }
                _logger.LogInformation($"Назначение пользователю с Id - {user.Id} роли - {selectionRole.Name}");
                var addRole = await _userManager.AddToRoleAsync(user, selectionRole.Name);

                if (addRole.Succeeded)
                {
                    _logger.LogInformation($"Роль пользователя Id - {user.Id} успешно изменена на {selectionRole.Name}");
                    TempData["SuccesMessage"] = $"Пользователю {user.UserName} назначена роль - {selectionRole.Name}";

                    return RedirectToAction("Index", "User");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,$"Произошла ошибка назначения роли пользователя Id - {user.Id}. Имя роли {selectionRole.Name}");
                return View("Error");
            }
            
        }
    }
}

