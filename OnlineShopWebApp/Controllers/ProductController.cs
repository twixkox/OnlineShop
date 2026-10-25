using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Db.Interfaces;
using OnlineShop.Db.Models;
using OnlineShopWebApp.Areas.Client.Models;
using OnlineShopWebApp.Helpers;
using System.Security.Claims;

namespace OnlineShopWebApp.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductStorages _productStorage;
        private readonly ICategoryStorages _categoryStorages;
        private readonly IReviewStorages _reviews;
        private readonly UserManager<User> _users;
        private readonly ILogger<ProductController> _logger;

        public ProductController(IProductStorages productStorage, ICategoryStorages categoryStorages, IReviewStorages reviews,
            UserManager<User> users, ILogger<ProductController> logger)
        {
            _productStorage = productStorage;
            _categoryStorages = categoryStorages;
            _reviews = reviews;
            _users = users;
            _logger = logger;
        }

        public async Task<IActionResult> Index(Guid id)
        {
            try
            {
                var product = await _productStorage.TryGetProductByIdAsync(id);

                if (product == null)
                {
                    Response.StatusCode = StatusCodes.Status404NotFound;
                    return View(model: null);
                }

                var category = await _categoryStorages.TryGetById(product.CategoryId);
                var viewModel = product.ToProductViewModel();
                viewModel.CurrentCategoryName = category?.Name;

                var reviews = await _reviews.GetByProductAsync(product.Id);
                viewModel.ReviewsCount = reviews.Count;
                viewModel.Rating = reviews.Count > 0 ? reviews.Average(r => r.Rating) : 0;

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                ViewBag.Category = category;
                ViewBag.Reviews = reviews;
                ViewBag.MyReview = userId == null ? null : reviews.FirstOrDefault(r => r.UserId == userId);
                ViewBag.Related = (await _productStorage.GetByCategoryIdAsync(product.CategoryId))
                    .Where(p => p.Id != product.Id)
                    .OrderByDescending(p => p.Stock > 0)
                    .Take(4)
                    .ToList()
                    .ToProductsViewModels();

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Произошла ошибка при получении товара. Product/Index");
                return View("Error");
            }
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Review(ReviewInputModel input)
        {
            if (await _productStorage.TryGetProductByIdAsync(input.ProductId) == null) return NotFound();

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault()?.ErrorMessage ?? "Проверьте отзыв";
                return RedirectToAction(nameof(Index), new { id = input.ProductId });
            }

            var user = await _users.GetUserAsync(User);
            if (user == null) return Challenge();

            var displayName = string.IsNullOrWhiteSpace(user.FirstName)
                ? user.UserName!.Split('@')[0]
                : $"{user.FirstName} {(string.IsNullOrWhiteSpace(user.LastName) ? "" : user.LastName[0] + ".")}".Trim();

            await _reviews.SaveAsync(new Review
            {
                ProductId = input.ProductId,
                UserId = user.Id,
                UserName = displayName,
                Rating = input.Rating,
                Text = string.IsNullOrWhiteSpace(input.Text) ? null : input.Text.Trim()
            });

            TempData["SuccessMessage"] = "Спасибо за отзыв!";
            return Redirect(Url.Action(nameof(Index), new { id = input.ProductId }) + "#reviews");
        }
    }
}
