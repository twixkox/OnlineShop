using OnlineShop.Db.Models;

namespace OnlineShop.Db.Interfaces
{
    public interface IOrderStorages
    {
        Task<List<Order>> GetAllAsync();
        Task AddAsync(Order order);
        Task<Order> TryGetByIdAsync(Guid orderId);
        Task UpdateStatusAsync(Guid orderId, OrderStatus newStatus);
        Task<List<Order>> GetAllOrdersCurrentUser(string userId);
        Task<Order?> TryGetByPaymentIdAsync(string paymentId);
        Task UpdatePaymentAsync(Guid orderId, PaymentStatus status, string? paymentId = null);
    }
}
