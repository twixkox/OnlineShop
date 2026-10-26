using Microsoft.Extensions.Options;
using OnlineShopWebApp.Logging;
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

        /// <summary>
        /// Папка для писем, когда SMTP не настроен (удобно при разработке): каждое письмо сохраняется файлом .eml.
        /// Раньше такие письма целиком писались в журнал — вместе со ссылками для сброса пароля.
        /// </summary>
        public string? PickupDirectory { get; set; }

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
        private readonly string? _pickupDirectory;

        public SmtpEmailService(IOptions<SmtpOptions> options, ILogger<SmtpEmailService> logger, IWebHostEnvironment environment)
        {
            _options = options.Value;
            _logger = logger;

            if (!string.IsNullOrWhiteSpace(_options.PickupDirectory))
            {
                _pickupDirectory = Path.IsPathRooted(_options.PickupDirectory)
                    ? _options.PickupDirectory
                    : Path.Combine(environment.ContentRootPath, _options.PickupDirectory);
            }
        }

        public bool IsConfigured => _options.IsConfigured;

        public async Task SendAsync(string to, string subject, string htmlBody)
        {
            if (string.IsNullOrWhiteSpace(to)) return;

            // В журнал попадают только тема и замаскированный адрес — без текста письма
            var maskedTo = LogMask.Email(to);

            if (!_options.IsConfigured && _pickupDirectory == null)
            {
                _logger.LogWarning("SMTP не настроен: письмо «{Subject}» для {To} не отправлено", subject, maskedTo);
                return;
            }

            try
            {
                using var message = new MailMessage
                {
                    From = new MailAddress(_options.IsConfigured ? _options.From! : "noreply@plantshop.local", _options.FromName),
                    Subject = subject,
                    Body = htmlBody,
                    IsBodyHtml = true
                };
                message.To.Add(to);

                using var client = _options.IsConfigured
                    ? new SmtpClient(_options.Host, _options.Port)
                    {
                        EnableSsl = _options.EnableSsl,
                        Credentials = string.IsNullOrEmpty(_options.UserName)
                            ? CredentialCache.DefaultNetworkCredentials
                            : new NetworkCredential(_options.UserName, _options.Password)
                    }
                    : new SmtpClient
                    {
                        DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory,
                        PickupDirectoryLocation = Directory.CreateDirectory(_pickupDirectory!).FullName
                    };

                await client.SendMailAsync(message);
                _logger.LogInformation(_options.IsConfigured
                    ? "Письмо «{Subject}» отправлено на {To}"
                    : "Письмо «{Subject}» для {To} сохранено в папку писем (SMTP не настроен)", subject, maskedTo);
            }
            catch (Exception ex)
            {
                // Ошибка почты не должна ломать оформление заказа
                _logger.LogError(ex, "Не удалось отправить письмо «{Subject}» на {To}", subject, maskedTo);
            }
        }
    }
}
