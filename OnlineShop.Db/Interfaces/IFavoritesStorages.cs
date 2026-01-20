using OnlineShop.Db.Models;

namespace OnlineShop.Db.Interfaces
{
    public interface IFavoritesStorages
    {
        Task<Favorite> TryGetByUserIdAsync (string userId);
        Task AddAsync(Product product, string userId);
        Task DeleteAsync(Guid productId, string userID);
    }
}
