using OnlineShopWebApp.Logging;
using Serilog;
using Serilog.Formatting.Compact;
using System.Diagnostics;

namespace OnlineShop.Tests
{
    public class LogMaskTests
    {
        [Theory]
        [InlineData("anna.smirnova@mail.ru", "an***@mail.ru")]
        [InlineData("a@b.ru", "a***@b.ru")]
        [InlineData("  admin@gmail.com ", "ad***@gmail.com")]
        [InlineData(null, "—")]
        [InlineData("not-an-email", "no***")]
        public void Email(string? input, string expected) => Assert.Equal(expected, LogMask.Email(input));

        [Theory]
        [InlineData("+7 (921) 555-12-34", "***-**-34")]
        [InlineData("89215551234", "***-**-34")]
        [InlineData("12", "***")]
        public void Phone(string input, string expected) => Assert.Equal(expected, LogMask.Phone(input));

        [Fact]
        public void Text_HidesEmailsPhonesAndTokens()
        {
            var text = "Письмо для maria@example.com, тел. +7 (911) 222-33-44: https://shop/Authorization/ResetPassword?email=x&token=CfDJ8abc+/=";
            var masked = LogMask.Text(text);

            Assert.DoesNotContain("maria@example.com", masked);
            Assert.DoesNotContain("222-33-44", masked);
            Assert.DoesNotContain("CfDJ8abc", masked);
            Assert.Contains("ma***@example.com", masked);
            Assert.Contains("token=***", masked);
        }
    }

    public class LogReaderTests : IDisposable
    {
        private readonly string _dir = Path.Combine(Path.GetTempPath(), "ps-logs-" + Guid.NewGuid().ToString("N"));

        public LogReaderTests() => Directory.CreateDirectory(_dir);

        public void Dispose() => Directory.Delete(_dir, recursive: true);

        private LogReader Reader() => new(new LogViewerOptions { Directory = _dir }, "/");

        private void Write(string file, params string[] lines) => File.WriteAllLines(Path.Combine(_dir, file), lines);

        private static string Line(string time, string message, string? level = null, string? trace = null, string? ex = null, string? user = null) =>
            "{" + string.Join(",", new[]
            {
                $"\"@t\":\"{time}\"",
                $"\"@m\":\"{message}\"",
                level == null ? null : $"\"@l\":\"{level}\"",
                trace == null ? null : $"\"@tr\":\"{trace}\"",
                ex == null ? null : $"\"@x\":\"{ex}\"",
                user == null ? null : $"\"UserId\":\"{user}\"",
                "\"SourceContext\":\"OnlineShopWebApp.Controllers.CartController\""
            }.Where(x => x != null)) + "}";

        [Fact]
        public void Parse_ReadsCompactJson()
        {
            var entry = LogReader.Parse(Line("2026-10-26T10:00:00.1234567+03:00", "Привет", "Error", "abc", "System.Exception: boom\\n   at X"));
            Assert.NotNull(entry);
            Assert.Equal("Error", entry!.Level);
            Assert.Equal("Привет", entry.Message);
            Assert.Equal("abc", entry.TraceId);
            Assert.Equal("System.Exception: boom", entry.ExceptionTitle);
            Assert.Equal("CartController", entry.Source);
        }

        [Fact]
        public void Render_SubstitutesPropertiesWithoutQuotes()
        {
            var props = new Dictionary<string, string> { ["RequestMethod"] = "GET", ["RequestPath"] = "/catalog", ["StatusCode"] = "200" };
            Assert.Equal("HTTP GET /catalog → 200 за 15 мс {x}",
                LogReader.Render("HTTP {RequestMethod} {RequestPath} → {StatusCode} за {Elapsed:0} мс {{x}}", props, null, new[] { "15" }));
        }

        [Fact]
        public void Parse_DefaultLevelIsInformation_AndSkipsBrokenLines()
        {
            Assert.Equal("Information", LogReader.Parse(Line("2026-10-26T10:00:00Z", "ok"))!.Level);
            Assert.Null(LogReader.Parse("{\"@t\":\"2026-10-26T10:00:00Z\",\"@m\":\"обор"));
            Assert.Null(LogReader.Parse("просто текст"));
        }

        [Fact]
        public void Query_NewestFirst_FiltersByLevelTextAndUser()
        {
            Write("app-20261026.log",
                Line("2026-10-26T10:00:00Z", "Первая", "Debug"),
                Line("2026-10-26T10:01:00Z", "Заказ создан", user: "u1"),
                Line("2026-10-26T10:02:00Z", "Медленно", "Warning"),
                Line("2026-10-26T10:03:00Z", "Упало", "Error", ex: "NullReferenceException"));
            var reader = Reader();
            var day = new DateOnly(2026, 10, 26);

            var all = reader.Query(new LogQuery { Date = day, Level = LogLevelFilter.All });
            Assert.Equal(new[] { "Упало", "Медленно", "Заказ создан", "Первая" }, all.Entries.Select(e => e.Message));
            Assert.Equal(1, all.LevelCounts["Error"]);

            Assert.Equal(3, reader.Query(new LogQuery { Date = day }).TotalCount); // по умолчанию без отладки
            Assert.Equal(2, reader.Query(new LogQuery { Date = day, Level = LogLevelFilter.Warning }).TotalCount);
            Assert.Equal("Упало", reader.Query(new LogQuery { Date = day, Q = "nullreference" }).Entries.Single().Message);
            Assert.Equal("Заказ создан", reader.Query(new LogQuery { Date = day, UserId = "u1" }).Entries.Single().Message);
        }

        [Fact]
        public void Query_ByTraceId_SearchesAllDays_AndAcceptsW3cId()
        {
            var trace = "4bf92f3577b34da6a3ce929d0e0e4736";
            Write("app-20261025.log", Line("2026-10-25T09:00:00Z", "Вчерашняя ошибка", "Error", trace), Line("2026-10-25T09:00:01Z", "Другая", trace: "ffff"));
            Write("app-20261026.log", Line("2026-10-26T10:00:00Z", "Сегодня"));
            var reader = Reader();

            var result = reader.Query(new LogQuery { TraceId = $"00-{trace}-00f067aa0ba902b7-01", Level = LogLevelFilter.All });
            Assert.Equal("Вчерашняя ошибка", result.Entries.Single().Message);
        }

        [Fact]
        public void Files_SplitBySize_AreReadInOrder()
        {
            Write("app-20261026.log", Line("2026-10-26T10:00:00Z", "1"));
            Write("app-20261026_001.log", Line("2026-10-26T11:00:00Z", "2"));
            Write("errors-202610.log", Line("2026-10-26T11:00:00Z", "не журнал дня", "Error"));
            var reader = Reader();

            Assert.Equal(new[] { new DateOnly(2026, 10, 26) }, reader.GetDays());
            Assert.Equal(new[] { "1", "2" }, reader.ReadDay(new DateOnly(2026, 10, 26)).Select(e => e.Message));

            var download = reader.OpenDownload(new DateOnly(2026, 10, 26));
            Assert.Equal("application/zip", download!.Value.ContentType);
            download.Value.Stream.Dispose();
        }

        [Fact]
        public void Paging_IsClamped()
        {
            Write("app-20261026.log", Enumerable.Range(0, 25).Select(i => Line($"2026-10-26T10:{i:00}:00Z", $"m{i}")).ToArray());
            var result = Reader().Query(new LogQuery { Date = new DateOnly(2026, 10, 26), PageSize = 10, Page = 99 });

            Assert.Equal(3, result.TotalPages);
            Assert.Equal(3, result.Page);
            Assert.Equal(new[] { "m4", "m3", "m2", "m1", "m0" }, result.Entries.Select(e => e.Message));
        }

        [Fact]
        public void RealSerilogOutput_IsReadable_WithTraceId()
        {
            var path = Path.Combine(_dir, "app-.log");
            using (var logger = new LoggerConfiguration()
                       .WriteTo.File(new CompactJsonFormatter(), path, rollingInterval: RollingInterval.Day, shared: true)
                       .CreateLogger())
            {
                using var activity = new Activity("test").Start();
                logger.ForContext("SourceContext", "Tests").Warning("Заказ {OrderId} для {User} за {Elapsed:0} мс", 42, LogMask.Email("maria@example.com"), 12.345);
                logger.Error(new InvalidOperationException("boom"), "Ошибка");
            }

            var reader = Reader();
            var entries = reader.ReadDay(reader.GetDays().Single()).ToList();

            Assert.Equal("Заказ 42 для ma***@example.com за 12 мс", entries[0].Message);
            Assert.Equal("Warning", entries[0].Level);
            Assert.Equal(32, entries[0].TraceId!.Length);
            Assert.Equal("42", entries[0].Properties["OrderId"]);
            Assert.StartsWith("System.InvalidOperationException: boom", entries[1].Exception);
        }
    }
}
