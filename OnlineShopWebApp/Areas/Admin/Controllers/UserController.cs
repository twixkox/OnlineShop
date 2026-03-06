using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Db.Models;
using OnlineShopWebApp.Areas.Admin.Intarfaces;
using OnlineShopWebApp.Areas.Admin.Models;
using OnlineShopWebApp.Helpers;
using OnlineShopWebApp.Models;


namespace OnlineShopWebApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class UserController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IWebHostEnvironment _appEnviroment;
        private readonly IFileStorageService _fileProvider;

        public UserController(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, IWebHostEnvironment appEnviroment, IFileStorageService fileProvider)
        {
            _fileProvider = fileProvider;
            _userManager = userManager;
            _roleManager = roleManager;
            _appEnviroment = appEnviroment;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            
            var result = users.ToListUserViewModels();
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

        public async Task<IActionResult> Detail(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            var role = await _userManager.GetRolesAsync(user);

            var result = user.ToUserViewModel();

            result.Role = role.First();

            return View(result);
        }

        [HttpGet]
        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Add(RegistrationUser user)
        {
            if (!ModelState.IsValid) return View(user);

            var checkLogin = await _userManager.FindByEmailAsync(user.UserName);
            
            if (checkLogin != null)
            {
                ModelState.AddModelError("", "Пользователь с таким логином уже существует");
            }

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

            await _userManager.CreateAsync(existingUser,user.Password);

            await _userManager.AddToRoleAsync(existingUser, "User");

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Update(string userId)
        {
            var existingUser = await _userManager.FindByIdAsync(userId);
           
            return View(existingUser.ToUserViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Update(UserViewModel user)
        {
            

            var existingUser = await _userManager.FindByIdAsync(user.Id);

            existingUser.FirstName = user.FirstName;
            existingUser.LastName = user.LastName;
            existingUser.PhoneNumber = user.Phone;

            await _userManager.UpdateAsync(existingUser);

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Delete(string userId)
        {
            var existingUser = await _userManager.FindByIdAsync(userId);

            await _userManager.DeleteAsync(existingUser);

            return RedirectToAction("Index");
        }
        [HttpGet]
        public async Task<IActionResult> Roles(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Пользователь не найден";
                return RedirectToAction("Index");
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            ViewBag.UserName = user.UserName;
            ViewBag.UserId = userId;

            return View(userRoles);
        }

        [HttpGet]
        public async Task<IActionResult> ChangePassword(string userId)
        {
            var existingUser = await _userManager.FindByIdAsync(userId);

            var result = new AdminChangePasswordViewModel
            {
                UserId = userId,
                Email = existingUser.Email
            };

            return View(result);

        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(AdminChangePasswordViewModel user)
        {
            var existingUser = await _userManager.FindByIdAsync(user.UserId);

            var token = await _userManager.RemovePasswordAsync(existingUser);

            var result = await _userManager.AddPasswordAsync(existingUser, user.Password);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Пароль успешно изменен";
                return RedirectToAction("DetailAsync", new { userId = user.UserId });
            }

            return RedirectToAction("Index");
        }


        
    }
}
