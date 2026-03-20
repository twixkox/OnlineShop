using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using OnlineShop.Db.Models;
using OnlineShopWebApp.Areas.Admin.Intarfaces;
using OnlineShopWebApp.Areas.Client.Models;
using System.Threading.Tasks;

namespace OnlineShopWebApp.Controllers
{
    public class RegistrationController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ILogger<RegistrationController> _logger;
        private readonly IFileStorageService _fileStorageService;
        public RegistrationController(UserManager<User> userManager, SignInManager<User> signInManager,ILogger<RegistrationController> logger, IFileStorageService fileStorageService)
        {
            _fileStorageService = fileStorageService;
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Registration(RegistrationUser user)
        {
            if (user.UserName == user.Password) ModelState.AddModelError("", "Логин и пароль должны отличаться");
                       
            if (!ModelState.IsValid)
            {
                _logger.LogWarning($"Попытка передачи невалидной модели для регистрации");
                return View("Index",user);
            }

            var currentUser = new User()
            {
                Email = user.UserName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.Phone,
                CreationDateTime = DateTime.Now,
                UserName = user.UserName,
                ProfileImage = _fileStorageService.GetUserPhotoPath()
            };
            _logger.LogInformation($"Создание нового пользователя");
            await _userManager.CreateAsync(currentUser,user.Password);
            _logger.LogInformation($"Присвоение роли пользователю");
            var addRole = await _userManager.AddToRoleAsync(currentUser, "User");
            try
            {
                await _signInManager.SignInAsync(currentUser, true);

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Произошла ошибка при регистрации нового пользователя. Registration/Registration");
                return View("Error");
            }
        }

        public IActionResult Success()
        {
            return View("Index");
        }
    }
}
