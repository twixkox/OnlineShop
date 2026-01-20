using OnlineShop.Db.Models;

namespace OnlineShop.Db.Interfaces
{
    public interface ICartsStorages
    {
        Task AddAsync(Product product, string userId);

        Task<Cart> TryGetByUserIdAsync(string userId);

        Task ClearAsync(string userId);

        Task SubtractAsync(Guid productId, string UserId);
    }
}
