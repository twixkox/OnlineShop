using Microsoft.Data.SqlClient;

namespace OnlineShopWebApp.Helpers
{
    /// <summary>Описание строки подключения для журнала — без пароля.</summary>
    public static class DatabaseInfo
    {
        public static string Describe(string connectionString)
        {
            try
            {
                var b = new SqlConnectionStringBuilder(connectionString);
                var login = b.IntegratedSecurity ? "вход Windows" : $"логин {b.UserID}";
                return $"сервер «{b.DataSource}», база «{b.InitialCatalog}», {login}";
            }
            catch (ArgumentException)
            {
                return "строка подключения не распознана";
            }
        }

        public static bool IsLocalDb(string connectionString) =>
            connectionString.Contains("(localdb)", StringComparison.OrdinalIgnoreCase);
    }
}
