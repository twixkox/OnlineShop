using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using OnlineShop.Db.Interfaces;
using OnlineShop.Db.Models;
using OnlineShopWebApp.Helpers;
using OnlineShopWebApp.Models;
using System.Security.Claims;
using System.Threading.Tasks;

namespace OnlineShopWebApp.Controllers
{
    public class ProfileController : Controller
    {
        private readonly ILogger<ProfileController> logger;
        private readonly IOrderStorages _orderStorages;
        private readonly UserManager<User> _user;

        public ProfileController(ILogger<ProfileController> logger, IOrderStorages orderStorages, UserManager<User> user)
        {
            this.logger = logger;
            _orderStorages = orderStorages;
            _user = user;
        }
        [HttpGet]
        public async Task<IActionResult> ProfileEdit()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var currentUser = await _user.FindByIdAsync(userId);            

            return View("ProfileEdit",currentUser.ToUserViewModel());
        }

        [HttpPatch]
        public async Task<IActionResult> ProfileEdit(UserViewModel userViewModel)
        {
            var user = new User
            {
                FirstName = userViewModel.FirstName,
                LastName = userViewModel.LastName,
                PhoneNumber = userViewModel.Phone
            };

            await _user.UpdateAsync(user);

            return View(nameof(ProfileEdit));
        }

        public async Task<IActionResult> UserOrder()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var orders = await _orderStorages.GetAllOrdersCurrentUser(userId);

            var ordersViewModel = orders.ToOrdersViewModels();
            return View(ordersViewModel);
        }

        [HttpGet]
        public async Task<IActionResult> EditPassword()
        {
            return View(nameof(EditPassword));
        }

        [HttpPost]
        public async Task<IActionResult> EditPassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var currentUser = await _user.FindByIdAsync(userId);

            var result = await _user.ChangePasswordAsync(currentUser, model.CurrentPassword, model.NewPassword);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Ваш пароль успешно изменен";
                return RedirectToAction("Index", "Home");
            }

            return View(model);
        }
    }
}
