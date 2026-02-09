using OnlineShop.Db.Models;

namespace OnlineShop.Db.Interfaces
{
    public interface ICategoryStorages
    {
        Task Add(Category category);
        Task Delete(Guid categoryId);
        Task Edit(Category category);
        Task<List<Category>> GetAll();
        Task<List<Product>> TryGetProductsByCategoryId(string categoryId);
        Task<Category> TryGetById(Guid categoryId);
    }
}