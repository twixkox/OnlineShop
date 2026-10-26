using System.Text.RegularExpressions;

namespace OnlineShopWebApp.Logging
{
    /// <summary>
    /// Маскировка персональных данных перед записью в журнал.
    /// В логе должно быть достаточно, чтобы понять, о ком речь, но не полный email или телефон.
    /// </summary>
    public static partial class LogMask
    {
        /// <summary>anna.smirnova@mail.ru → an***@mail.ru</summary>
        public static string Email(string? email)
        {
            if (string.IsNullOrWhiteSpace(email)) return "—";

            var value = email.Trim();
            var at = value.IndexOf('@');
            if (at <= 0) return Mask(value);

            var local = value[..at];
            var visible = local.Length <= 2 ? local[..1] : local[..2];
            return $"{visible}***{value[at..]}";
        }

        /// <summary>+7 (921) 555-12-34 → ***-**-34</summary>
        public static string Phone(string? phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return "—";

            var digits = new string(phone.Where(char.IsDigit).ToArray());
            return digits.Length < 4 ? "***" : $"***-**-{digits[^2..]}";
        }

        /// <summary>Скрывает в произвольном тексте email-адреса, телефоны и ссылки с токенами.</summary>
        public static string Text(string? text)
        {
            if (string.IsNullOrEmpty(text)) return text ?? "";

            var result = EmailRegex().Replace(text, m => Email(m.Value));
            result = TokenRegex().Replace(result, m => m.Groups[1].Value + "=***");
            result = PhoneRegex().Replace(result, m => Phone(m.Value));
            return result;
        }

        private static string Mask(string value) => value.Length <= 2 ? "***" : value[..2] + "***";

        [GeneratedRegex(@"[A-Za-z0-9._%+\-]+@[A-Za-z0-9.\-]+\.[A-Za-z]{2,}")]
        private static partial Regex EmailRegex();

        [GeneratedRegex(@"\b(token|code|key|password|secret)=[^&\s""'<]+", RegexOptions.IgnoreCase)]
        private static partial Regex TokenRegex();

        [GeneratedRegex(@"(?<!\d)(?:\+7|8)[\s\-(]*\d{3}[\s\-)]*\d{3}[\s\-]*\d{2}[\s\-]*\d{2}(?!\d)")]
        private static partial Regex PhoneRegex();
    }
}
