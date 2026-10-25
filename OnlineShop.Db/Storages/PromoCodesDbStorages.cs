using Microsoft.EntityFrameworkCore;
using OnlineShop.Db.Interfaces;
using OnlineShop.Db.Models;

namespace OnlineShop.Db.Storages
{
    public class PromoCodesDbStorages : IPromoCodeStorages
    {
        private readonly DatabaseContext databaseContext;

        public PromoCodesDbStorages(DatabaseContext databaseContext)
        {
            this.databaseContext = databaseContext;
        }

        public static string Normalize(string code) => code.Trim().ToUpperInvariant();

        public async Task<List<PromoCode>> GetAllAsync()
        {
            return await databaseContext.PromoCodes.AsNoTracking().OrderBy(p => p.Code).ToListAsync();
        }

        public async Task<PromoCode?> TryGetByIdAsync(Guid id)
        {
            return await databaseContext.PromoCodes.FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<PromoCode?> TryGetByCodeAsync(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return null;
            var normalized = Normalize(code);
            return await databaseContext.PromoCodes.AsNoTracking().FirstOrDefaultAsync(p => p.Code == normalized);
        }

        public async Task AddAsync(PromoCode promoCode)
        {
            promoCode.Code = Normalize(promoCode.Code);
            await databaseContext.PromoCodes.AddAsync(promoCode);
            await databaseContext.SaveChangesAsync();
        }

        public async Task UpdateAsync(PromoCode promoCode)
        {
            var existing = await TryGetByIdAsync(promoCode.Id);
            if (existing == null) return;

            existing.Code = Normalize(promoCode.Code);
            existing.DiscountPercent = promoCode.DiscountPercent;
            existing.MinOrderAmount = promoCode.MinOrderAmount;
            existing.ValidUntil = promoCode.ValidUntil;
            existing.IsActive = promoCode.IsActive;
            existing.Description = promoCode.Description;
            await databaseContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var existing = await TryGetByIdAsync(id);
            if (existing == null) return;

            databaseContext.PromoCodes.Remove(existing);
            await databaseContext.SaveChangesAsync();
        }
    }
}
