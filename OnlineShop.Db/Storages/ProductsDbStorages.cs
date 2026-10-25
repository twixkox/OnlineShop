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
            if (existingProduct != null) { databaseContext.Products.Remove(existingProduct); }

            await databaseContext.SaveChangesAsync();
        }
        public async Task<Product> TryGetProductByIdAsync(Guid productId)
        {
            return await databaseContext.Products.Include(x => x.CartItems)
                .FirstOrDefaultAsync(product => product.Id == productId);
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
                currentProduct.CategoryId = product.CategoryId;
                currentProduct.CategoryName = product.CategoryName;
                currentProduct.Stock = product.Stock;
                currentProduct.Height = product.Height;
                currentProduct.FrostZone = product.FrostZone;
                currentProduct.Light = product.Light;
                currentProduct.Watering = product.Watering;
                currentProduct.FloweringPeriod = product.FloweringPeriod;
            }

            await databaseContext.SaveChangesAsync();
        }
        public async Task<List<Product>> SearchAsync(string? query)
        {
            if (string.IsNullOrWhiteSpace(query)) return [];

            query = query.Trim();

            return await databaseContext.Products
                .Where(p => p.Name.Contains(query) || p.Description.Contains(query))
                .ToListAsync();
        }

        public async Task<List<Product>> GetByCategoryIdAsync(Guid categoryId)
        {
            return await databaseContext.Products
                .Where(p => p.CategoryId == categoryId)
                .ToListAsync();
        }

        public async Task<List<Guid>> TryGetProductsByCategoryId(Guid categoryId)
        {
            return await databaseContext.Products
                .Where(c => c.CategoryId == categoryId)
                .Select(p => p.Id)
                .ToListAsync();
        }

        public async Task<PagedResult<Product>> QueryAsync(ProductQuery query)
        {
            var q = databaseContext.Products.AsNoTracking().AsQueryable();

            if (query.CategoryId.HasValue) q = q.Where(p => p.CategoryId == query.CategoryId);

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var text = query.Search.Trim();
                q = q.Where(p => p.Name.Contains(text) || p.Description.Contains(text));
            }

            if (query.MinPrice.HasValue) q = q.Where(p => p.Cost >= query.MinPrice);
            if (query.MaxPrice.HasValue) q = q.Where(p => p.Cost <= query.MaxPrice);
            if (query.Light.Count > 0) q = q.Where(p => p.Light != null && query.Light.Contains(p.Light.Value));
            if (query.Watering.Count > 0) q = q.Where(p => p.Watering != null && query.Watering.Contains(p.Watering.Value));
            if (query.MaxFrostZone.HasValue) q = q.Where(p => p.FrostZone != null && p.FrostZone <= query.MaxFrostZone);
            if (query.InStockOnly) q = q.Where(p => p.Stock > 0);

            q = query.Sort switch
            {
                ProductSort.PriceAsc => q.OrderBy(p => p.Cost).ThenBy(p => p.Name),
                ProductSort.PriceDesc => q.OrderByDescending(p => p.Cost).ThenBy(p => p.Name),
                ProductSort.Name => q.OrderBy(p => p.Name),
                ProductSort.Newest => q.OrderByDescending(p => p.Id),
                // По умолчанию: сначала товары в наличии
                _ => q.OrderByDescending(p => p.Stock > 0).ThenBy(p => p.Name)
            };

            var pageSize = Math.Clamp(query.PageSize, 1, 100);
            var total = await q.CountAsync();
            var totalPages = Math.Max(1, (int)Math.Ceiling(total / (double)pageSize));
            var page = Math.Clamp(query.Page, 1, totalPages);

            var items = await q.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PagedResult<Product> { Items = items, TotalCount = total, Page = page, PageSize = pageSize };
        }

        public async Task<(decimal Min, decimal Max)> GetPriceRangeAsync(Guid? categoryId = null)
        {
            var q = databaseContext.Products.AsNoTracking().AsQueryable();
            if (categoryId.HasValue) q = q.Where(p => p.CategoryId == categoryId);

            var prices = await q.Select(p => p.Cost).ToListAsync();
            return prices.Count == 0 ? (0, 0) : (prices.Min(), prices.Max());
        }

        public async Task<List<string>> TryReserveStockAsync(IEnumerable<(Guid ProductId, int Quantity)> items)
        {
            var list = items.ToList();
            var ids = list.Select(i => i.ProductId).ToList();
            var products = await databaseContext.Products.Where(p => ids.Contains(p.Id)).ToListAsync();

            var missing = new List<string>();
            foreach (var (productId, quantity) in list)
            {
                var product = products.FirstOrDefault(p => p.Id == productId);
                if (product == null || product.Stock < quantity) missing.Add(product?.Name ?? productId.ToString());
            }

            if (missing.Count > 0) return missing;

            foreach (var (productId, quantity) in list)
            {
                products.First(p => p.Id == productId).Stock -= quantity;
            }

            await databaseContext.SaveChangesAsync();
            return missing;
        }
    }
}

