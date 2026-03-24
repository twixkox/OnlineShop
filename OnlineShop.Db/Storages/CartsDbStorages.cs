using Microsoft.EntityFrameworkCore;
using OnlineShop.Db.Interfaces;
using OnlineShop.Db.Models;

namespace OnlineShop.Db.Storages
{
    public class CartsDbStorages : ICartsStorages
    {
        private readonly DatabaseContext databaseContext;

        public CartsDbStorages(DatabaseContext databaseContext)
        {
            this.databaseContext = databaseContext;
        }

        public async Task<Cart> TryGetByUserIdAsync(string userId)
        {
            return await databaseContext.Carts
                .Include(x => x.Items)
                .ThenInclude(x => x.Product).
                FirstOrDefaultAsync(x => x.UserId == userId);
        }

        public async Task AddAsync(Product? product, string userId)
        {
            {
                var existingCart = await TryGetByUserIdAsync(userId);

                if (existingCart == null)
                {
                    var newCart = new Cart
                    {
                        UserId = userId,
                    };

                    newCart.Items = new List<CartItem>
                {
                    new CartItem
                    {
                        Quantity = 1,
                        Product = product,
                    }
                };
                    await databaseContext.Carts.AddAsync(newCart);
                }
                else
                {
                    var existingCartItem = existingCart.Items.FirstOrDefault(item => item.Product.Id == product.Id);
                    if (existingCartItem == null)
                    {
                        existingCartItem = new CartItem()
                        {
                            Product = product,
                            Quantity = 1,
                        };
                         existingCart.Items.Add(existingCartItem);
                    }
                    else
                    {
                        existingCartItem.Quantity += 1;
                    }
                }
                await databaseContext.SaveChangesAsync();
            }
        }

        public async Task ClearAsync(string userId)
        {
            var existingCart = databaseContext.Carts.FirstOrDefault(x => x.UserId == userId);

            if (existingCart != null) databaseContext.Remove(existingCart);

            await databaseContext.SaveChangesAsync();
        }

        public async Task SubtractAsync(Guid productId, string UserId)
        {
            var existingCart = await TryGetByUserIdAsync(UserId);

            var existingCartItem =  existingCart.Items?.FirstOrDefault(item => item.Product.Id == productId);

            if (existingCartItem == null) return;

            existingCartItem.Quantity--;

            if (existingCartItem.Quantity == 0) existingCart.Items.Remove(existingCartItem);

            await databaseContext.SaveChangesAsync();
        }
    }
}
