using Microsoft.EntityFrameworkCore;
using OnlineShop.Db.Models;

namespace OnlineShop.Db
{
    public class DatabaseContext : DbContext
    {
        public DbSet<Product> Products { get; set; }

        public DbSet<Cart> Carts { get; set; }

        public DbSet<Favorite> Favorites { get; set; }

        public DbSet<Order> Order { get; set; }

        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
        {
            Database.Migrate();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<Product>().HasData(new List<Product>()
            {
                new Product() {Id = Guid.Parse("ef667330-84d7-48ee-908b-5aa72b61114b"),Name = "Товар 1", Cost = 1000, Description = "Описание", PhotoPath = "images/products/12312312" },
                //new Product() {Id = Guid.Parse("cf23df52-568c-44a7-a791-df2ae0befd8a"),Name = "Товар 2", Cost = 2000, Description = "Описание", PhotoPath = "images/products/7cb5829b-bfee-4b5f-9d91-f78c5e1268cb.png" },
                //new Product() {Id = Guid.Parse("0abcf8a8-1f9d-488b-b770-d7eec4a81de8"),Name = "Товар 3", Cost = 3000, Description = "Описание", PhotoPath = "images/products/7cb5829b-bfee-4b5f-9d91-f78c5e1268cb.png" },
                //new Product() {Id = Guid.Parse("5604bb42-7175-49c7-8cb5-ac4b75fdb6bd"),Name = "Товар 4", Cost = 4000, Description = "Описание", PhotoPath = "images/products/7cb5829b-bfee-4b5f-9d91-f78c5e1268cb.png" },
                //new Product() {Id = Guid.Parse("11073797-5c9c-43c5-b681-d3e3330f4043"),Name = "Товар 5", Cost = 5000, Description = "Описание", PhotoPath = "images/products/7cb5829b-bfee-4b5f-9d91-f78c5e1268cb.png" },
                //new Product() {Id = Guid.Parse("ae9d3fe6-0c1e-4bcc-8e9a-c60087fb0026"),Name = "Товар 6", Cost = 6000, Description = "Описание", PhotoPath = "images/products/7cb5829b-bfee-4b5f-9d91-f78c5e1268cb.png" },
            });
        }


    }
}
