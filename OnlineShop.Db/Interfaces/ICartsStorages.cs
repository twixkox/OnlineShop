using OnlineShop.Db.Models;

namespace OnlineShop.Db.Interfaces
{
    public interface ICartsStorages
    {
        /// <summary>Добавляет одну единицу товара. Возвращает false, если на складе больше нет.</summary>
        Task<bool> AddAsync(Product product, string userId);
        Task<Cart> TryGetByUserIdAsync(string userId);
        Task ClearAsync(string userId);
        Task SubtractAsync(Guid productId, string UserId);
        Task RemoveAsync(Guid productId, string userId);

        /// <summary>Переносит гостевую корзину в корзину пользователя после входа.</summary>
        Task MergeAsync(string fromOwnerId, string toUserId);

        Task SetPromoCodeAsync(string userId, string? code, double discountPercent);
    }
}
