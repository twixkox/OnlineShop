using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Db.Models;
using OnlineShopWebApp.Models;
using System.Threading.Tasks;

namespace OnlineShopWebApp.Controllers
{
    public class RegistrationController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        public RegistrationController(UserManager<User> userManager, SignInManager<User> signInManager)
        {
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
                       
            if (!ModelState.IsValid) return View("Index", user);

            var currentUser = new User()
            {
                Email = user.UserName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.Phone,
                CreationDateTime = DateTime.Now,
                UserName = user.UserName,
                
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
