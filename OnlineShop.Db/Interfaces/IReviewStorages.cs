using OnlineShop.Db.Models;

namespace OnlineShop.Db.Interfaces
{
    public interface IReviewStorages
    {
        Task<List<Review>> GetByProductAsync(Guid productId);
        Task<List<Review>> GetLatestAsync(int count);
        Task<Review?> TryGetUserReviewAsync(Guid productId, string userId);

        /// <summary>Добавляет отзыв или обновляет существующий отзыв пользователя о товаре.</summary>
        Task SaveAsync(Review review);
        Task DeleteAsync(Guid reviewId);

        /// <summary>Средняя оценка и количество отзывов для набора товаров.</summary>
        Task<Dictionary<Guid, (double Average, int Count)>> GetRatingsAsync(IEnumerable<Guid> productIds);
    }
}
