using Microsoft.EntityFrameworkCore;
using OnlineShop.Db.Interfaces;
using OnlineShop.Db.Models;

namespace OnlineShop.Db.Storages
{
    public class OrdersDbStorages : IOrderStorages
    {
        private readonly DatabaseContext databaseContext;

        public OrdersDbStorages(DatabaseContext databaseContext)
        {
            this.databaseContext = databaseContext;
        }

        public async Task AddAsync(Order order)
        {
            await databaseContext.Order.AddAsync(order);
            await databaseContext.SaveChangesAsync();
        }

        public async Task<List<Order>> GetAllAsync()
        {
            return await databaseContext.Order
                .Include(x => x.DeliveryUserInfo)
                .Include(x => x.Items)
                .ThenInclude(x => x.Product)
                .ToListAsync();
        }

        public async Task<List<Order>> GetAllOrdersCurrentUser(string userId)
        {
            return await databaseContext.Order
                .Where(x => x.UserId == userId)
                .Include(x => x.DeliveryUserInfo)
                .Include(x => x.Items)
                .ThenInclude(x => x.Product)
                .ToListAsync();
        }

        public async Task<Order> TryGetByIdAsync(Guid orderId)
        {
            return await databaseContext.Order.Include(x => x.Items)
                .ThenInclude(x => x.Product)
                .Include(x => x.DeliveryUserInfo)
                .FirstOrDefaultAsync(order => order.Id == orderId);
        }

        public async Task UpdateStatusAsync(Guid orderId, OrderStatus newStatus)
        {
            var existingOrder = await TryGetByIdAsync(orderId);
            if (existingOrder != null)
            {
                existingOrder.Status = newStatus;
            }
            await databaseContext.SaveChangesAsync();
        }

        public async Task<Order?> TryGetByPaymentIdAsync(string paymentId)
        {
            return await databaseContext.Order
                .Include(x => x.Items).ThenInclude(x => x.Product)
                .Include(x => x.DeliveryUserInfo)
                .FirstOrDefaultAsync(o => o.PaymentId == paymentId);
        }

        public async Task UpdatePaymentAsync(Guid orderId, PaymentStatus status, string? paymentId = null)
        {
            var order = await databaseContext.Order.FirstOrDefaultAsync(o => o.Id == orderId);
            if (order == null) return;

            order.PaymentStatus = status;
            if (paymentId != null) order.PaymentId = paymentId;
            await databaseContext.SaveChangesAsync();
        }
    }
}
