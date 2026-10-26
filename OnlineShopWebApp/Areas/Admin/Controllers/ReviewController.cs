using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Db.Interfaces;

namespace OnlineShopWebApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Manager")]
    public class ReviewController : Controller
    {
        private readonly IReviewStorages _reviews;

        public ReviewController(IReviewStorages reviews)
        {
            _reviews = reviews;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _reviews.GetLatestAsync(200));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _reviews.DeleteAsync(id);
            TempData["InfoMessage"] = "Отзыв удалён";
            return RedirectToAction(nameof(Index));
        }
    }
}
