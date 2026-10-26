using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineShopWebApp.Logging;

namespace OnlineShopWebApp.Areas.Admin.Controllers
{
    /// <summary>
    /// Просмотр журнала прямо в админке. Доступен только администраторам:
    /// в журнале есть Id пользователей и IP-адреса.
    /// </summary>
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class JournalController : Controller
    {
        private readonly LogReader _reader;

        public JournalController(LogReader reader)
        {
            _reader = reader;
        }

        public IActionResult Index(LogQuery query)
        {
            var days = _reader.GetDays();

            // По умолчанию — последний день с записями (обычно сегодня)
            if (!query.Date.HasValue && string.IsNullOrWhiteSpace(query.TraceId) && days.Count > 0)
            {
                query.Date = days[0];
            }

            ViewBag.Days = days;
            ViewBag.Files = _reader.GetFiles();
            ViewBag.TotalSize = _reader.TotalSize();
            ViewBag.Query = query;
            ViewBag.IsToday = query.Date == DateOnly.FromDateTime(DateTime.Now);

            return View(_reader.Query(query));
        }

        public IActionResult Download(DateOnly date)
        {
            var file = _reader.OpenDownload(date);
            if (file == null) return NotFound();

            var (stream, name, type) = file.Value;
            return File(stream, type, name);
        }
    }
}
