using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace OnlineShopWebApp.Services
{
    public class SmtpOptions
    {
        public const string SectionName = "Smtp";

        public string? Host { get; set; }
        public int Port { get; set; } = 587;
        public bool EnableSsl { get; set; } = true;
        public string? UserName { get; set; }
        public string? Password { get; set; }

        /// <summary>Адрес отправителя, например shop@example.com</summary>
        public string? From { get; set; }

        public string FromName { get; set; } = "PlantShop";

        /// <summary>Куда отправлять уведомления о новых заказах</summary>
        public string? ManagerEmail { get; set; }

        public bool IsConfigured => !string.IsNullOrWhiteSpace(Host) && !string.IsNullOrWhiteSpace(From);
    }

    public interface IEmailService
    {
        bool IsConfigured { get; }
        Task SendAsync(string to, string subject, string htmlBody);
    }

    /// <summary>
    /// Отправка писем через SMTP. Если SMTP не настроен — письмо записывается в лог, чтобы сайт продолжал работать.
    /// </summary>
    public class SmtpEmailService : IEmailService
    {
        private readonly SmtpOptions _options;
        private readonly ILogger<SmtpEmailService> _logger;

        public SmtpEmailService(IOptions<SmtpOptions> options, ILogger<SmtpEmailService> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        public bool IsConfigured => _options.IsConfigured;

        public async Task SendAsync(string to, string subject, string htmlBody)
        {
            if (string.IsNullOrWhiteSpace(to)) return;

            if (!_options.IsConfigured)
            {
                _logger.LogInformation("SMTP не настроен. Письмо для {To} «{Subject}» не отправлено. Текст: {Body}", to, subject, htmlBody);
                return;
            }

            try
            {
                using var message = new MailMessage
                {
                    From = new MailAddress(_options.From!, _options.FromName),
                    Subject = subject,
                    Body = htmlBody,
                    IsBodyHtml = true
                };
                message.To.Add(to);

                using var client = new SmtpClient(_options.Host, _options.Port)
                {
                    EnableSsl = _options.EnableSsl,
                    Credentials = string.IsNullOrEmpty(_options.UserName)
                        ? CredentialCache.DefaultNetworkCredentials
                        : new NetworkCredential(_options.UserName, _options.Password)
                };

                await client.SendMailAsync(message);
                _logger.LogInformation("Письмо «{Subject}» отправлено на {To}", subject, to);
            }
            catch (Exception ex)
            {
                // Ошибка почты не должна ломать оформление заказа
                _logger.LogError(ex, "Не удалось отправить письмо «{Subject}» на {To}", subject, to);
            }
        }
    }
}
