using OnlineShop.Db.Models;

namespace OnlineShop.Db.Interfaces
{
    public interface IPromoCodeStorages
    {
        Task<List<PromoCode>> GetAllAsync();
        Task<PromoCode?> TryGetByIdAsync(Guid id);
        Task<PromoCode?> TryGetByCodeAsync(string code);
        Task AddAsync(PromoCode promoCode);
        Task UpdateAsync(PromoCode promoCode);
        Task DeleteAsync(Guid id);
    }
}
