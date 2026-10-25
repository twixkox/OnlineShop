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
                .ThenInclude(x => x.Product)
                .FirstOrDefaultAsync(x => x.UserId == userId);
        }

        public async Task<bool> AddAsync(Product? product, string userId)
        {
            if (product == null) return false;

            var existingCart = await TryGetByUserIdAsync(userId);

            if (existingCart == null)
            {
                if (product.Stock < 1) return false;

                var newCart = new Cart
                {
                    UserId = userId,
                    Items = new List<CartItem> { new CartItem { Quantity = 1, Product = product } }
                };
                await databaseContext.Carts.AddAsync(newCart);
            }
            else
            {
                var existingCartItem = existingCart.Items.FirstOrDefault(item => item.Product.Id == product.Id);
                var inCart = existingCartItem?.Quantity ?? 0;

                // Нельзя положить в корзину больше, чем есть на складе
                if (inCart + 1 > product.Stock) return false;

                if (existingCartItem == null)
                {
                    existingCart.Items.Add(new CartItem { Product = product, Quantity = 1 });
                }
                else
                {
                    existingCartItem.Quantity += 1;
                }
            }

            await databaseContext.SaveChangesAsync();
            return true;
        }

        public async Task ClearAsync(string userId)
        {
            var existingCart = await databaseContext.Carts.FirstOrDefaultAsync(x => x.UserId == userId);

            if (existingCart != null) databaseContext.Remove(existingCart);

            await databaseContext.SaveChangesAsync();
        }

        public async Task SubtractAsync(Guid productId, string UserId)
        {
            var existingCart = await TryGetByUserIdAsync(UserId);

            var existingCartItem = existingCart?.Items?.FirstOrDefault(item => item.Product.Id == productId);

            if (existingCart == null || existingCartItem == null) return;

            existingCartItem.Quantity--;

            if (existingCartItem.Quantity <= 0) existingCart.Items.Remove(existingCartItem);

            await databaseContext.SaveChangesAsync();
        }

        public async Task RemoveAsync(Guid productId, string userId)
        {
            var existingCart = await TryGetByUserIdAsync(userId);

            var existingCartItem = existingCart?.Items?.FirstOrDefault(item => item.Product.Id == productId);

            if (existingCart == null || existingCartItem == null) return;

            existingCart.Items.Remove(existingCartItem);

            await databaseContext.SaveChangesAsync();
        }

        public async Task MergeAsync(string fromOwnerId, string toUserId)
        {
            if (string.IsNullOrEmpty(fromOwnerId) || fromOwnerId == toUserId) return;

            var guestCart = await TryGetByUserIdAsync(fromOwnerId);
            if (guestCart == null) return;

            var userCart = await TryGetByUserIdAsync(toUserId);

            if (userCart == null)
            {
                // Корзины у пользователя ещё нет — просто «передаём» гостевую
                guestCart.UserId = toUserId;
            }
            else
            {
                foreach (var item in guestCart.Items)
                {
                    var existing = userCart.Items.FirstOrDefault(i => i.Product.Id == item.Product.Id);
                    var limit = Math.Max(0, item.Product.Stock);

                    if (existing == null)
                    {
                        userCart.Items.Add(new CartItem { Product = item.Product, Quantity = Math.Min(item.Quantity, limit) });
                    }
                    else
                    {
                        existing.Quantity = Math.Min(existing.Quantity + item.Quantity, Math.Max(existing.Quantity, limit));
                    }
                }

                userCart.Items.RemoveAll(i => i.Quantity <= 0);
                userCart.PromoCode ??= guestCart.PromoCode;
                if (userCart.Discount == 0) userCart.Discount = guestCart.Discount;

                databaseContext.Carts.Remove(guestCart);
            }

            await databaseContext.SaveChangesAsync();
        }

        public async Task SetPromoCodeAsync(string userId, string? code, double discountPercent)
        {
            var cart = await databaseContext.Carts.FirstOrDefaultAsync(x => x.UserId == userId);
            if (cart == null) return;

            cart.PromoCode = code;
            cart.Discount = discountPercent;
            await databaseContext.SaveChangesAsync();
        }
    }
}
