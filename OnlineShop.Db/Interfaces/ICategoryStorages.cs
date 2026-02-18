using OnlineShop.Db.Models;
using System.Globalization;

namespace OnlineShop.Db.Interfaces
{
    public interface ICategoryStorages
    {
        Task Add(Category category);
        Task Delete(string categoryId);
        Task Edit(Category category);
        Task<List<Category>> GetAll();
        //Task<List<Product>> TryGetProductsByCategoryId(string categoryId);
        Task<Category> TryGetById(Guid categoryId);
    }
}