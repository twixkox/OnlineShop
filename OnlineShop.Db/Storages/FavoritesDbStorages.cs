using Microsoft.EntityFrameworkCore;
using OnlineShop.Db.Interfaces;
using OnlineShop.Db.Models;

namespace OnlineShop.Db.Storages
{
    public class FavoritesDbStorages : IFavoritesStorages
    {
        private readonly DatabaseContext databaseContext;

        public FavoritesDbStorages(DatabaseContext databaseContext)
        {
            this.databaseContext = databaseContext;
        }

        public async Task<Favorite> GetAllAsync(string userId)
        {
            return await databaseContext.Favorites.Include(x => x.Products).FirstOrDefaultAsync(x => x.UserId == userId);
        }

        public async Task AddAsync(Product product, string userId)
        {
            var existingFavorite = await TryGetByUserIdAsync(userId);

            if (existingFavorite == null)
            {
                existingFavorite = new Favorite()
                {
                    UserId = userId,
                    Products = new List<Product>() { product }
                };
                await databaseContext.Favorites.AddAsync(existingFavorite);
            }
            else
            {
                var existFavoriteItem = existingFavorite.Products.FirstOrDefault(x => x.Id == product.Id);

                if (existFavoriteItem == null)
                {
                    existingFavorite.Products.Add(product);
                }
            }
            await databaseContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid productId, string userId)
        {
            var existingFavorite = await TryGetByUserIdAsync(userId);

            var existingProduct = existingFavorite.Products.FirstOrDefault(product => product.Id == productId);

            if (existingProduct != null) { existingFavorite.Products.Remove(existingProduct); }

            await databaseContext.SaveChangesAsync();
        }

        public async Task<Favorite> TryGetByUserIdAsync(string userId)
        {
            return await databaseContext.Favorites.Include(x => x.Products).FirstOrDefaultAsync(x => x.UserId == userId);
        }
    }
}
