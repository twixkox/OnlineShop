using Microsoft.Extensions.Options;
using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OnlineShopWebApp.Services
{
    public class YooKassaOptions
    {
        public const string SectionName = "YooKassa";

        public string? ShopId { get; set; }
        public string? SecretKey { get; set; }
        public string ApiUrl { get; set; } = "https://api.yookassa.ru/v3/";

        public bool IsConfigured => !string.IsNullOrWhiteSpace(ShopId) && !string.IsNullOrWhiteSpace(SecretKey);
    }

    public record PaymentCreated(string PaymentId, string ConfirmationUrl);

    public interface IPaymentService
    {
        bool IsEnabled { get; }

        /// <summary>Создаёт платёж и возвращает ссылку на страницу оплаты.</summary>
        Task<PaymentCreated> CreatePaymentAsync(Guid orderId, decimal amount, string description, string returnUrl);

        /// <summary>Статус платежа по данным ЮKassa: pending, waiting_for_capture, succeeded, canceled.</summary>
        Task<string?> GetPaymentStatusAsync(string paymentId);
    }

    /// <summary>
    /// Интеграция с ЮKassa (https://yookassa.ru/developers/api).
    /// Платёж создаётся с автоматическим подтверждением (capture = true) и редиректом на страницу оплаты.
    /// </summary>
    public class YooKassaPaymentService : IPaymentService
    {
        private readonly HttpClient _http;
        private readonly YooKassaOptions _options;
        private readonly ILogger<YooKassaPaymentService> _logger;

        public YooKassaPaymentService(HttpClient http, IOptions<YooKassaOptions> options, ILogger<YooKassaPaymentService> logger)
        {
            _http = http;
            _options = options.Value;
            _logger = logger;

            if (_options.IsConfigured)
            {
                _http.BaseAddress = new Uri(_options.ApiUrl);
                var token = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_options.ShopId}:{_options.SecretKey}"));
                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", token);
            }
        }

        public bool IsEnabled => _options.IsConfigured;

        public static object BuildCreateRequest(Guid orderId, decimal amount, string description, string returnUrl) => new
        {
            amount = new { value = amount.ToString("0.00", CultureInfo.InvariantCulture), currency = "RUB" },
            capture = true,
            confirmation = new { type = "redirect", return_url = returnUrl },
            description = description.Length > 128 ? description[..128] : description,
            metadata = new { orderId = orderId.ToString() }
        };

        public async Task<PaymentCreated> CreatePaymentAsync(Guid orderId, decimal amount, string description, string returnUrl)
        {
            if (!IsEnabled) throw new InvalidOperationException("ЮKassa не настроена");

            using var request = new HttpRequestMessage(HttpMethod.Post, "payments")
            {
                Content = JsonContent.Create(BuildCreateRequest(orderId, amount, description, returnUrl))
            };
            // Ключ идемпотентности: повторный запрос не создаст второй платёж
            request.Headers.Add("Idempotence-Key", $"{orderId:N}-{(long)(amount * 100)}");

            var response = await _http.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("ЮKassa: ошибка создания платежа {Status}: {Body}", (int)response.StatusCode, body);
                throw new InvalidOperationException("Не удалось создать платёж");
            }

            var payment = JsonSerializer.Deserialize<YooPayment>(body)!;
            return new PaymentCreated(payment.Id, payment.Confirmation?.ConfirmationUrl ?? "");
        }

        public async Task<string?> GetPaymentStatusAsync(string paymentId)
        {
            if (!IsEnabled || string.IsNullOrWhiteSpace(paymentId)) return null;

            var response = await _http.GetAsync($"payments/{Uri.EscapeDataString(paymentId)}");
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("ЮKassa: не удалось получить платёж {PaymentId}: {Status}", paymentId, (int)response.StatusCode);
                return null;
            }

            var payment = await response.Content.ReadFromJsonAsync<YooPayment>();
            return payment?.Status;
        }

        private class YooPayment
        {
            [JsonPropertyName("id")] public string Id { get; set; } = "";
            [JsonPropertyName("status")] public string Status { get; set; } = "";
            [JsonPropertyName("confirmation")] public YooConfirmation? Confirmation { get; set; }
        }

        private class YooConfirmation
        {
            [JsonPropertyName("confirmation_url")] public string? ConfirmationUrl { get; set; }
        }
    }
}
