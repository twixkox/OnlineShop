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
    }
}
