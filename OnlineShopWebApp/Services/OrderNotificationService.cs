using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using OnlineShop.Db.Models;
using System.Globalization;
using System.Net;
using System.Text;

namespace OnlineShopWebApp.Services
{
    /// <summary>
    /// Письма о заказах: покупателю и менеджеру.
    /// </summary>
    public class OrderNotificationService
    {
        private static readonly CultureInfo Ru = CultureInfo.GetCultureInfo("ru-RU");

        private readonly IEmailService _email;
        private readonly UserManager<User> _users;
        private readonly SmtpOptions _options;

        public OrderNotificationService(IEmailService email, UserManager<User> users, IOptions<SmtpOptions> options)
        {
            _email = email;
            _users = users;
            _options = options.Value;
        }

        public static string StatusName(OrderStatus status) => status switch
        {
            OrderStatus.Created => "Создан",
            OrderStatus.Processed => "В обработке",
            OrderStatus.Delivering => "В пути",
            OrderStatus.Delivered => "Доставлен",
            OrderStatus.Canceled => "Отменён",
            _ => status.ToString()
        };

        public static string Number(Order order) => order.Id.ToString()[..8].ToUpperInvariant();

        public static decimal Subtotal(Order order) => order.Items?.Sum(i => (i.Product?.Cost ?? 0) * i.Quantity) ?? 0;

        public static decimal Total(Order order) => PromoCodeService.ApplyDiscount(Subtotal(order), order.DiscountPercent);

        public async Task OrderCreatedAsync(Order order, string orderUrl)
        {
            var user = await _users.FindByIdAsync(order.UserId);
            var items = ItemsTable(order);

            if (user?.Email != null)
            {
                await _email.SendAsync(user.Email, $"Заказ № {Number(order)} принят — PlantShop", Layout(
                    $"Спасибо за заказ, {Enc(order.DeliveryUserInfo?.UserName ?? user.FirstName)}!",
                    $"<p>Мы получили заказ <b>№ {Number(order)}</b> и скоро позвоним, чтобы подтвердить доставку " +
                    $"на <b>{order.DeliveryUserInfo?.DeliveryDate.ToString("d MMMM yyyy", Ru)}</b>.</p>{items}" +
                    Button(orderUrl, "Открыть заказ")));
            }

            if (!string.IsNullOrWhiteSpace(_options.ManagerEmail))
            {
                var d = order.DeliveryUserInfo;
                await _email.SendAsync(_options.ManagerEmail, $"Новый заказ № {Number(order)} на {Total(order).ToString("C0", Ru)}", Layout(
                    $"Новый заказ № {Number(order)}",
                    $"<p><b>{Enc(d?.UserName)}</b>, {Enc(d?.Phone)}<br>{Enc(d?.Adress)}, {Enc(d?.Apartment)}<br>" +
                    $"Доставка: {d?.DeliveryDate.ToString("d MMMM yyyy", Ru)}<br>" +
                    $"Оплата: {(order.PaymentMethod == PaymentMethod.Online ? "онлайн" : "при получении")}" +
                    (string.IsNullOrEmpty(d?.Comment) ? "" : $"<br>Комментарий: {Enc(d.Comment)}") + $"</p>{items}"));
            }
        }

        public async Task StatusChangedAsync(Order order, string orderUrl)
        {
            var user = await _users.FindByIdAsync(order.UserId);
            if (user?.Email == null) return;

            await _email.SendAsync(user.Email, $"Заказ № {Number(order)}: {StatusName(order.Status).ToLower(Ru)}", Layout(
                $"Статус заказа № {Number(order)} изменился",
                $"<p>Новый статус: <b>{StatusName(order.Status)}</b>.</p>" + Button(orderUrl, "Посмотреть заказ")));
        }

        public Task PasswordResetAsync(string email, string resetUrl)
        {
            return _email.SendAsync(email, "Восстановление пароля — PlantShop", Layout(
                "Восстановление пароля",
                "<p>Чтобы задать новый пароль, нажмите на кнопку ниже. Если вы не запрашивали восстановление — просто проигнорируйте это письмо.</p>" +
                Button(resetUrl, "Задать новый пароль")));
        }

        private static string ItemsTable(Order order)
        {
            var sb = new StringBuilder("<table style=\"width:100%;border-collapse:collapse;margin:16px 0\">");
            foreach (var item in order.Items ?? new())
            {
                sb.Append($"<tr><td style=\"padding:6px 0;border-bottom:1px solid #eee\">{Enc(item.Product?.Name)} × {item.Quantity}</td>" +
                          $"<td style=\"padding:6px 0;border-bottom:1px solid #eee;text-align:right\">{((item.Product?.Cost ?? 0) * item.Quantity).ToString("C0", Ru)}</td></tr>");
            }
            if (order.DiscountPercent > 0)
            {
                sb.Append($"<tr><td style=\"padding:6px 0\">Скидка ({Enc(order.PromoCode)}, {order.DiscountPercent}%)</td>" +
                          $"<td style=\"padding:6px 0;text-align:right\">−{(Subtotal(order) - Total(order)).ToString("C0", Ru)}</td></tr>");
            }
            sb.Append($"<tr><td style=\"padding:8px 0\"><b>Итого</b></td><td style=\"padding:8px 0;text-align:right\"><b>{Total(order).ToString("C0", Ru)}</b></td></tr></table>");
            return sb.ToString();
        }

        private static string Button(string url, string text) =>
            $"<p><a href=\"{Enc(url)}\" style=\"display:inline-block;background:#2d6a4f;color:#fff;padding:12px 22px;border-radius:10px;text-decoration:none;font-weight:bold\">{Enc(text)}</a></p>";

        private static string Layout(string title, string body) =>
            "<div style=\"font-family:Arial,sans-serif;max-width:560px;margin:auto;color:#18261f\">" +
            "<div style=\"background:#1f4d3a;color:#fff;padding:18px 24px;border-radius:14px 14px 0 0;font-size:20px;font-weight:bold\">🌿 PlantShop</div>" +
            $"<div style=\"border:1px solid #e6e2d8;border-top:none;padding:24px;border-radius:0 0 14px 14px\"><h2 style=\"margin-top:0\">{Enc(title)}</h2>{body}" +
            "<p style=\"color:#5d6b63;font-size:13px;margin-top:24px\">Питомник PlantShop · +7 (812) 925-05-08</p></div></div>";

        private static string Enc(string? s) => WebUtility.HtmlEncode(s ?? "");
    }
}
