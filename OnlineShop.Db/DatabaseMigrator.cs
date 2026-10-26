using Microsoft.EntityFrameworkCore;

namespace OnlineShop.Db
{
    public static class DatabaseMigrator
    {
        /// <summary>
        /// Применяет миграции, а если миграций нет (например, тестовая база SQLite/InMemory) — создаёт схему по модели.
        /// </summary>
        public static async Task MigrateAsync(DbContext context)
        {
            if (context.Database.IsRelational() && context.Database.GetMigrations().Any())
            {
                await context.Database.MigrateAsync();
            }
            else
            {
                await context.Database.EnsureCreatedAsync();
            }
        }
    }
}
