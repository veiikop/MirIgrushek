using System.IO;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace MirIgrushek.Services
{
    /// <summary>
    /// Точка доступа к базе данных через ADO.NET.
    /// Читает строку подключения из appsettings.json и выдаёт открытые соединения.
    /// </summary>
    public static class Database
    {
        private static readonly string ConnectionString;

        // Статический конструктор: один раз загружает конфигурацию при старте.
        static Database()
        {
            IConfigurationRoot config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            ConnectionString = config.GetConnectionString("MirIgrushekDB")
                ?? throw new InvalidOperationException(
                    "Строка подключения 'MirIgrushekDB' не найдена в appsettings.json.");
        }

        /// <summary>
        /// Возвращает уже открытое соединение с базой данных.
        /// Вызывающий код обязан обернуть его в using.
        /// </summary>
        public static SqlConnection GetConnection()
        {
            SqlConnection connection = new SqlConnection(ConnectionString);
            connection.Open();
            return connection;
        }

        /// <summary>
        /// Проверяет доступность базы данных при запуске приложения.
        /// </summary>
        public static bool TestConnection(out string error)
        {
            error = string.Empty;
            try
            {
                using SqlConnection connection = GetConnection();
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }
    }
}
