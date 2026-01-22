using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using OnlineShop.Db.Models;
using OnlineShopWebApp.Areas.Admin.Intarfaces;
using OnlineShopWebApp.Models;
using System.Threading.Tasks;

namespace OnlineShopWebApp.Controllers
{
    public class RegistrationController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IWebHostEnvironment _appEnviroment;
        private readonly ILogger<RegistrationController> _logger;
        private readonly IFileStorageService _fileStorageService;
        public RegistrationController(UserManager<User> userManager, SignInManager<User> signInManager, IWebHostEnvironment appEnviroment, ILogger<RegistrationController> logger, IFileStorageService fileStorageService)
        {
            _fileStorageService = fileStorageService;
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
            _appEnviroment = appEnviroment;
        }

        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Registration(RegistrationUser user)
        {
            if (user.UserName == user.Password) ModelState.AddModelError("", "Логин и пароль должны отличаться");
                       
            if (!ModelState.IsValid) return View("Index", user);


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

            await _userManager.CreateAsync(currentUser,user.Password);

            var addRole = await _userManager.AddToRoleAsync(currentUser, "User");

            await _signInManager.SignInAsync(currentUser,true);

            return RedirectToAction("Index","Home");
        }

        public IActionResult Success()
        {
            return View("Index");
        }
    }
}
