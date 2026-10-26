using Microsoft.AspNetCore.Identity;
using OnlineShopWebApp.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Db.Models;
using OnlineShopWebApp.Areas.Client.Models;

namespace OnlineShopWebApi.Controllers.Admin
{
    [Area("Admin")]
    [ApiController]
    [Route("api/[controller]")]
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

        [HttpGet("GetRoles")]
        public async Task<ActionResult<List<IdentityRole>>> Index()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            try
            {
                _logger.LogDebug("Получение списка ролей успешно");

                return Ok(roles);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Произошла ошибка при получение ролей");

                return BadRequest();
            }
        }

        [HttpPost("AddRole")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(IdentityRole role)
        {
            var roleName = await _roleManager.RoleExistsAsync(role.Name);

            _logger.LogDebug("Получение роли {RoleName}", role.Name);

            var existingRole = new IdentityRole(role.Name);

            var result = await _roleManager.CreateAsync(existingRole);
            try
            {
                if (result.Succeeded)
                {
                    _logger.LogInformation("Создание роли {RoleName}", role.Name);
                    return Created();
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Ошибка добавления роли {RoleName}", role.Name);

                return BadRequest();
            }

        }
        [HttpDelete(nameof(Delete))]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string roleId)
        {
            var existingRole = await _roleManager.FindByIdAsync(roleId);
            if (existingRole == null)
            {
                _logger.LogInformation("Роль не найдена");

                return Conflict($"Роль не найдена");
            }

            var usersInRole = await _userManager.GetUsersInRoleAsync(existingRole.Name);

            _logger.LogDebug("Проверка наличия у пользоваталей роли - {ExistingRoleName}", existingRole.Name);

            if (usersInRole.Any())
            {
                _logger.LogWarning("Удаление роли {ExistingRoleName} невозможно, {UsersInRoleCount} имеют данную роль", existingRole.Name, usersInRole.Count);

                return Conflict($"Удаление роли {existingRole.Name} невозможно");
            }

            var result = await _roleManager.DeleteAsync(existingRole);
            try
            {
                if (result.Succeeded)
                {
                    _logger.LogInformation("Роль {ExistingRoleName} удалена", existingRole.Name);
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Ошибка удаления роли {RoleId}", roleId);

                return BadRequest();
            }
        }

        [HttpGet(nameof(AssignRole))]
        public async Task<IActionResult> AssignRole(string userId)
        {
            var existingUser = await _userManager.FindByIdAsync(userId);
            if (existingUser == null)
            {
                _logger.LogError("Не удалось найти пользователю с id - {UserId}", userId);

                return BadRequest();
            }

            var userRole = await _userManager.GetRolesAsync(existingUser);

            _logger.LogDebug("Получение роли пользователя {UserId}", userId);

            var currentRole = userRole.FirstOrDefault();

            var roles = await _roleManager.Roles.ToListAsync();
            try
            {
                var availableRoles = roles.Select(r => new RoleViewModel
                {
                    Id = r.Id,
                    Name = r.Name,
                    Description = r.Name switch
                    {
                        "Admin" => "Полный доступ ко всем функциям системы",
                        "Manager" => "Управление товарами и заказами",
                        "User" => "Обычный пользователь",
                        _ => "Роль пользователя"
                    }
                }).ToList();

                var model = new AssignRoleViewModel
                {
                    UserId = existingUser.Id,
                    UserName = existingUser.UserName,
                    CurrentRole = currentRole,
                    AvailableRoles = availableRoles,
                };

                return Ok(model);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Ошибка изменения роли пользователя {UserId}", userId);

                return BadRequest();
            }
        }

        [HttpPost(nameof(AssignRole))]
        public async Task<IActionResult> AssignRole(AssignRoleViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);

            _logger.LogDebug("Поиск пользователя с ID {UserId}", model.UserId);

            if (user == null)
            {
                _logger.LogWarning("Пользователь с таким Id не найден");
                return BadRequest();
            }

            var selectionRole = await _roleManager.FindByIdAsync(model.SelectedRoleId);

            if (selectionRole == null)
            {
                _logger.LogWarning("Роль не найдена");
                return BadRequest();
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            try
            { 
                if (currentRoles.Any())
                {
                    var removeRole = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                    _logger.LogInformation("Удаление роли пользователя {UserId}", user.Id);
                    if (!removeRole.Succeeded)
                    {
                        foreach (var error in removeRole.Errors)
                        {
                            _logger.LogWarning("Ошибка удаления роли у пользователя {UserId}", user.Id);
                        }
                        return BadRequest();
                    }

                }
                var addRole = await _userManager.AddToRoleAsync(user, selectionRole.Name);

                if (addRole.Succeeded)
                {
                    _logger.LogInformation("Пользователю {UserName} назначена роль - {SelectionRoleName}", LogMask.Email(user.UserName), selectionRole.Name);

                    return Ok();
                }

                return Ok(addRole);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Ошибка при назначении роли пользователя {UserId}", model.UserId);

                return BadRequest();
            }
        }
    }
}

