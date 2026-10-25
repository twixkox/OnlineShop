using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Db.Models;
using OnlineShopWebApp.Areas.Admin.Intarfaces;
using OnlineShop.Db.Interfaces;
using OnlineShopWebApp.Areas.Client.Models;
using OnlineShopWebApp.Services;

namespace OnlineShopWebApp.Controllers
{
    public class RegistrationController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ILogger<RegistrationController> _logger;
        private readonly IFileStorageService _fileStorageService;
        private readonly ICartsStorages _carts;
        private readonly ICartOwnerProvider _cartOwner;
        public RegistrationController(UserManager<User> userManager, SignInManager<User> signInManager, ILogger<RegistrationController> logger,
            IFileStorageService fileStorageService, ICartsStorages carts, ICartOwnerProvider cartOwner)
        {
            _carts = carts;
            _cartOwner = cartOwner;
            _fileStorageService = fileStorageService;
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public IActionResult Index()
        {
            if (User.Identity?.IsAuthenticated == true) return RedirectToAction("Index", "Home");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Registration(RegistrationUser user)
        {
            if (user.UserName == user.Password) ModelState.AddModelError("", "Логин и пароль должны отличаться");

            if (!string.IsNullOrWhiteSpace(user.UserName))
            {
                var existingUser = await _userManager.FindByNameAsync(user.UserName.Trim());

                if (existingUser != null)
                {
                    ModelState.AddModelError("", "Пользователь с таким Email уже существует");
                }
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Попытка передачи невалидной модели для регистрации");
                return View("Index", user);
            }

            try
            {
                var currentUser = new User()
                {
                    Email = user.UserName.Trim(),
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    PhoneNumber = user.Phone,
                    CreationDateTime = DateTime.Now,
                    UserName = user.UserName.Trim(),
                    ProfileImage = _fileStorageService.GetUserPhotoPath()
                };

                _logger.LogInformation("Создание нового пользователя");
                var createResult = await _userManager.CreateAsync(currentUser, user.Password);

                // Раньше результат не проверялся: при ошибке Identity пользователь «входил» в несуществующий аккаунт
                if (!createResult.Succeeded)
                {
                    foreach (var error in createResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View("Index", user);
                }

                _logger.LogInformation("Присвоение роли пользователю");
                await _userManager.AddToRoleAsync(currentUser, Constants.UserRoleName);

                await _signInManager.SignInAsync(currentUser, isPersistent: true);

                // Товары, которые гость успел положить в корзину, остаются у него после регистрации
                var guestId = _cartOwner.GetGuestOwnerId();
                if (guestId != null)
                {
                    await _carts.MergeAsync(guestId, currentUser.Id);
                    _cartOwner.ForgetGuest();
                }

                TempData["SuccessMessage"] = $"Добро пожаловать, {currentUser.FirstName}! Аккаунт создан";
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Произошла ошибка при регистрации нового пользователя. Registration/Registration");
                return View("Error");
            }
        }

        public IActionResult Success()
        {
            return RedirectToAction(nameof(Index));
        }
    }
}
