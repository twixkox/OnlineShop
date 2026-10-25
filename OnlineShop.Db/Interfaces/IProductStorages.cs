using OnlineShop.Db.Models;

namespace OnlineShop.Db.Interfaces
{
    public interface IProductStorages
    {
        Task<List<Product>> GetAllAsync();
        Task AddAsync(Product product);
        Task<Product> TryGetProductByIdAsync(Guid productId);
        Task DeleteAsync(Guid id);
        Task EditProductAsync(Product product);
        Task<List<Product>> SearchAsync(string query);
        Task<List<Guid>> TryGetProductsByCategoryId(Guid categoryId);
        Task<List<Product>> GetByCategoryIdAsync(Guid categoryId);

        /// <summary>Каталог с фильтрами, сортировкой и страницами.</summary>
        Task<PagedResult<Product>> QueryAsync(ProductQuery query);

        /// <summary>Минимальная и максимальная цена (для подсказок в фильтре).</summary>
        Task<(decimal Min, decimal Max)> GetPriceRangeAsync(Guid? categoryId = null);

        /// <summary>Списывает остатки. Возвращает названия товаров, которых не хватило (пусто — всё списано).</summary>
        Task<List<string>> TryReserveStockAsync(IEnumerable<(Guid ProductId, int Quantity)> items);
    }
}
