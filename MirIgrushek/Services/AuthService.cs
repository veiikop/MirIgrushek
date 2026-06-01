using Microsoft.Data.SqlClient;
using MirIgrushek.Models;

namespace MirIgrushek.Services
{
    /// <summary>
    /// Сервис авторизации: проверяет логин/пароль по таблице Users.
    /// </summary>
    public static class AuthService
    {
        /// <summary>
        /// Пытается авторизовать пользователя.
        /// Возвращает объект User при успехе или null, если пара логин/пароль неверна.
        /// </summary>
        public static User? Authenticate(string login, string password)
        {
            const string sql = @"
                SELECT u.UserId, u.RoleId, r.RoleName, u.FullName, u.Login, u.Password
                FROM dbo.Users u
                INNER JOIN dbo.Roles r ON r.RoleId = u.RoleId
                WHERE u.Login = @login AND u.Password = @password;";

            using SqlConnection connection = Database.GetConnection();
            using SqlCommand command = new SqlCommand(sql, connection);
            // Параметризованный запрос защищает от SQL-инъекций.
            command.Parameters.AddWithValue("@login", login);
            command.Parameters.AddWithValue("@password", password);

            using SqlDataReader reader = command.ExecuteReader();
            if (reader.Read())
            {
                return new User
                {
                    UserId = reader.GetInt32(0),
                    RoleId = reader.GetInt32(1),
                    RoleName = reader.GetString(2),
                    FullName = reader.GetString(3),
                    Login = reader.GetString(4),
                    Password = reader.GetString(5)
                };
            }

            return null;
        }
    }
}
