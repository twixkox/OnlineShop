using Microsoft.AspNetCore.Mvc;
using OnlineShop.Db.Interfaces;
using OnlineShopWebApp.Helpers;
using System.Security.Claims;
using System.Threading.Tasks;

namespace OnlineShopWebApp.Controllers
{
    public class ProfileController : Controller
    {
        private readonly ILogger<ProfileController> logger;
        private readonly IOrderStorages _orderStorages;

        public ProfileController(ILogger<ProfileController> logger, IOrderStorages orderStorages)
        {
            this.logger = logger;
            _orderStorages = orderStorages;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> UserOrder()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var orders = await _orderStorages.GetAllOrdersCurrentUser(userId);

            var ordersViewModel = orders.ToOrdersViewModels();
            return View(ordersViewModel);
        }
    }
}
