using OnlineShop.Db.Models;

namespace OnlineShopWebApp.Areas.Admin.Models
{
    public record DashboardPoint(string Label, decimal Revenue, int Orders);

    public record DashboardTopProduct(string Name, int Quantity, decimal Revenue);

    public class DashboardViewModel
    {
        public int Days { get; set; }
        public decimal Revenue { get; set; }
        public double? RevenueChange { get; set; }
        public int OrdersCount { get; set; }
        public double? OrdersChange { get; set; }
        public decimal AverageCheck { get; set; }
        public int NewUsers { get; set; }
        public int ProductsCount { get; set; }
        public List<DashboardPoint> Points { get; set; } = new();
        public List<DashboardTopProduct> TopProducts { get; set; } = new();
        public Dictionary<OrderStatus, int> StatusCounts { get; set; } = new();
        public List<Product> LowStock { get; set; } = new();
        public List<Order> LatestOrders { get; set; } = new();
        public List<Review> LatestReviews { get; set; } = new();
        public int ErrorsLastDay { get; set; }
        public int WarningsLastDay { get; set; }
        public OnlineShopWebApp.Logging.LogEntry? LastError { get; set; }
    }
}
