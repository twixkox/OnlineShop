using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Db.Interfaces;
using OnlineShop.Db.Models;
using OnlineShopWebApp.Areas.Admin.Models;

namespace OnlineShopWebApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Manager")]
    public class PromoCodeController : Controller
    {
        private readonly IPromoCodeStorages _promoCodes;

        public PromoCodeController(IPromoCodeStorages promoCodes)
        {
            _promoCodes = promoCodes;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _promoCodes.GetAllAsync());
        }

        [HttpGet]
        public IActionResult Add() => View("Edit", new PromoCodeViewModel());

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var promo = await _promoCodes.TryGetByIdAsync(id);
            if (promo == null) return NotFound();

            return View(new PromoCodeViewModel
            {
                Id = promo.Id,
                Code = promo.Code,
                DiscountPercent = promo.DiscountPercent,
                MinOrderAmount = promo.MinOrderAmount,
                ValidUntil = promo.ValidUntil.HasValue ? DateOnly.FromDateTime(promo.ValidUntil.Value) : null,
                IsActive = promo.IsActive,
                Description = promo.Description
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(PromoCodeViewModel model)
        {
            var sameCode = await _promoCodes.TryGetByCodeAsync(model.Code ?? "");
            if (sameCode != null && sameCode.Id != model.Id)
            {
                ModelState.AddModelError(nameof(model.Code), "Такой промокод уже есть");
            }

            if (!ModelState.IsValid) return View("Edit", model);

            var promo = new PromoCode
            {
                Id = model.Id,
                Code = model.Code!,
                DiscountPercent = model.DiscountPercent,
                MinOrderAmount = model.MinOrderAmount,
                ValidUntil = model.ValidUntil?.ToDateTime(TimeOnly.MinValue),
                IsActive = model.IsActive,
                Description = model.Description
            };

            if (model.Id == Guid.Empty) await _promoCodes.AddAsync(promo);
            else await _promoCodes.UpdateAsync(promo);

            TempData["SuccessMessage"] = $"Промокод {promo.Code.ToUpperInvariant()} сохранён";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _promoCodes.DeleteAsync(id);
            TempData["InfoMessage"] = "Промокод удалён";
            return RedirectToAction(nameof(Index));
        }
    }
}
