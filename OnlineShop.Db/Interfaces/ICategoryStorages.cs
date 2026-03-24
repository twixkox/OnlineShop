using OnlineShop.Db.Models;

namespace OnlineShop.Db.Interfaces
{
    public interface ICategoryStorages
    {
        Task Add(Category category);
        Task Delete(string categoryId);
        Task Edit(Category category);
        Task<List<Category>> GetAll();
        Task<Category> TryGetById(Guid categoryId);
    }
}