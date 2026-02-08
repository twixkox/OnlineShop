using Microsoft.EntityFrameworkCore;
using OnlineShop.Db.Interfaces;
using OnlineShop.Db.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace OnlineShop.Db.Storages
{
    public class CategoryDbStorages : ICategoryStorages
    {
        private readonly DatabaseContext databaseContext;

        public CategoryDbStorages(DatabaseContext databaseContext)
        {
            this.databaseContext = databaseContext;
        }

        public async Task<List<Category>> GetAll()
        {
            return await databaseContext.Categories.ToListAsync();
        }
        public async Task Add(Category category)
        {
            await databaseContext.Categories.AddAsync(category);

            await databaseContext.SaveChangesAsync();
        }

        public async Task Delete(Guid categoryId)
        {
            var existingCategory = await databaseContext.Categories.FirstOrDefaultAsync(c => c.Id == categoryId);

            if (existingCategory != null) { databaseContext.Categories.Remove(existingCategory); }

            await databaseContext.SaveChangesAsync();
        }

        public async Task Edit(Category category)
        {
            var existingCategory = await databaseContext.Categories.FirstOrDefaultAsync(x => x.Id == category.Id);

            if (existingCategory != null)
            {
                existingCategory.Name = category.Name;
                existingCategory.Description = category.Description;
            }

            await databaseContext.SaveChangesAsync();
        }

        public async Task<List<Product>> TryGetProductsByCategoryId(string categoryId)
        {
            return await databaseContext.Products.Where(p => p.CategoryId.Contains(categoryId)).ToListAsync();
        }
    }
}
