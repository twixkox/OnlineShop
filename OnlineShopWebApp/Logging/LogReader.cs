using Microsoft.Extensions.Options;
using System.Globalization;
using System.IO.Compression;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace OnlineShopWebApp.Logging
{
    public class LogViewerOptions
    {
        public const string SectionName = "LogViewer";

        /// <summary>Папка с файлами журнала (относительно папки сайта или абсолютный путь)</summary>
        public string Directory { get; set; } = "Logs";

        /// <summary>Префикс ежедневных файлов: app-20261026.log</summary>
        public string FilePrefix { get; set; } = "app";

        /// <summary>Сколько последних записей держать в памяти при поиске по большому файлу</summary>
        public int MaxEntries { get; set; } = 20000;
    }

    public enum LogLevelFilter
    {
        All = 0,
        Debug = 1,
        Information = 2,
        Warning = 3,
        Error = 4
    }

    public class LogEntry
    {
        public int Line { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public string Level { get; set; } = "Information";
        public string Message { get; set; } = "";
        public string? Exception { get; set; }
        public string? TraceId { get; set; }
        public string? SourceContext { get; set; }
        public Dictionary<string, string> Properties { get; set; } = new();

        public int LevelRank => Rank(Level);

        public string Source => SourceContext == null ? "" : SourceContext[(SourceContext.LastIndexOf('.') + 1)..];

        /// <summary>Первая строка исключения: тип и сообщение</summary>
        public string? ExceptionTitle => Exception?.Split('\n', 2)[0].Trim();

        public static int Rank(string level) => level switch
        {
            "Verbose" => 0,
            "Debug" => 1,
            "Information" => 2,
            "Warning" => 3,
            "Error" => 4,
            "Fatal" => 5,
            _ => 2
        };
    }

    public class LogQuery
    {
        public DateOnly? Date { get; set; }
        public LogLevelFilter Level { get; set; } = LogLevelFilter.Information;
        public string? Q { get; set; }
        public string? TraceId { get; set; }
        public string? UserId { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 100;
    }

    public class LogQueryResult
    {
        public List<LogEntry> Entries { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int TotalPages { get; set; }
        public Dictionary<string, int> LevelCounts { get; set; } = new();
        public List<DateOnly> SearchedDays { get; set; } = new();
        public bool Truncated { get; set; }
    }

    public record LogFileInfo(DateOnly Date, string Name, long Size);

    /// <summary>
    /// Читает журнал в формате Compact JSON (одна запись — одна строка), который пишет Serilog.
    /// Файлы открываются на чтение без блокировки, поэтому сайт продолжает писать в них.
    /// </summary>
    public partial class LogReader
    {
        private readonly LogViewerOptions _options;
        private readonly string _directory;

        public LogReader(IOptions<LogViewerOptions> options, IWebHostEnvironment environment)
            : this(options.Value, environment.ContentRootPath)
        {
        }

        public LogReader(LogViewerOptions options, string contentRoot)
        {
            _options = options;
            _directory = Path.IsPathRooted(options.Directory) ? options.Directory : Path.Combine(contentRoot, options.Directory);
        }

        public string DirectoryPath => _directory;

        /// <summary>Файлы журнала по дням, свежие сверху.</summary>
        public List<LogFileInfo> GetFiles()
        {
            if (!System.IO.Directory.Exists(_directory)) return new();

            var pattern = FileNameRegex();
            return new DirectoryInfo(_directory)
                .EnumerateFiles($"{_options.FilePrefix}-*.log")
                .Select(f => (File: f, Match: pattern.Match(f.Name)))
                .Where(x => x.Match.Success && x.Match.Groups["prefix"].Value == _options.FilePrefix)
                .Select(x => new LogFileInfo(
                    DateOnly.ParseExact(x.Match.Groups["date"].Value, "yyyyMMdd", CultureInfo.InvariantCulture),
                    x.File.Name,
                    x.File.Length))
                .OrderByDescending(f => f.Date).ThenByDescending(f => f.Name, StringComparer.Ordinal)
                .ToList();
        }

        public List<DateOnly> GetDays() => GetFiles().Select(f => f.Date).Distinct().ToList();

        public long TotalSize() => GetFiles().Sum(f => f.Size);

        public LogQueryResult Query(LogQuery query)
        {
            var days = GetDays();
            var result = new LogQueryResult();

            // Поиск по коду запроса без даты — ищем по всем дням, начиная с последнего
            IEnumerable<DateOnly> targetDays = query.Date.HasValue
                ? new[] { query.Date.Value }
                : !string.IsNullOrWhiteSpace(query.TraceId)
                    ? days
                    : days.Take(1);

            var matches = new List<LogEntry>();
            var minRank = (int)query.Level == 0 ? 0 : (int)query.Level;
            var text = query.Q?.Trim();
            var traceId = NormalizeTraceId(query.TraceId);

            foreach (var day in targetDays)
            {
                result.SearchedDays.Add(day);
                foreach (var entry in ReadDay(day))
                {
                    // Счётчики по уровням — с учётом текстовых фильтров, но без фильтра уровня
                    if (!MatchesText(entry, text, traceId, query.UserId)) continue;

                    result.LevelCounts[entry.Level] = result.LevelCounts.GetValueOrDefault(entry.Level) + 1;

                    if (entry.LevelRank < minRank) continue;

                    matches.Add(entry);
                    if (matches.Count > _options.MaxEntries)
                    {
                        matches.RemoveRange(0, matches.Count - _options.MaxEntries);
                        result.Truncated = true;
                    }
                }

                // Запрос обычно укладывается в один день — дальше не ищем
                if (traceId != null && matches.Count > 0) break;
            }

            matches.Reverse(); // свежие сверху

            var pageSize = Math.Clamp(query.PageSize, 10, 500);
            result.TotalCount = matches.Count;
            result.TotalPages = Math.Max(1, (int)Math.Ceiling(matches.Count / (double)pageSize));
            result.Page = Math.Clamp(query.Page, 1, result.TotalPages);
            result.Entries = matches.Skip((result.Page - 1) * pageSize).Take(pageSize).ToList();
            return result;
        }

        /// <summary>Сколько ошибок и предупреждений было за последние часы (для сводки в админке).</summary>
        public (int Errors, int Warnings, LogEntry? Last) CountProblems(TimeSpan period)
        {
            var since = DateTimeOffset.Now - period;
            int errors = 0, warnings = 0;
            LogEntry? last = null;

            foreach (var day in GetDays().Where(d => d.ToDateTime(TimeOnly.MaxValue) >= since.LocalDateTime))
            {
                foreach (var entry in ReadDay(day))
                {
                    if (entry.Timestamp < since || entry.LevelRank < 3) continue;
                    if (entry.LevelRank >= 4)
                    {
                        errors++;
                        // Для одного запроса пишутся две записи: строка запроса и само исключение.
                        // Показываем ту, где есть исключение — по ней понятнее, что сломалось
                        var sameRequest = last != null && entry.TraceId != null && entry.TraceId == last.TraceId;
                        if (last == null || (sameRequest ? entry.Exception != null : entry.Timestamp > last.Timestamp)) last = entry;
                    }
                    else warnings++;
                }
            }

            return (errors, warnings, last);
        }

        /// <summary>Файл(ы) за день для скачивания: один файл как есть, несколько — архивом.</summary>
        public (Stream Stream, string FileName, string ContentType)? OpenDownload(DateOnly day)
        {
            var files = GetFiles().Where(f => f.Date == day).OrderBy(f => f.Name, StringComparer.Ordinal).ToList();
            if (files.Count == 0) return null;

            if (files.Count == 1)
            {
                return (OpenShared(Path.Combine(_directory, files[0].Name)), files[0].Name, "application/x-ndjson");
            }

            var zip = new MemoryStream();
            using (var archive = new ZipArchive(zip, ZipArchiveMode.Create, leaveOpen: true))
            {
                foreach (var file in files)
                {
                    var item = archive.CreateEntry(file.Name, CompressionLevel.Fastest);
                    using var target = item.Open();
                    using var source = OpenShared(Path.Combine(_directory, file.Name));
                    source.CopyTo(target);
                }
            }
            zip.Position = 0;
            return (zip, $"{_options.FilePrefix}-{day:yyyyMMdd}.zip", "application/zip");
        }

        public IEnumerable<LogEntry> ReadDay(DateOnly day)
        {
            // Один день может состоять из нескольких файлов (app-20261026.log, app-20261026_001.log ...)
            foreach (var file in GetFiles().Where(f => f.Date == day).OrderBy(f => f.Name, StringComparer.Ordinal))
            {
                foreach (var entry in ReadFile(Path.Combine(_directory, file.Name)))
                {
                    yield return entry;
                }
            }
        }

        public static IEnumerable<LogEntry> ReadFile(string path)
        {
            using var stream = OpenShared(path);
            using var reader = new StreamReader(stream);
            var lineNo = 0;
            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                lineNo++;
                var entry = Parse(line, lineNo);
                if (entry != null) yield return entry;
            }
        }

        /// <summary>Разбирает одну строку формата CLEF (Compact Log Event Format).</summary>
        public static LogEntry? Parse(string line, int lineNo = 0)
        {
            if (string.IsNullOrWhiteSpace(line) || line[0] != '{') return null;

            try
            {
                using var doc = JsonDocument.Parse(line);
                var root = doc.RootElement;
                var entry = new LogEntry { Line = lineNo };
                string? template = null;
                var renderings = new List<string>();

                foreach (var prop in root.EnumerateObject())
                {
                    switch (prop.Name)
                    {
                        case "@t":
                            entry.Timestamp = DateTimeOffset.Parse(prop.Value.GetString()!, CultureInfo.InvariantCulture).ToLocalTime();
                            break;
                        case "@m":
                            entry.Message = prop.Value.GetString() ?? "";
                            break;
                        case "@mt":
                            template = prop.Value.GetString();
                            break;
                        case "@r" when prop.Value.ValueKind == JsonValueKind.Array:
                            renderings.AddRange(prop.Value.EnumerateArray().Select(r => r.GetString() ?? ""));
                            break;
                        case "@l":
                            entry.Level = prop.Value.GetString() ?? "Information";
                            break;
                        case "@x":
                            entry.Exception = prop.Value.GetString();
                            break;
                        case "@tr":
                            entry.TraceId = prop.Value.GetString();
                            break;
                        case "SourceContext":
                            entry.SourceContext = prop.Value.GetString();
                            break;
                        case "@i" or "@sp":
                            break;
                        default:
                            entry.Properties[prop.Name] = prop.Value.ValueKind == JsonValueKind.String
                                ? prop.Value.GetString() ?? ""
                                : prop.Value.GetRawText();
                            break;
                    }
                }

                if (string.IsNullOrEmpty(entry.Message) && template != null)
                {
                    entry.Message = Render(template, entry.Properties, entry.SourceContext, renderings);
                }

                return entry;
            }
            catch (JsonException)
            {
                // Строка оборвана (файл дописывается прямо сейчас) — пропускаем
                return null;
            }
        }

        /// <summary>
        /// Собирает текст сообщения из шаблона и свойств (формат CompactJsonFormatter):
        /// "Заказ {OrderId} создан" + {"OrderId": 42} → "Заказ 42 создан". Строки — без кавычек.
        /// Значения с форматом ({Elapsed:0}) берутся из готовых "@r".
        /// </summary>
        public static string Render(string template, IReadOnlyDictionary<string, string> properties, string? sourceContext = null, IReadOnlyList<string>? renderings = null)
        {
            var renderIndex = 0;
            return TokenRegex().Replace(template, m =>
            {
                if (m.Value == "{{") return "{";
                if (m.Value == "}}") return "}";

                var name = m.Groups["name"].Value;
                if (m.Groups["format"].Success && renderings != null && renderIndex < renderings.Count)
                {
                    return renderings[renderIndex++];
                }

                if (name == "SourceContext" && sourceContext != null) return sourceContext;
                return properties.TryGetValue(name, out var value) ? value : m.Value;
            });
        }

        private static bool MatchesText(LogEntry entry, string? text, string? traceId, string? userId)
        {
            if (traceId != null && !string.Equals(entry.TraceId, traceId, StringComparison.OrdinalIgnoreCase)) return false;

            if (!string.IsNullOrEmpty(userId) && entry.Properties.GetValueOrDefault("UserId") != userId) return false;

            if (string.IsNullOrEmpty(text)) return true;

            return entry.Message.Contains(text, StringComparison.OrdinalIgnoreCase)
                   || (entry.Exception?.Contains(text, StringComparison.OrdinalIgnoreCase) ?? false)
                   || (entry.SourceContext?.Contains(text, StringComparison.OrdinalIgnoreCase) ?? false)
                   || entry.Properties.Values.Any(v => v.Contains(text, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// На странице ошибки показывается TraceId; на всякий случай принимаем и полный W3C-идентификатор
        /// вида 00-{traceId}-{spanId}-01.
        /// </summary>
        public static string? NormalizeTraceId(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;

            var v = value.Trim();
            var parts = v.Split('-');
            return parts.Length == 4 && parts[1].Length == 32 ? parts[1] : v;
        }

        private static FileStream OpenShared(string path) =>
            new(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);

        [GeneratedRegex(@"\{\{|\}\}|\{[@$]?(?<name>[A-Za-z0-9_]+)(,-?\d+)?(?<format>:[^}]+)?\}")]
        private static partial Regex TokenRegex();

        [GeneratedRegex(@"^(?<prefix>.+)-(?<date>\d{8})(_\d+)?\.log$")]
        private static partial Regex FileNameRegex();
    }
}
