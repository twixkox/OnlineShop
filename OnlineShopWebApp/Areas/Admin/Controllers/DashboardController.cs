using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Db.Interfaces;
using OnlineShop.Db.Models;
using OnlineShopWebApp.Areas.Admin.Models;
using OnlineShopWebApp.Logging;
using OnlineShopWebApp.Services;

namespace OnlineShopWebApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Manager")]
    public class DashboardController : Controller
    {
        private readonly IOrderStorages _orders;
        private readonly IProductStorages _products;
        private readonly IReviewStorages _reviews;
        private readonly UserManager<User> _users;
        private readonly LogReader _logs;

        public DashboardController(IOrderStorages orders, IProductStorages products, IReviewStorages reviews, UserManager<User> users, LogReader logs)
        {
            _logs = logs;
            _orders = orders;
            _products = products;
            _reviews = reviews;
            _users = users;
        }

        public async Task<IActionResult> Index(int days = 30)
        {
            days = days is 7 or 30 or 90 or 365 ? days : 30;
            var from = DateTime.Today.AddDays(-days + 1);

            var allOrders = await _orders.GetAllAsync();
            var active = allOrders.Where(o => o.Status != OrderStatus.Canceled).ToList();
            var period = active.Where(o => o.CreationDateOrder >= from).ToList();
            var previous = active.Where(o => o.CreationDateOrder >= from.AddDays(-days) && o.CreationDateOrder < from).ToList();

            var revenue = period.Sum(OrderNotificationService.Total);
            var prevRevenue = previous.Sum(OrderNotificationService.Total);

            // Выручка по дням (или по месяцам для года)
            var byMonth = days > 90;
            var points = new List<DashboardPoint>();
            if (byMonth)
            {
                for (var m = new DateTime(from.Year, from.Month, 1); m <= DateTime.Today; m = m.AddMonths(1))
                {
                    var monthOrders = period.Where(o => o.CreationDateOrder.Year == m.Year && o.CreationDateOrder.Month == m.Month).ToList();
                    points.Add(new DashboardPoint(m.ToString("MMM"), monthOrders.Sum(OrderNotificationService.Total), monthOrders.Count));
                }
            }
            else
            {
                for (var d = from; d <= DateTime.Today; d = d.AddDays(1))
                {
                    var dayOrders = period.Where(o => o.CreationDateOrder.Date == d).ToList();
                    points.Add(new DashboardPoint(d.ToString("dd.MM"), dayOrders.Sum(OrderNotificationService.Total), dayOrders.Count));
                }
            }

            var topProducts = period
                .SelectMany(o => o.Items)
                .Where(i => i.Product != null)
                .GroupBy(i => i.Product.Id)
                .Select(g => new DashboardTopProduct(g.First().Product.Name, g.Sum(i => i.Quantity), g.Sum(i => i.Quantity * i.Product.Cost)))
                .OrderByDescending(p => p.Quantity)
                .Take(5)
                .ToList();

            var products = await _products.GetAllAsync();

            var model = new DashboardViewModel
            {
                Days = days,
                Revenue = revenue,
                RevenueChange = prevRevenue == 0 ? null : (double)((revenue - prevRevenue) / prevRevenue * 100),
                OrdersCount = period.Count,
                OrdersChange = previous.Count == 0 ? null : (period.Count - previous.Count) * 100.0 / previous.Count,
                AverageCheck = period.Count == 0 ? 0 : revenue / period.Count,
                NewUsers = _users.Users.Count(u => u.CreationDateTime >= from),
                Points = points,
                TopProducts = topProducts,
                StatusCounts = allOrders.GroupBy(o => o.Status).ToDictionary(g => g.Key, g => g.Count()),
                LowStock = products.Where(p => p.Stock <= 3).OrderBy(p => p.Stock).Take(8).ToList(),
                LatestOrders = allOrders.OrderByDescending(o => o.CreationDateOrder).Take(6).ToList(),
                LatestReviews = await _reviews.GetLatestAsync(4),
                ProductsCount = products.Count
            };

            var problems = _logs.CountProblems(TimeSpan.FromHours(24));
            model.ErrorsLastDay = problems.Errors;
            model.WarningsLastDay = problems.Warnings;
            model.LastError = problems.Last;

            return View(model);
        }
    }
}
