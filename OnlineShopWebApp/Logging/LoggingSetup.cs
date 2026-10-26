using Serilog;
using Serilog.Events;
using System.Security.Claims;

namespace OnlineShopWebApp.Logging
{
    /// <summary>
    /// Общая настройка Serilog для сайта и Web API.
    /// Всё, что можно менять без пересборки (уровни, пути, срок хранения), задаётся в разделе "Serilog" файла appsettings.json.
    /// </summary>
    public static class LoggingSetup
    {
        private static readonly string[] StaticPrefixes = { "/lib/", "/css/", "/js/", "/img/", "/uploads/", "/favicon" };

        public static bool IsStaticFile(PathString path) =>
            StaticPrefixes.Any(p => path.StartsWithSegments(p.TrimEnd('/'), StringComparison.OrdinalIgnoreCase));

        /// <summary>
        /// Временный логгер на время запуска: ловит ошибки, которые случаются до чтения конфигурации.
        /// </summary>
        public static void CreateBootstrapLogger()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console()
                .CreateBootstrapLogger();
        }

        public static void AddAppLogging(this WebApplicationBuilder builder)
        {
            builder.Services.AddSerilog((services, configuration) => configuration
                .ReadFrom.Configuration(builder.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Application", builder.Environment.ApplicationName));
        }

        /// <summary>
        /// Добавляет Id пользователя ко всем записям журнала, сделанным во время запроса.
        /// </summary>
        public static IApplicationBuilder UseUserLogContext(this IApplicationBuilder app)
        {
            return app.Use(async (context, next) =>
            {
                var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null)
                {
                    await next();
                    return;
                }

                using (Serilog.Context.LogContext.PushProperty("UserId", userId))
                {
                    await next();
                }
            });
        }

        /// <summary>
        /// Одна строка в журнале на каждый запрос: метод, адрес, код ответа, время, пользователь и IP.
        /// Медленные запросы (дольше 2 секунд) отмечаются как предупреждение, ошибки сервера — как ошибка.
        /// </summary>
        public static IApplicationBuilder UseAppRequestLogging(this IApplicationBuilder app)
        {
            return app.UseSerilogRequestLogging(options =>
            {
                options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} → {StatusCode} за {Elapsed:0} мс";

                options.GetLevel = (context, elapsed, ex) =>
                    ex != null || context.Response.StatusCode >= 500 ? LogEventLevel.Error
                    : IsStaticFile(context.Request.Path) ? LogEventLevel.Verbose // картинки, стили и скрипты не засоряют журнал
                    : elapsed > 2000 ? LogEventLevel.Warning
                    : LogEventLevel.Information;

                options.EnrichDiagnosticContext = (diagnostic, context) =>
                {
                    diagnostic.Set("ClientIp", context.Connection.RemoteIpAddress?.ToString());
                    diagnostic.Set("QueryString", LogMask.Text(context.Request.QueryString.Value));

                    var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (userId != null) diagnostic.Set("UserId", userId);
                };
            });
        }
    }
}
