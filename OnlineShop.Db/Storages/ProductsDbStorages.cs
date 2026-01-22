using Microsoft.EntityFrameworkCore;
using OnlineShop.Db.Interfaces;
using OnlineShop.Db.Models;

namespace OnlineShop.Db.Storages
{
    public class ProductsDbStorages : IProductStorages
    {
        private readonly DatabaseContext databaseContext;

        public ProductsDbStorages(DatabaseContext databaseContext)
        {
            this.databaseContext = databaseContext;
        }

        public async Task<List<Product>> GetAllAsync()
        {
            return await databaseContext.Products.ToListAsync();
        }
        public async Task AddAsync(Product product)
        {
            await databaseContext.Products.AddAsync(product);

            await databaseContext.SaveChangesAsync();
        }
        public async Task DeleteAsync(Guid id)
        {
            var existingProduct = await databaseContext.Products.FirstOrDefaultAsync(product => product.Id == id);

            if (existingProduct != null) {databaseContext.Products.Remove(existingProduct); }

            await databaseContext.SaveChangesAsync();
        }
        public async Task<Product> TryGetProductByIdAsync(Guid productId)
        {
            return await databaseContext.Products.Include(x => x.CartItems).FirstOrDefaultAsync(product => product.Id == productId);
        }
        public async Task EditProductAsync(Product product)
        {
            var currentProduct = await databaseContext.Products.FirstOrDefaultAsync(x => x.Id == product.Id);
            if (currentProduct != null)
            {
                currentProduct.Name = product.Name;
                currentProduct.Cost = product.Cost;
                currentProduct.Description = product.Description;
                currentProduct.PhotoPath = product.PhotoPath;
                currentProduct.ThumbnailPath = product.ThumbnailPath;
            }

            await databaseContext.SaveChangesAsync();
        }
        public async Task<List<Product>> SearchAsync(string? query)
        {
            if (query == null) return [];

            return await databaseContext.Products.Where(p => p.Name.Contains(query)).ToListAsync();
        }
    }
}

