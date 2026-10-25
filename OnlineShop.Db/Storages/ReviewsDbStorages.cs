using Microsoft.EntityFrameworkCore;
using OnlineShop.Db.Interfaces;
using OnlineShop.Db.Models;

namespace OnlineShop.Db.Storages
{
    public class ReviewsDbStorages : IReviewStorages
    {
        private readonly DatabaseContext databaseContext;

        public ReviewsDbStorages(DatabaseContext databaseContext)
        {
            this.databaseContext = databaseContext;
        }

        public async Task<List<Review>> GetByProductAsync(Guid productId)
        {
            return await databaseContext.Reviews.AsNoTracking()
                .Where(r => r.ProductId == productId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Review>> GetLatestAsync(int count)
        {
            return await databaseContext.Reviews.AsNoTracking()
                .Include(r => r.Product)
                .OrderByDescending(r => r.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<Review?> TryGetUserReviewAsync(Guid productId, string userId)
        {
            return await databaseContext.Reviews.FirstOrDefaultAsync(r => r.ProductId == productId && r.UserId == userId);
        }

        public async Task SaveAsync(Review review)
        {
            review.Rating = Math.Clamp(review.Rating, 1, 5);
            var existing = await TryGetUserReviewAsync(review.ProductId, review.UserId);

            if (existing == null)
            {
                await databaseContext.Reviews.AddAsync(review);
            }
            else
            {
                existing.Rating = review.Rating;
                existing.Text = review.Text;
                existing.UserName = review.UserName;
                existing.CreatedAt = DateTime.Now;
            }

            await databaseContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid reviewId)
        {
            var review = await databaseContext.Reviews.FirstOrDefaultAsync(r => r.Id == reviewId);
            if (review == null) return;

            databaseContext.Reviews.Remove(review);
            await databaseContext.SaveChangesAsync();
        }

        public async Task<Dictionary<Guid, (double Average, int Count)>> GetRatingsAsync(IEnumerable<Guid> productIds)
        {
            var ids = productIds.Distinct().ToList();
            if (ids.Count == 0) return new();

            var rows = await databaseContext.Reviews.AsNoTracking()
                .Where(r => ids.Contains(r.ProductId))
                .GroupBy(r => r.ProductId)
                .Select(g => new { g.Key, Average = g.Average(r => (double)r.Rating), Count = g.Count() })
                .ToListAsync();

            return rows.ToDictionary(r => r.Key, r => (r.Average, r.Count));
        }
    }
}
