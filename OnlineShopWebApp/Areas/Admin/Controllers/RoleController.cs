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

            //if (!ModelState.IsValid) { return View(role); }

            var roleName = await _roleManager.RoleExistsAsync(role.Name);

            if (roleName) ModelState.AddModelError("", "Данная роль уже существует");



            var existingRole = new IdentityRole(role.Name);

            var result = await _roleManager.CreateAsync(existingRole);

            if (result.Succeeded)
            {
                _logger.LogInformation("Создание роли {RoleName}", role.Name);
                TempData["SuccessMessage"] = $"Роль {role.Name} успешно создана.";
                return RedirectToAction("Index");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return RedirectToAction(nameof(Index));
        }
        [HttpPost] [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string roleId)
        {
            var existingRole = await _roleManager.FindByIdAsync(roleId);

            if (existingRole == null)
            {
                TempData["ErrorMessage"] = $"Роль не найдена";
                return RedirectToAction("Index");
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
            var existingUser = await _userManager.FindByIdAsync(userId);
            if (existingUser == null)
            {
                ModelState.AddModelError("", "Не удалось найти пользователя с таким ID");
                _logger.LogError($"Не удалось назначить роль пользователю {existingUser}");
                TempData["WarningMessage"] = $"Не удалось назначить роль пользователю {existingUser}";
                return RedirectToAction("Index", "User");
            }

            var userRole = await _userManager.GetRolesAsync(existingUser);
            var currentRole = userRole.FirstOrDefault();

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
            if (ModelState.IsValid) { return View(model); }

            var user = await _userManager.FindByIdAsync(model.UserId);

            if (user == null)
            {
                ModelState.AddModelError("", "Пользователь с таким Id не найден");
                return View(model);
            }

            var selectionRole = await _roleManager.FindByIdAsync(model.SelectedRoleId);
            if (selectionRole == null)
            {
                ModelState.AddModelError("", "Роль не найдена");
                return View(model);
            }

            var currentRoles = await _userManager.GetRolesAsync(user);

            if (currentRoles.Any())
            {
                var removeRole = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                if (!removeRole.Succeeded)
                {
                    foreach (var error in removeRole.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    return View(model);
                }

            }
            var addRole = await _userManager.AddToRoleAsync(user, selectionRole.Name);

            if (addRole.Succeeded)
            {
                TempData["SuccesMessage"] = $"Пользователю {user.UserName} назначена роль - {selectionRole.Name}";

                return RedirectToAction("Index", "User");
            }

            foreach (var error in addRole.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }
    }
}

